using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using DotSpatial.Projections;
using DotSpatial.Projections.Transforms;
using Pigeoid.CoordinateOperation;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using System.Text.RegularExpressions;

namespace Pigeoid.Interop.Proj4
{
    public abstract class Proj4Crs : OgcNamedAuthorityBoundEntity, ICrs
    {

        public static ProjectionInfo CreateProjection(ICrs crs) {
            var projectedCrs = crs as ICrsProjected;
            if (projectedCrs != null)
                return Proj4CrsProjected.CreateProjection(projectedCrs);
            var geographicCrs = crs as ICrsGeographic;
            if (geographicCrs != null)
                return Proj4CrsGeographic.CreateProjection(geographicCrs);

            throw new NotSupportedException();
        }

        public static Proj4Crs Wrap(ProjectionInfo projectionInfo) {
            if (projectionInfo == null) throw new ArgumentNullException("projectionInfo");
            Contract.EndContractBlock();

            if (projectionInfo.Transform == null || projectionInfo.IsLatLon) {
                return new Proj4CrsGeographic(projectionInfo.GeographicInfo);
            }

            return new Proj4CrsProjected(projectionInfo);
        }

        protected Proj4Crs(string name, string authority, string code)
            : this(name, new AuthorityTag(authority ?? "PROJ4", code ?? String.Empty)) {
            Contract.Requires(name != null);
        }

        protected Proj4Crs(string name, IAuthorityTag tag)
            : base(name, tag) {
            Contract.Requires(name != null);
        }

    }

    public class Proj4CrsProjected : Proj4Crs, ICrsProjected
    {

        private static readonly TransformManager TransformManager = new TransformManager();

        private static readonly Dictionary<string, string> ToProj4NameLookup = new Dictionary<string, string>(CoordinateOperationNameNormalizedComparer.Default) {        
            {CoordinateOperationStandardNames.AlbersEqualAreaConic,"aea"},
            {CoordinateOperationStandardNames.AzimuthalEquidistant, "aeqd"},
            {CoordinateOperationStandardNames.BipolarObliqueConformalConic, "bipc"},
            {CoordinateOperationStandardNames.CassiniSoldner, "cass"},
            {CoordinateOperationStandardNames.CylindricalEqualArea, "cea"},
            {CoordinateOperationStandardNames.CrasterParabolic, "crast"},
            {CoordinateOperationStandardNames.Eckert1, "eck1"},
            {CoordinateOperationStandardNames.Eckert2, "eck2"},
            {CoordinateOperationStandardNames.Eckert3, "eck3"},
            {CoordinateOperationStandardNames.Eckert4, "eck4"},
            {CoordinateOperationStandardNames.Eckert5, "eck5"},
            {CoordinateOperationStandardNames.Eckert6, "eck6"},
            {CoordinateOperationStandardNames.EquidistantCylindrical, "eqc"},
            {CoordinateOperationStandardNames.EquidistantConic, "eqdc"},
            {CoordinateOperationStandardNames.Foucaut, "fouc"},
            {CoordinateOperationStandardNames.GallStereographic, "gall"},
            {CoordinateOperationStandardNames.GeneralSinusoidal, "gn_sinu"},
            {CoordinateOperationStandardNames.Geos, "geos"},
            {CoordinateOperationStandardNames.Gnomonic, "gnom"},
            {CoordinateOperationStandardNames.GoodeHomolosine, "goode"},
            {CoordinateOperationStandardNames.HammerAitoff, "hammer"},
            {CoordinateOperationStandardNames.HotineObliqueMercator, "omerc"},
            {CoordinateOperationStandardNames.Kavraisky5,"kav5"},
            {CoordinateOperationStandardNames.LambertAzimuthalEqualArea, "laea"},
            {CoordinateOperationStandardNames.LambertConicConformal2Sp, "lcc"},
            {CoordinateOperationStandardNames.LambertEqualAreaConic, "leac"},
            {CoordinateOperationStandardNames.Loximuthal, "loxim"},
            {CoordinateOperationStandardNames.McBrydeThomasFlatPolarSine, "mbt_s"},
            {CoordinateOperationStandardNames.Mercator2Sp, "merc"},
            {CoordinateOperationStandardNames.Mercator1Sp, "merc"},
            {CoordinateOperationStandardNames.MercatorAuxiliarySphere, "merc"},
            {CoordinateOperationStandardNames.MillerCylindrical, "mill"},
            {CoordinateOperationStandardNames.Mollweide, "moll"},
            {CoordinateOperationStandardNames.NewZealandMapGrid, "nzmg"},
            {CoordinateOperationStandardNames.ObliqueStereographic, "sterea"},
            {CoordinateOperationStandardNames.ObliqueCylindricalEqualArea, "ocea"},
            {CoordinateOperationStandardNames.ObliqueMercator, "omerc"},
            {CoordinateOperationStandardNames.Orthographic, "ortho"},
            {CoordinateOperationStandardNames.Polyconic, "poly"},
            {CoordinateOperationStandardNames.PutinsP1, "putp1"},
            {CoordinateOperationStandardNames.QuarticAuthalic, "qua_aut"},
            {CoordinateOperationStandardNames.Robinson, "robin"},
            {CoordinateOperationStandardNames.Sinusoidal, "sinu"},
            {CoordinateOperationStandardNames.Stereographic, "stere"},
            {CoordinateOperationStandardNames.SwissObliqueCylindrical, "somerc"},
            {CoordinateOperationStandardNames.TransverseMercator, "tmerc"},
            {CoordinateOperationStandardNames.TwoPointEquidistant, "tpeqd"},
            {CoordinateOperationStandardNames.PolarStereographic, "stere"},
            {CoordinateOperationStandardNames.VanDerGrinten, "vandg"},
            {CoordinateOperationStandardNames.Wagner4, "wag4"},
            {CoordinateOperationStandardNames.Wagner5, "wag5"},
            {CoordinateOperationStandardNames.Wagner6, "wag6"},
            {CoordinateOperationStandardNames.Winkel1, "wink1"},
            {CoordinateOperationStandardNames.Winkel2, "wink2"},
            {CoordinateOperationStandardNames.WinkelTripel, "wintri"},
        };


        private static string ToProj4MethodName(string name) {
            if (String.IsNullOrEmpty(name))
                return name;

            string lookupValue;
            if (ToProj4NameLookup.TryGetValue(name, out lookupValue))
                return lookupValue;

            return name.ToLowerInvariant();
        }

        private static readonly Regex GeodeticOperationNameRegex = new Regex("geographic|geocentric|geodetic", RegexOptions.IgnoreCase);

        public static ProjectionInfo CreateProjection(ICrsProjected crsProjected) {
            if (crsProjected == null) throw new ArgumentNullException("crsProjected");
            Contract.Ensures(Contract.Result<ProjectionInfo>() != null);

            var result = ProjectionInfo.FromProj4String(String.Empty);

            result.Name = crsProjected.Name;

            if (crsProjected.Authority != null) {
                int epsgCode;
                if (crsProjected.Authority.Name == "EPSG" && Int32.TryParse(crsProjected.Authority.Code, out epsgCode))
                    result.EpsgCode = epsgCode;

                result.Authority = crsProjected.Authority.Name;
            }

            var geographicBase = (crsProjected.BaseCrs as ICrsGeodetic) ?? (crsProjected as ICrsGeodetic);
            if (geographicBase != null)
                result.GeographicInfo = Proj4CrsGeographic.CreateGeographic(geographicBase);


            var projectionInfo = crsProjected.Projection as IParameterizedCoordinateOperationInfo;

            if (projectionInfo == null)
                return result;

            var projectionMethod = projectionInfo.Method;
            if(projectionMethod == null)
                throw new InvalidOperationException("No projection method.");

            var proj4Name = ToProj4MethodName(projectionMethod.Name);

            IUnit geographicUnit = geographicBase == null ? null : geographicBase.Unit;
            IUnit projectionUnit = crsProjected.Unit;
            if (projectionUnit != null)
                result.Unit = Proj4LinearUnit.ConvertToProj4(projectionUnit);
            switch (proj4Name) { 
                case "stere":
                    ApplyParametersStere(result, projectionInfo.Parameters);
                    break;
                default:
                    ApplyParametersDefault(result, projectionInfo.Parameters);
                    break;
            }

            if (result.Zone.HasValue && proj4Name == "tmerc")
                proj4Name = "utm";

            if (!TransformManager.Transforms.Exists(x => x.Proj4Name == proj4Name)) {
                if (GeodeticOperationNameRegex.IsMatch(proj4Name)) {
                    proj4Name = "longlat";
                }
                else {
                    throw new InvalidOperationException();
                }
            }

            result.Transform = TransformManager.GetProj4(proj4Name);

            var finalResult = ProjectionInfo.FromProj4String(result.ToProj4String()); // TODO: fix this hack
            finalResult.CentralMeridian = result.CentralMeridian;
            finalResult.LongitudeOfCenter = result.LongitudeOfCenter;
            finalResult.LatitudeOfOrigin = result.LatitudeOfOrigin;
            finalResult.StandardParallel1 = result.StandardParallel1;
            finalResult.StandardParallel2 = result.StandardParallel2;
            finalResult.FalseEasting = result.FalseEasting;
            finalResult.FalseNorthing = result.FalseNorthing;
            finalResult.ScaleFactor = result.ScaleFactor;
            finalResult.alpha = result.alpha;
            finalResult.IsSouth = result.IsSouth;
            finalResult.Zone = result.Zone;
            finalResult.Unit = result.Unit;
            if (result.GeographicInfo == null) { // NOTE: this is all so terrible
                finalResult.GeographicInfo = null;
            }
            else {
                if (finalResult.GeographicInfo == null) {
                    finalResult.GeographicInfo = result.GeographicInfo;
                }
                else {
                    finalResult.GeographicInfo.Datum.Spheroid = result.GeographicInfo.Datum.Spheroid;
                }
            }
            return finalResult;
        }

        private static void ApplyParametersDefault(ProjectionInfo result, IEnumerable<INamedParameter> parameters) {
            Contract.Requires(result != null);
            Contract.Requires(parameters != null);
            var lon0Param = new MultiParameterSelector(
                new CentralMeridianParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector()
            );
            var loncParam = new LongitudeOfCenterParameterSelector();
            var lat0Param = new MultiParameterSelector(
                new LatitudeOfCenterParameterSelector(),
                new LatitudeOfNaturalOriginParameterSelector());
            var lat1Param = new StandardParallelParameterSelector(1);
            var lat2Param = new StandardParallelParameterSelector(2);
            var x0Param = new FalseEastingParameterSelector();
            var y0Param = new FalseNorthingParameterSelector();
            var k0Param = new ScaleFactorParameterSelector();
            var alphaParam = new KeywordNamedParameterSelector("ALPHA", "AZIMUTH");
            var zoneParam = new FullMatchParameterSelector("ZONE");
            var southParam = new FullMatchParameterSelector("SOUTH");
            var standardParallel1Param = new MultiParameterSelector(
                new StandardParallelParameterSelector(),
                new StandardParallelParameterSelector(1));
            var standardParallel2Param = new StandardParallelParameterSelector(2);

            var paramLookup = new NamedParameterLookup(parameters);
            paramLookup.Assign(lon0Param, loncParam, lat0Param, lat1Param, lat2Param, x0Param, y0Param, k0Param, alphaParam, zoneParam, southParam, standardParallel1Param, standardParallel2Param);

            if (lon0Param.IsSelected)
                result.CentralMeridian = lon0Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (loncParam.IsSelected)
                result.LongitudeOfCenter = loncParam.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (lat0Param.IsSelected)
                result.LatitudeOfOrigin = lat0Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (lat1Param.IsSelected)
                result.StandardParallel1 = lat1Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (lat2Param.IsSelected)
                result.StandardParallel2 = lat2Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (x0Param.IsSelected)
                result.FalseEasting = x0Param.GetValueAsDouble(OgcLinearUnit.DefaultMeter);
            if (y0Param.IsSelected)
                result.FalseNorthing = y0Param.GetValueAsDouble(OgcLinearUnit.DefaultMeter);
            if (k0Param.IsSelected)
                result.ScaleFactor = k0Param.GetValueAsDouble(ScaleUnitUnity.Value) ?? 1.0;
            if (alphaParam.IsSelected)
                result.alpha = alphaParam.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (southParam.IsSelected)
                result.IsSouth = southParam.GetValueAsBoolean().GetValueOrDefault();
            if (standardParallel1Param.IsSelected)
                result.StandardParallel1 = standardParallel1Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (standardParallel2Param.IsSelected)
                result.StandardParallel2 = standardParallel2Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (zoneParam.IsSelected)
                result.Zone = zoneParam.GetValueAsInt32();
        }

        private static void ApplyParametersStere(ProjectionInfo result, IEnumerable<INamedParameter> parameters) {
            Contract.Requires(result != null);
            Contract.Requires(parameters != null);
            var lon0Param = new MultiParameterSelector(
                new CentralMeridianParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector(),
                new LongitudeOfCenterParameterSelector());
            var spParam = new MultiParameterSelector(
                new LatitudeOfTrueScaleParameterSelector(),
                new StandardParallelParameterSelector(),
                new LatitudeOfNaturalOriginParameterSelector(),
                new LatitudeOfCenterParameterSelector());
            var k0Param = new ScaleFactorParameterSelector();
            var x0Param = new FalseEastingParameterSelector();
            var y0Param = new FalseNorthingParameterSelector();

            var paramLookup = new NamedParameterLookup(parameters);
            paramLookup.Assign(lon0Param, spParam, k0Param, x0Param, y0Param);

            if (lon0Param.IsSelected)
                result.CentralMeridian = lon0Param.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (spParam.IsSelected)
                result.StandardParallel1 = spParam.GetValueAsDouble(OgcAngularUnit.DefaultDegrees);
            if (k0Param.IsSelected)
                result.ScaleFactor = k0Param.GetValueAsDouble(ScaleUnitUnity.Value) ?? 1.0;
            if (x0Param.IsSelected)
                result.FalseEasting = x0Param.GetValueAsDouble(OgcLinearUnit.DefaultMeter);
            if (y0Param.IsSelected)
                result.FalseNorthing = y0Param.GetValueAsDouble(OgcLinearUnit.DefaultMeter);

            if (!result.LatitudeOfOrigin.HasValue && result.StandardParallel1.HasValue)
                result.LatitudeOfOrigin = result.StandardParallel1.GetValueOrDefault() >= 0 ? 90.0 : -90.0;
        }

        private static AuthorityTag CreateAuthorityTag(ProjectionInfo projectionInfo) {
            Contract.Requires(projectionInfo != null);
            Contract.Ensures(Contract.Result<AuthorityTag>() != null);
            return projectionInfo.EpsgCode >= 0
                ? new AuthorityTag("EPSG", projectionInfo.EpsgCode.ToString(CultureInfo.InvariantCulture))
                : new AuthorityTag("PROJ4", String.Empty);
        }

        public Proj4CrsProjected(ProjectionInfo projectionInfo)
            : base(projectionInfo.Name ?? projectionInfo.Transform.Name ?? "Unknown", CreateAuthorityTag(projectionInfo)) {
                if (projectionInfo.Transform == null)
                    throw new ArgumentException("Transform is required", "projectionInfo");
            Contract.Requires(projectionInfo != null);
            Contract.Requires(projectionInfo.GeographicInfo != null);
            Core = projectionInfo;
            Geographic = new Proj4CrsGeographic(projectionInfo.GeographicInfo);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Geographic != null);
            Contract.Invariant(Core != null);
        }

        public ProjectionInfo Core { get; private set; }

        public Proj4CrsGeographic Geographic { get; private set; }

        public ICrsGeodetic BaseCrs {
            get { return Geographic; }
        }

        public ICoordinateOperationInfo Projection {
            get {

                var geographicInfo = Core.GeographicInfo;

                var projectionUnit = Core.Unit != null ? new Proj4LinearUnit(Core.Unit) : null;
                var geographicUnit = geographicInfo != null && geographicInfo.Unit != null
                    ? new Proj4AngularUnit(geographicInfo.Unit) : null;

                var parameters = new List<INamedParameter>();
                if (Core.FalseEasting.HasValue)
                    parameters.Add(new NamedParameter<double>("False_Easting", Core.FalseEasting.GetValueOrDefault(), OgcLinearUnit.DefaultMeter));
                if (Core.FalseNorthing.HasValue)
                    parameters.Add(new NamedParameter<double>("False_Northing", Core.FalseNorthing.GetValueOrDefault(), OgcLinearUnit.DefaultMeter));
                if (!Double.IsNaN(Core.ScaleFactor) && Core.ScaleFactor != 0 && Core.ScaleFactor != 1)
                    parameters.Add(new NamedParameter<double>("Scale_Factor", Core.ScaleFactor, ScaleUnitUnity.Value));
                if (Core.LatitudeOfOrigin.HasValue)
                    parameters.Add(new NamedParameter<double>("Latitude_Of_Origin", Core.LatitudeOfOrigin.GetValueOrDefault(), geographicUnit));
                if (Core.LongitudeOfCenter.HasValue)
                    parameters.Add(new NamedParameter<double>("Longitude_Of_Center", Core.LongitudeOfCenter.GetValueOrDefault(), geographicUnit));
                if (Core.StandardParallel1.HasValue)
                    parameters.Add(new NamedParameter<double>("Standard_Parallel_1", Core.StandardParallel1.GetValueOrDefault(), geographicUnit));
                if (Core.StandardParallel2.HasValue)
                    parameters.Add(new NamedParameter<double>("Standard_Parallel_2", Core.StandardParallel2.GetValueOrDefault(), geographicUnit));
                if (Core.Over)
                    parameters.Add(new NamedParameter<int>("over", 1));
                if (Core.Geoc)
                    parameters.Add(new NamedParameter<int>("geoc", 1));
                if (Core.alpha.HasValue) {
                    var paramName = "Alpha";
                    if (Core.Transform != null) {
                        if(Core.Transform.Proj4Name == "omerc")
                            paramName = "Azimuth";
                    }
                    parameters.Add(new NamedParameter<double>(paramName, Core.alpha.GetValueOrDefault()));
                }


                double? centralMeridian = Core.CentralMeridian;

                if (Core.Zone.HasValue)
                {
                    parameters.Add(new NamedParameter<int>("zone", Core.Zone.GetValueOrDefault()));
                    // (Core.Zone.GetValueOrDefault() * 6) - 183;
                }

                if (centralMeridian.HasValue)
                    parameters.Add(new NamedParameter<double>("Central_Meridian", centralMeridian.GetValueOrDefault(), geographicUnit));

                if (Core.IsLatLon) {
                    parameters.Add(new NamedParameter<string>("proj", "longlat"));
                }
                else {
                    if (Core.Transform != null)
                        parameters.Add(new NamedParameter<string>("proj", Core.Transform.Name ?? Core.Transform.Proj4Name));
                    // TODO: make sure to get the units added to the parameters!
                }

                if (Core.IsSouth)
                    parameters.Add(new NamedParameter<bool>("south", true));
                if (Core.NoDefs)
                    parameters.Add(new NamedParameter<bool>("no_defs", true));

                var methodName = (
                    Core.Transform == null
                        ? null
                        : (Core.Transform.Proj4Name ?? Core.Transform.Name)
                ) ?? "Unknown";
                var method = new OgcCoordinateOperationMethodInfo(methodName);
                return new CoordinateOperationInfo(
                    Core.Name ?? "Unknown",
                    parameters,
                    method
                );
            }
        }

        public Proj4DatumWrapper Datum { get { return Geographic.Datum; } }

        IDatumGeodetic ICrsGeodetic.Datum { get { return Datum; } }

        public IUnit Unit {
            get {
                if (Core.Unit == null)
                    return null;
                return new Proj4LinearUnit(Core.Unit);
            }
        }

        public IList<IAxis> Axes {
            get {
                throw new NotImplementedException();
            }
        }
    }

    public class Proj4CrsGeographic : Proj4Crs, ICrsGeographic
    {

        public static GeographicInfo CreateGeographic(ICrsGeodetic crsGeodetic) {
            if (crsGeodetic == null) throw new ArgumentNullException("crsGeodetic");
            Contract.Requires(crsGeodetic.Datum.PrimeMeridian != null);
            Contract.Ensures(Contract.Result<GeographicInfo>() != null);

            var result = new GeographicInfo {
                Name = crsGeodetic.Name
            };

            result.Datum = Proj4DatumWrapper.Create(crsGeodetic.Datum);

            Contract.Assume(crsGeodetic.Datum.PrimeMeridian != null);
            result.Meridian = Proj4MeridianWrapper.Create(crsGeodetic.Datum.PrimeMeridian);

            // TODO: set the unit

            return result;
        }

        public static ProjectionInfo CreateProjection(ICrsGeographic crsGeographic) {
            if (crsGeographic == null) throw new ArgumentNullException("crsGeographic");
            Contract.Ensures(Contract.Result<ProjectionInfo>() != null);

            var geographic = CreateGeographic(crsGeographic);

            var result = new ProjectionInfo {
                GeographicInfo = geographic,
                IsLatLon = true,
                Transform = new DotSpatial.Projections.Transforms.LongLat()
            };

            var finalResult = ProjectionInfo.FromProj4String(result.ToProj4String()); // TODO: fix this hack
            return finalResult;
        }

        public Proj4CrsGeographic(GeographicInfo geographicInfo)
            : base(geographicInfo.Name ?? geographicInfo.Datum.Name ?? "Unknown", new AuthorityTag("PROJ4", String.Empty))
        {
            Contract.Requires(geographicInfo != null);
            Contract.Requires(geographicInfo.Meridian != null);
            Contract.Requires(geographicInfo.Datum != null);
            Contract.Requires(geographicInfo.Datum.Spheroid != null);
            Core = geographicInfo;
            Datum = Proj4DatumWrapper.CreateWrapper(geographicInfo);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Datum != null);
            Contract.Invariant(Core != null);
        }

        public GeographicInfo Core { get; private set; }

        public Proj4DatumWrapper Datum { get; private set; }

        IDatumGeodetic ICrsGeodetic.Datum {
            get { return Datum; }
        }

        public IUnit Unit {
            get {
                return Core.Unit == null ? null : new Proj4AngularUnit(Core.Unit);
            }
        }

        public IList<IAxis> Axes {
            get {
                throw new NotImplementedException();
            }
        }
    }


}
