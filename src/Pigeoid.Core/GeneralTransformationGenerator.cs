using System;
using System.Linq;
using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Ogc;
using Pigeoid.Transformation;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class GeneralTransformationGenerator : ITransformationGenerator<ICrs>
	{

		[Obsolete("I feel like I should already have a class or at least interface for this concept...")]
		private class Step
		{
			public ICrs From { get; set; }
			public ICrs To { get; set; }
			public ICoordinateOperationInfo OpInfo { get; set; }
			public ITransformation Tx { get; set; }
		}

		public ITransformation Generate(ICrs from, ICrs to) {
			var steps = GenerateCore(from, to);
			throw new NotImplementedException();
		}

		private IEnumerable<Step> GenerateCore(ICrs from, ICrs to) {
			if(from is ICrsGeodetic && to is ICrsGeodetic) {
				return GenerateCoreProjectedLevel(from as ICrsGeodetic, to as ICrsGeodetic);
			}
			return null;
		}

		private IEnumerable<Step> GenerateCoreProjectedLevel(ICrsGeodetic from, ICrsGeodetic to) {
			var result = new List<Step>();
			
			// undo the from projection
			var unProjectedFrom = from;
			while(unProjectedFrom is ICrsProjected) {
				var projected = unProjectedFrom as ICrsProjected;
				var projectedBase = projected.BaseCrs;
				result.Add(new Step {
					From = projected,
					To = projectedBase,
					OpInfo = projected.Projection.GetInverse()
				});
				unProjectedFrom = projectedBase;
			}

			// undo the to projection
			var backDownToProjectionSteps = new Stack<Step>();
			var unProjectedTo = to;
			while(unProjectedTo is ICrsProjected) {
				var projected = unProjectedTo as ICrsProjected;
				var projectedBase = projected.BaseCrs;
				backDownToProjectionSteps.Push(new Step {
					From = projectedBase,
					To = projected,
					OpInfo = projected.Projection
				});
				unProjectedTo = projectedBase;
			}

			// now we need to find a path between unProjectedFrom and unProjectedTo
			var datumShiftOperation = GenerateCoreDatumShift(unProjectedFrom, unProjectedTo);
			if (null == datumShiftOperation)
				return null;

			result.AddRange(datumShiftOperation);

			result.AddRange(backDownToProjectionSteps);
			return result;
			

		}

		private IEnumerable<Step> GenerateCoreDatumShift(ICrsGeodetic from, ICrsGeodetic to) {
			if(from is ICrsGeographic) {
				if (to is ICrsGeographic)
					return GenerateCoreDatumShiftGeographic(from as ICrsGeographic, to as ICrsGeographic);
				if (to is ICrsGeocentric)
					return GenerateCoreDatumShift(from as ICrsGeographic, to as ICrsGeocentric);
			}
			else if(from is ICrsGeocentric) {
				if (to is ICrsGeographic)
					return GenerateCoreDatumShift(from as ICrsGeocentric, to as ICrsGeographic);
				if (to is ICrsGeocentric)
					return GenerateCoreDatumShiftGeocentric(from as ICrsGeocentric, to as ICrsGeocentric);
			}
			return null;
		}

		private ICoordinateOperationInfo GenerateConcatenated(params ICoordinateOperationInfo[] operations) {
			var opList = operations.Where(x => null != x).ToArray();
			if (opList.Length == 0)
				return null;
			if (opList.Length == 1)
				return opList[0];
			return new ConcatenatedCoordinateOperationInfo(opList);
		}

		private IEnumerable<Step> GenerateCoreDatumShiftGeocentric(ICrsGeocentric from, ICrsGeocentric to) {
			var shift = GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
			if (null != shift) {
				yield return new Step {
					From = from,
					To = to,
					OpInfo = shift
				};
			}
		}

		private ICoordinateOperationInfo GenerateCoreDatumShiftGeocentric(IDatumGeodetic from, IDatumGeodetic to) {

			var operations = new List<ICoordinateOperationInfo>();

			var fromTransform = from.IsTransformableToWgs84 ? from.BasicWgs84Transformation : null;
			var toTransform = to.IsTransformableToWgs84 ? to.BasicWgs84Transformation : null;
			var performWgs84Transform = null != fromTransform
				&& null != toTransform
				&& fromTransform.Equals(toTransform);

			if(performWgs84Transform && null != fromTransform) {
				operations.Add(fromTransform);
			}

			var fromUnit = from.Spheroid.AxisUnit;
			var toUnit = to.Spheroid.AxisUnit;
			if(null != fromUnit && null != toUnit) {
				var conversion = fromUnit.GetConversionTo(toUnit);
				if(null != conversion) {
					if(conversion is UomUnityConversion) {
						;
					}
					else if(conversion is IUomScalarConversion<double>) {
						throw new NotImplementedException("scalar unit conversion");
					}
					else {
						throw new NotImplementedException("dunno what to do about this");
					}
				}
			}

			if(performWgs84Transform && null != toTransform) {
				operations.Add(((ICoordinateOperationInfo)toTransform).GetInverse());
			}

			if (operations.Count == 0)
				return null;
			if (operations.Count == 1)
				return operations[0];
			return new ConcatenatedCoordinateOperationInfo(operations);
		} 

		private IEnumerable<Step> GenerateCoreDatumShiftGeographic(ICrsGeographic from, ICrsGeographic to) {
			yield return new Step {
				From = from,
				To = to,
				OpInfo = GenerateConcatenated(
					new GeographicGeocentricTransformation(from.Datum.Spheroid),
					GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum),
					new GeocentricGeographicTransformation(to.Datum.Spheroid)
				)
			};
		}

		private IEnumerable<Step> GenerateCoreDatumShift(ICrsGeocentric from, ICrsGeographic to) {
			yield return new Step {
				From = from,
				To = to,
				OpInfo = GenerateConcatenated(
					GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum),
					new GeocentricGeographicTransformation(to.Datum.Spheroid)
				)
			};
		}

		private IEnumerable<Step> GenerateCoreDatumShift(ICrsGeographic from, ICrsGeocentric to) {
			yield return new Step {
				From = from,
				To = to,
				OpInfo = GenerateConcatenated(
					new GeographicGeocentricTransformation(from.Datum.Spheroid),
					GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum)
				)
			};
		}

	}
}
