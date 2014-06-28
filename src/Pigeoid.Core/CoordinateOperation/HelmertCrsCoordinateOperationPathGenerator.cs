using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Unit;

namespace Pigeoid.CoordinateOperation
{
    public class HelmertCrsCoordinateOperationPathGenerator : ICoordinateOperationPathGenerator<ICrs>
    {


        public ICoordinateOperationCrsPathInfo Generate(ICrs from, ICrs to) {
            //return GenerateConcatenated(GenerateCore(from, to));
            var pathInfo = GenerateCore(from, to);

            throw new NotImplementedException();
        }

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

        private ICoordinateOperationCrsPathInfo GenerateCore(ICrs from, ICrs to) {
            if (from is ICrsGeodetic && to is ICrsGeodetic) {
                return GenerateCoreProjectedLevel(from as ICrsGeodetic, to as ICrsGeodetic);
            }
            return null;
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreProjectedLevel(ICrsGeodetic from, ICrsGeodetic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            var nodes = new List<ICrs>();
            var operations = new List<ICoordinateOperationInfo>();

            nodes.Add(from);

            // undo the from projection
            var unProjectedFrom = from;
            while (unProjectedFrom is ICrsProjected) {
                var projected = unProjectedFrom as ICrsProjected;
                var projectedBase = projected.BaseCrs;
                operations.Add(projected.Projection.GetInverse());
                nodes.Add(projectedBase);
                unProjectedFrom = projectedBase;
            }

            // undo the to projection
            var backDownToProjectionSteps = new Stack<ICoordinateOperationInfo>();
            var backDownToProjectionNodes = new Stack<ICrs>();
            var unProjectedTo = to;
            while (unProjectedTo is ICrsProjected) {
                var projected = unProjectedTo as ICrsProjected;
                var projectedBase = projected.BaseCrs;
                backDownToProjectionSteps.Push(projected.Projection);
                backDownToProjectionNodes.Push(projected);
                unProjectedTo = projectedBase;
            }

            // now we need to find a path between unProjectedFrom and unProjectedTo
            var datumShiftOperation = GenerateCoreDatumShift(unProjectedFrom, unProjectedTo);
            if (null == datumShiftOperation)
                return null;

            operations.AddRange(datumShiftOperation.CoordinateOperations);
            nodes.AddRange(datumShiftOperation.CoordinateReferenceSystems.Skip(1));

            operations.AddRange(backDownToProjectionSteps);
            nodes.AddRange(backDownToProjectionNodes);

            return new CoordinateOperationCrsPathInfo(nodes, operations);
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShift(ICrsGeodetic from, ICrsGeodetic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            if (from is ICrsGeographic) {
                if (to is ICrsGeographic)
                    return GenerateCoreDatumShiftGeographic(from as ICrsGeographic, to as ICrsGeographic);
                if (to is ICrsGeocentric)
                    return GenerateCoreDatumShift(from as ICrsGeographic, to as ICrsGeocentric);
            }
            else if (from is ICrsGeocentric) {
                if (to is ICrsGeographic)
                    return GenerateCoreDatumShift(from as ICrsGeocentric, to as ICrsGeographic);
                if (to is ICrsGeocentric)
                    return GenerateCoreDatumShiftGeocentric(from as ICrsGeocentric, to as ICrsGeocentric);
            }
            return null;
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShiftGeocentric(ICrsGeocentric from, ICrsGeocentric to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<List<ICoordinateOperationInfo>>() != null);
            //return GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
            throw new NotImplementedException();
        }

        private CoordinateOperationCrsPathInfo GenerateCoreDatumShiftGeocentric(IDatumGeodetic from, IDatumGeodetic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<List<ICoordinateOperationInfo>>() != null);
            var operations = new List<ICoordinateOperationInfo>();

            var fromTransform = from.IsTransformableToWgs84 ? from.BasicWgs84Transformation : null;
            var toTransform = to.IsTransformableToWgs84 ? to.BasicWgs84Transformation : null;
            var performWgs84Transform = null != fromTransform
                && null != toTransform
                && !fromTransform.Equals(toTransform);

            if (performWgs84Transform && null != fromTransform) {
                operations.Add(fromTransform);
            }

            var fromUnit = from.Spheroid.AxisUnit;
            var toUnit = to.Spheroid.AxisUnit;
            if (null != fromUnit && null != toUnit && fromUnit != toUnit) {
                var conversion = SimpleUnitConversionGenerator.FindConversion(fromUnit, toUnit);
                if (null != conversion) {
                    if (conversion is UnitUnityConversion) {
                        ; // do nothing
                    }
                    else if (conversion is IUnitScalarConversion<double>) {
                        throw new NotImplementedException("scalar unit conversion");
                    }
                    else {
                        throw new NotImplementedException("dunno what to do about this");
                    }
                }
            }

            if (performWgs84Transform && ((ICoordinateOperationInfo)toTransform).HasInverse)
                operations.Add(((ICoordinateOperationInfo)toTransform).GetInverse());

            //return operations;
            throw new NotImplementedException();
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShiftGeographic(ICrsGeographic from, ICrsGeographic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<List<ICoordinateOperationInfo>>() != null);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            var shiftPathInfo = GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
            throw new NotImplementedException();

            /*
            var operations = shiftPathInfo.CoordinateOperations;
            var fromSpheroid = from.Datum.Spheroid;
            var toSpheroid = to.Datum.Spheroid;

            if (null != fromSpheroid && null != toSpheroid && (operations.Count != 0 || fromSpheroid.A != toSpheroid.A || fromSpheroid.B != toSpheroid.B)) {
                operations.Insert(0, new GeographicGeocentricTransformation(fromSpheroid));
                operations.Add(new GeocentricGeographicTransformation(toSpheroid));
            }
            return operations;*/
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShift(ICrsGeocentric from, ICrsGeographic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<List<ICoordinateOperationInfo>>() != null);
            throw new NotImplementedException();
            /*
            var operations = GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum);
            operations.Add(new GeocentricGeographicTransformation(to.Datum.Spheroid));
            return operations;*/
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShift(ICrsGeographic from, ICrsGeocentric to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<List<ICoordinateOperationInfo>>() != null);
            throw new NotImplementedException();
            /*var operations = new List<ICoordinateOperationInfo>{
                new GeographicGeocentricTransformation(from.Datum.Spheroid)
            };
            operations.AddRange(GenerateCoreDatumShiftGeocentric(from.Datum, to.Datum));
            return operations;*/
        }

    }
}
