using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Transformation;

namespace Pigeoid
{
	public class GeneralCrsCoordinateOperationPathGenerator : ICoordinateOperationPathGenerator<ICrs>
	{

		private static ICoordinateOperationInfo GenerateConcatenated(List<ICoordinateOperationInfo> operations) {
			if (null == operations)
				return null;

			operations.RemoveAll(x => null == x);

			if (operations.Count == 0)
				return null;
			if (operations.Count == 1)
				return operations[0];
			return new ConcatenatedCoordinateOperationInfo(operations);
		}

		public ICoordinateOperationInfo Generate(ICrs from, ICrs to) {
			return GenerateConcatenated(GenerateCore(from, to));
		}

		private List<ICoordinateOperationInfo> GenerateCore(ICrs from, ICrs to) {
			if(from is ICrsGeodetic && to is ICrsGeodetic) {
				return GenerateCoreProjectedLevel(from as ICrsGeodetic, to as ICrsGeodetic);
			}
			return null;
		}

		private List<ICoordinateOperationInfo> GenerateCoreProjectedLevel(ICrsGeodetic from, ICrsGeodetic to) {
			var result = new List<ICoordinateOperationInfo>();

			// undo the from projection
			var unProjectedFrom = from;
			while(unProjectedFrom is ICrsProjected) {
				var projected = unProjectedFrom as ICrsProjected;
				var projectedBase = projected.BaseCrs;
				result.Add(projected.Projection.GetInverse());
				unProjectedFrom = projectedBase;
			}

			// undo the to projection
			var backDownToProjectionSteps = new Stack<ICoordinateOperationInfo>();
			var unProjectedTo = to;
			while(unProjectedTo is ICrsProjected) {
				var projected = unProjectedTo as ICrsProjected;
				var projectedBase = projected.BaseCrs;
				backDownToProjectionSteps.Push(projected.Projection);
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

		[ContractAnnotation("=>canbenull")]
		private List<ICoordinateOperationInfo> GenerateCoreDatumShift(ICrsGeodetic from, ICrsGeodetic to) {
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

		private List<ICoordinateOperationInfo> GenerateCoreDatumShiftGeocentric(ICrsGeocentric from, ICrsGeocentric to) {
			return GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
		}

		private List<ICoordinateOperationInfo> GenerateCoreDatumShiftGeocentric(IDatumGeodetic from, IDatumGeodetic to) {

			var operations = new List<ICoordinateOperationInfo>();

			var fromTransform = from.IsTransformableToWgs84 ? from.BasicWgs84Transformation : null;
			var toTransform = to.IsTransformableToWgs84 ? to.BasicWgs84Transformation : null;
			var performWgs84Transform = null != fromTransform
				&& null != toTransform
				&& !fromTransform.Equals(toTransform);

			if(performWgs84Transform && null != fromTransform) {
				operations.Add(fromTransform);
			}

			var fromUnit = from.Spheroid.AxisUnit;
			var toUnit = to.Spheroid.AxisUnit;
			if(null != fromUnit && null != toUnit && fromUnit != toUnit) {
				var conversion = fromUnit.GetConversionTo(toUnit);
				if(null != conversion) {
					if(conversion is UomUnityConversion) {
						; // do nothing
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

			return operations;
		}

		private List<ICoordinateOperationInfo> GenerateCoreDatumShiftGeographic(ICrsGeographic from, ICrsGeographic to){
			// ReSharper disable CompareOfFloatsByEqualityOperator
			var operations = GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
			var fromSpheroid = from.Datum.Spheroid;
			var toSpheroid = to.Datum.Spheroid;

			if (null != fromSpheroid && null != toSpheroid && (operations.Count != 0 || fromSpheroid.A != toSpheroid.A || fromSpheroid.B != toSpheroid.B)){
				operations.Insert(0, new GeographicGeocentricTransformation(fromSpheroid));
				operations.Add(new GeocentricGeographicTransformation(toSpheroid));
			}
			return operations;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		private List<ICoordinateOperationInfo> GenerateCoreDatumShift(ICrsGeocentric from, ICrsGeographic to){
			var operations = GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
			operations.Add(new GeocentricGeographicTransformation(to.Datum.Spheroid));
			return operations;
		}

		private List<ICoordinateOperationInfo> GenerateCoreDatumShift(ICrsGeographic from, ICrsGeocentric to){
			var operations = new List<ICoordinateOperationInfo>{
				new GeographicGeocentricTransformation(from.Datum.Spheroid)
			};
			operations.AddRange(GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum));
			return operations;
		}

	}
}
