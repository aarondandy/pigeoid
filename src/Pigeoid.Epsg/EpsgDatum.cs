using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Epsg.Resources;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.Epsg
{

    public abstract class EpsgDatum : IDatum
    {

        internal const int Wgs84DatumCode = 6326;

        private readonly ushort _code;

        internal EpsgDatum(ushort code, string name, EpsgArea area) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            _code = code;
            Name = name;
            Area = area;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(Area != null);
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public EpsgArea Area { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public abstract string Type { get; }

    }

    internal abstract class EpsgDatumContracts : EpsgDatum
    {

        protected EpsgDatumContracts(string name, EpsgArea area)
            : base(0, name, area)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
        }

        public override string Type {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }
    }

    public class EpsgDatumEngineering : EpsgDatum
    {

        internal static EpsgDatumEngineering Create(ushort code, string name, EpsgArea area) {
            return new EpsgDatumEngineering(code, name, area);
        }

        internal EpsgDatumEngineering(ushort code, string name, EpsgArea area) : base(code, name, area) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
        }
        public override string Type { get { return "Engineering"; } }
    }

    public class EpsgDatumVertical : EpsgDatum
    {
        internal static EpsgDatumVertical Create(ushort code, string name, EpsgArea area) {
            return new EpsgDatumVertical(code, name, area);
        }

        internal EpsgDatumVertical(ushort code, string name, EpsgArea area) : base(code, name, area) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
        }
        public override string Type { get { return "Vertical"; } }
    }

    public class EpsgDatumGeodetic : EpsgDatum, IDatumGeodetic
    {

        private readonly Lazy<Helmert7Transformation> _basicWgs84Transformation;

        internal EpsgDatumGeodetic(ushort code, string name, EpsgEllipsoid spheroid, EpsgPrimeMeridian primeMeridian, EpsgArea area)
            : base(code, name, area) {
            Contract.Requires(spheroid != null);
            Contract.Requires(primeMeridian != null);
            Contract.Requires(area != null);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Spheroid = spheroid;
            PrimeMeridian = primeMeridian;
            _basicWgs84Transformation = new Lazy<Helmert7Transformation>(FindBasicWgs84Transformation, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
            Contract.Invariant(PrimeMeridian != null);
            Contract.Invariant(_basicWgs84Transformation != null);
        }

        public EpsgEllipsoid Spheroid { get; private set; }

        ISpheroidInfo IDatumGeodetic.Spheroid { get { return Spheroid; } }

        public EpsgPrimeMeridian PrimeMeridian { get; private set; }

        IPrimeMeridianInfo IDatumGeodetic.PrimeMeridian { get { return PrimeMeridian; }  }

        public override string Type { get { return "Geodetic"; } }


        private Helmert7Transformation ExtractHelmert7Transformation(EpsgCoordinateTransformInfo transform) {
            var method = transform.Method;
            Contract.Assume(method != null);

            var compiler = new StaticCoordinateOperationCompiler();
            var compileRequest = new CoordinateOperationCrsPathInfo(
                new[] { transform.SourceCrs, transform.TargetCrs },
                new[] { transform });
            var compileResult = compiler.Compile(compileRequest);
            if (compileResult == null)
                return null;
            var transformSteps = (compileResult as IEnumerable<ITransformation>) ?? ArrayUtil.CreateSingleElementArray(compileResult);
            var exposedSteps = transformSteps.Select(step => {
                if (step is GeocentricTransformationGeographicWrapper) {
                    return ((GeocentricTransformationGeographicWrapper)step).GeocentricCore;
                }
                return step;
            });

            foreach (var step in exposedSteps) {
                if (step is Helmert7Transformation) {
                    return step as Helmert7Transformation;
                }
                if (step is GeocentricTranslation) {
                    return new Helmert7Transformation(((GeocentricTranslation)step).Delta);
                }
                if (step is GeographicGeocentricTranslation) {
                    return new Helmert7Transformation(((GeographicGeocentricTranslation)step).Delta);
                }
            }

            return null;
        }

        private Helmert7Transformation FindBasicWgs84Transformation() {
            if (Code == EpsgDatum.Wgs84DatumCode)
                return new Helmert7Transformation(Vector3.Zero);

            // TODO: this should be pre-calculated

            var acceptedForwardOpMethodCodes = new HashSet<int> { 9603, 9606, 9607 };

            const ushort GeogWgs84Code = 4326;
            var opCodesForwardToGeog84 = EpsgMicroDatabase.Default.GetOpsToCrs(GeogWgs84Code);

            var crsForThisDatum = EpsgMicroDatabase.Default.GetAllNormalCrs()
                .Where(crs => crs is EpsgCrsGeographic || crs is EpsgCrsGeocentric)
                .Cast<EpsgCrsDatumBased>()
                .Where(crs => crs.Datum.Code == Code);

            var crsCodesForThisDatum = crsForThisDatum
                .Select(crs => crs.Code)
                .Where(code => code >= 0 && code < UInt16.MaxValue)
                .Select(code => (ushort)code);

            var forwardOpCodes = crsCodesForThisDatum
                .Select(EpsgMicroDatabase.Default.GetOpsFromCrs)
                .Where(codes => codes != null)
                .Select(opCodesForwardToGeog84.Intersect)
                .SelectMany(codes => codes)
                .Distinct();

            var forwardOps = forwardOpCodes
                .Select(EpsgMicroDatabase.Default.GetCoordinateTransformOrConcatenatedInfo)
                .Where(op => op != null)
                .SelectMany<EpsgCoordinateOperationInfoBase,EpsgCoordinateTransformInfo>(op => {
                    if (op is EpsgCoordinateTransformInfo)
                        return ArrayUtil.CreateSingleElementArray((EpsgCoordinateTransformInfo)op);
                    else if (op is EpsgConcatenatedCoordinateOperationInfo)
                        return ((EpsgConcatenatedCoordinateOperationInfo)op).Steps.OfType<EpsgCoordinateTransformInfo>();
                    else
                        return ArrayUtil<EpsgCoordinateTransformInfo>.Empty;
                })
                .Where(op => acceptedForwardOpMethodCodes.Contains(op.Method.Code));

            var sortedTransformOperations = forwardOps
                .OrderBy(x => x.Deprecated)
                .ThenByDescending(x => x.Parameters.Count()) // prefer 7-parameter transforms to geocentric translations
                .ThenByDescending(x => x.Accuracy.HasValue)
                .ThenBy(x => x.Accuracy.GetValueOrDefault());

            var bestTransform = sortedTransformOperations
                .Select(ExtractHelmert7Transformation)
                .FirstOrDefault(x => x != null);

            return bestTransform;
        }

        public Helmert7Transformation BasicWgs84Transformation {
            get {
                var result = _basicWgs84Transformation.Value;
                Contract.Assume(IsTransformableToWgs84 ? result != null : result == null);
                return result;
            }
        }

        [Obsolete("Name should indicate it will use Helmert7Transformation (basic?)")]
        public bool IsTransformableToWgs84 {
            get {
                return _basicWgs84Transformation.Value != null;
            }
        }
    }
}
