using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Unit;
using Pigeoid.Utility;

namespace Pigeoid.CoordinateOperation
{
    public class HelmertCrsCoordinateOperationPathGenerator : ICoordinateOperationPathGenerator<ICrs>
    {

        public IEnumerable<ICoordinateOperationCrsPathInfo> Generate(ICrs from, ICrs to) {
            if (from == to)
                return ArrayUtil.CreateSingleElementArray(new CoordinateOperationCrsPathInfo(from));

            var fromGeodetic = from as ICrsGeodetic;
            var toGeodetic = to as ICrsGeodetic;
            if (fromGeodetic != null && toGeodetic != null)
                return ArrayUtil.CreateSingleElementArray(GenerateCoreProjectedLevel(fromGeodetic, toGeodetic));

            return ArrayUtil<ICoordinateOperationCrsPathInfo>.Empty;
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreProjectedLevel(ICrsGeodetic from, ICrsGeodetic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            var unProjectFromPath = FindPathToNonProjectedFromCrs(from);
            var unProjectedFromNode = unProjectFromPath.To as ICrsGeodetic;
            if (unProjectedFromNode == null)
                return null;

            var projectToPath = FindPathFromNonProjectedToCrs(to);
            var unProjectedToNode = projectToPath.From as ICrsGeodetic;
            if (unProjectedToNode == null)
                return null;

            var geocentricShiftPath = GenerateCoreDatumShift(unProjectedFromNode, unProjectedToNode);
            if (geocentricShiftPath == null)
                return null;

            var nodes = unProjectFromPath.CoordinateReferenceSystems.ToList();
            nodes.AddRange(geocentricShiftPath.CoordinateReferenceSystems.Skip(1));
            nodes.AddRange(projectToPath.CoordinateReferenceSystems.Skip(1));
            var operations = unProjectFromPath.CoordinateOperations.ToList();
            operations.AddRange(geocentricShiftPath.CoordinateOperations);
            operations.AddRange(projectToPath.CoordinateOperations);

            return new CoordinateOperationCrsPathInfo(nodes, operations);
        }

        private ICoordinateOperationCrsPathInfo FindPathToNonProjectedFromCrs(ICrs from) {
            Contract.Requires(from != null);
            Contract.Ensures(Contract.Result<ICoordinateOperationCrsPathInfo>() != null);

            var nodes = new List<ICrs> { from };
            var operations = new List<ICoordinateOperationInfo>();

            var unProjectedFrom = from;
            // TODO: consider limiting the number of iterations
            while (unProjectedFrom is ICrsProjected) {
                var projected = unProjectedFrom as ICrsProjected;
                var projectedBase = projected.BaseCrs;
                operations.Add(projected.Projection.GetInverse());
                nodes.Add(projectedBase);
                unProjectedFrom = projectedBase;
            }

            return new CoordinateOperationCrsPathInfo(nodes, operations);
        }

        private ICoordinateOperationCrsPathInfo FindPathFromNonProjectedToCrs(ICrs to) {
            Contract.Requires(to != null);
            Contract.Ensures(Contract.Result<ICoordinateOperationCrsPathInfo>() != null);

            var nodes = new Stack<ICrs>();
            var operations = new Stack<ICoordinateOperationInfo>();

            var unProjectedTo = to;
            // TODO: consider limiting the number of iterations
            while (unProjectedTo is ICrsProjected) {
                var projected = unProjectedTo as ICrsProjected;
                var projectedBase = projected.BaseCrs;
                operations.Push(projected.Projection);
                nodes.Push(projected);
                unProjectedTo = projectedBase;
            }

            if (unProjectedTo == null)
                throw new InvalidOperationException();

            nodes.Push(unProjectedTo);

            return new CoordinateOperationCrsPathInfo(nodes, operations);
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShift(ICrsGeodetic from, ICrsGeodetic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            var fromGeographic = from as ICrsGeographic;
            var toGeographic = to as ICrsGeographic;

            if (fromGeographic != null) {
                if (toGeographic != null)
                    return GenerateCoreDatumShiftGeographic(fromGeographic, toGeographic);
                var toGeocentric = to as ICrsGeocentric;
                if (toGeocentric != null)
                    return GenerateCoreDatumShift(fromGeographic, toGeocentric);
            }

            var fromGeocentric = from as ICrsGeocentric;
            if (fromGeocentric != null) {
                if (toGeographic != null)
                    return GenerateCoreDatumShift(fromGeocentric, toGeographic);
                var toGeocentric = to as ICrsGeocentric;
                if (toGeocentric != null)
                    return GenerateCoreDatumShiftGeocentric(fromGeocentric, toGeocentric);
            }

            return null;
        }

        private bool ComputationallyEqual(IUnit a, IUnit b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            var conversion = SimpleUnitConversionGenerator.FindConversion(a, b);
            return ConversionIsUnity(conversion);
        }

        private bool ConversionIsUnity(IUnitConversion<double> conversion) {
            if (conversion == null)
                return false;
            if (conversion is UnitUnityConversion)
                return true;

            var scalar = conversion as IUnitScalarConversion<double>;
            if (scalar != null)
                return scalar.Factor == 1.0;

            return false;
        }

        private bool ComputationallyEqual(ISpheroidInfo a, ISpheroidInfo b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            return a.A == b.A
                && (
                    a.B == b.B
                    || a.InvF == b.InvF
                );
        }

        private bool ComputationallyEqual(IPrimeMeridianInfo a, IPrimeMeridianInfo b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            return a.Longitude == b.Longitude
                && ComputationallyEqual(a.Unit, b.Unit);
        }

        private bool ComputationallyEqual(Helmert7Transformation a, Helmert7Transformation b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            return a.Equals(b);

            throw new NotImplementedException();
        }

        private bool ComputationallyEqual(IDatumGeodetic a, IDatumGeodetic b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            if (!ComputationallyEqual(a.Spheroid, b.Spheroid))
                return false;

            if (!ComputationallyEqual(a.PrimeMeridian, b.PrimeMeridian))
                return false;

            if (a.IsTransformableToWgs84)
                return b.IsTransformableToWgs84
                    && ComputationallyEqual(a.BasicWgs84Transformation, b.BasicWgs84Transformation);
            else
                return b.IsTransformableToWgs84 == false;

        }

        private bool ComputationallyEqual(IAxis a, IAxis b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            return String.Equals(a.Name, b.Name)
                && String.Equals(a.Orientation, b.Orientation);
        }

        private bool ComputationallyEqual(IList<IAxis> a, IList<IAxis> b) {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; ++i) {
                if (!ComputationallyEqual(a[i], b[i]))
                    return false;
            }
            return true;
        }

        private bool ComputationallyEqual(ICrsGeographic a, ICrsGeographic b) {
            Contract.Requires(a != null);
            Contract.Requires(b != null);
            if (a == b)
                return true;

            return ComputationallyEqual(a.Datum, b.Datum)
                && ComputationallyEqual(a.Axes, b.Axes)
                && ComputationallyEqual(a.Unit, b.Unit);
        }

        private bool ComputationallyEqual(ICrsGeocentric a, ICrsGeocentric b) {
            Contract.Requires(a != null);
            Contract.Requires(b != null);
            if (a == b)
                return true;

            return ComputationallyEqual(a.Datum, b.Datum)
                && ComputationallyEqual(a.Axes, b.Axes)
                && ComputationallyEqual(a.Unit, b.Unit);
        }

        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShiftGeographic(ICrsGeographic from, ICrsGeographic to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            if (ComputationallyEqual(from, to))
                return new CoordinateOperationCrsPathInfo(from);

            ;// if same unit, datum, and axes then treat as the same

            throw new NotImplementedException();

            // ReSharper disable CompareOfFloatsByEqualityOperator
            /*
            var shiftPathInfo = GenerateCoreDatumShiftGeocentric(from, to);
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


        private ICoordinateOperationCrsPathInfo GenerateCoreDatumShiftGeocentric(ICrsGeocentric from, ICrsGeocentric to) {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            if (ComputationallyEqual(from, to))
                return new CoordinateOperationCrsPathInfo(from);

            ;// if same unit, datum, and axes then treat as the same

            throw new NotImplementedException();
            /*
            var operations = new List<ICoordinateOperationInfo>();

            var fromDatum = from.Datum;
            var toDatum = to.Datum;
            if (fromDatum == null || toDatum == null)
                return null;

            var fromTransform = fromDatum.IsTransformableToWgs84 ? fromDatum.BasicWgs84Transformation : null;
            var toTransform = toDatum.IsTransformableToWgs84 ? toDatum.BasicWgs84Transformation : null;
            var performWgs84Transform = null != fromTransform
                && null != toTransform
                && !fromTransform.Equals(toTransform);

            if (performWgs84Transform && null != fromTransform) {
                operations.Add(fromTransform);
            }

            var fromUnit = fromDatum.Spheroid.AxisUnit;
            var toUnit = toDatum.Spheroid.AxisUnit;
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
            throw new NotImplementedException();*/
        }

    }
}
