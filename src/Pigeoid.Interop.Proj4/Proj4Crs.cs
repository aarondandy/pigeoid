using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using DotSpatial.Projections;
using DotSpatial.Projections.Transforms;
using Pigeoid.CoordinateOperation;
using Pigeoid.Ogc;
using Pigeoid.Unit;

namespace Pigeoid.Interop.Proj4
{
    public abstract class Proj4Crs : OgcNamedAuthorityBoundEntity, ICrs
    {

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
            {CoordinateOperationStandardNames.PolarStereographic, "ups"},
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

        public static ProjectionInfo CreateProjection(ICrsProjected crsProjected) {
            if (crsProjected == null) throw new ArgumentNullException("crsProjected");
            Contract.Ensures(Contract.Result<ProjectionInfo>() != null);

            var result = new ProjectionInfo {
                Name = crsProjected.Name
            };

            if (crsProjected.Authority != null) {
                int epsgCode;
                if (crsProjected.Authority.Name == "EPSG" && Int32.TryParse(crsProjected.Authority.Code, out epsgCode))
                    result.EpsgCode = epsgCode;

                result.Authority = crsProjected.Authority.Name;
            }

            if(crsProjected.BaseCrs is ICrsGeographic)
                result.GeographicInfo = Proj4CrsGeographic.CreateGeographic((ICrsGeographic)(crsProjected.BaseCrs));


            var projectionInfo = crsProjected.Projection as IParameterizedCoordinateOperationInfo;

            if (projectionInfo == null)
                return result;

            var projectionMethod = projectionInfo.Method;
            if(projectionMethod == null)
                throw new InvalidOperationException("No projection method.");

            var proj4Name = ToProj4MethodName(projectionMethod.Name);
            result.Transform = TransformManager.GetProj4(proj4Name);

            var lon0Param = new KeywordNamedParameterSelector("LON0", "CENTRALMERIDIAN");
            var lat0Param = new KeywordNamedParameterSelector("LAT0", "LATITUDEORIGIN");
            var x0Param = new KeywordNamedParameterSelector("X0", "FALSEEASTING");
            var y0Param = new KeywordNamedParameterSelector("Y0", "FALSENORTHING");

            var paramLookup = new NamedParameterLookup(projectionInfo.Parameters);
            paramLookup.Assign(lon0Param, lat0Param, x0Param, y0Param);

            // TODO: set the unit

            IUnit geographicUnit = null; // TODO: result.GeographicInfo.Unit
            IUnit projectionUnit = null; // TODO: result.Unit

            if (lon0Param.IsSelected)
                result.CentralMeridian = lon0Param.GetValueAsDouble(geographicUnit);

            if (lat0Param.IsSelected)
                result.LatitudeOfOrigin = lat0Param.GetValueAsDouble(geographicUnit);

            if (x0Param.IsSelected)
                result.FalseEasting = x0Param.GetValueAsDouble(projectionUnit);

            if (y0Param.IsSelected)
                result.FalseNorthing = y0Param.GetValueAsDouble(projectionUnit);

            return result;
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
                    parameters.Add(new NamedParameter<double>("x_0", Core.FalseEasting.Value, projectionUnit));
                if (Core.FalseNorthing.HasValue)
                    parameters.Add(new NamedParameter<double>("y_0", Core.FalseNorthing.Value, projectionUnit));
                if (!Double.IsNaN(Core.ScaleFactor) && Core.ScaleFactor != 0 && Core.ScaleFactor != 1)
                    parameters.Add(new NamedParameter<double>("k_0", Core.ScaleFactor, ScaleUnitUnity.Value));
                if (Core.LatitudeOfOrigin.HasValue)
                    parameters.Add(new NamedParameter<double>("lat_0", Core.LatitudeOfOrigin.Value, geographicUnit));
                if (Core.CentralMeridian.HasValue)
                    parameters.Add(new NamedParameter<double>("lon_0", Core.CentralMeridian.Value, geographicUnit));
                if (Core.StandardParallel1.HasValue)
                    parameters.Add(new NamedParameter<double>("lat_1", Core.StandardParallel1.Value, geographicUnit));
                if (Core.StandardParallel2.HasValue)
                    parameters.Add(new NamedParameter<double>("lat_2", Core.StandardParallel2.Value, geographicUnit));
                if (Core.Over)
                    parameters.Add(new NamedParameter<int>("over", 1));
                if (Core.Geoc)
                    parameters.Add(new NamedParameter<int>("geoc", 1));
                if (Core.alpha.HasValue)
                    parameters.Add(new NamedParameter<double>("alpha", Core.alpha.Value));
                if (Core.LongitudeOfCenter.HasValue)
                    parameters.Add(new NamedParameter<double>("lonc", Core.LongitudeOfCenter.Value, geographicUnit));
                if (Core.Zone.HasValue)
                    parameters.Add(new NamedParameter<int>("zone", Core.Zone.Value));
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
                        : (Core.Transform.Name ?? Core.Transform.Proj4Name)
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
                throw new NotImplementedException();
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

        public static GeographicInfo CreateGeographic(ICrsGeographic crsGeographic) {
            if(crsGeographic == null) throw new ArgumentNullException("crsGeographic");
            Contract.Requires(crsGeographic.Datum.PrimeMeridian != null);
            Contract.Ensures(Contract.Result<GeographicInfo>() != null);

            var result = new GeographicInfo {
                Name = crsGeographic.Name
            };

            result.Datum = Proj4DatumWrapper.Create(crsGeographic.Datum);

            Contract.Assume(crsGeographic.Datum.PrimeMeridian != null);
            result.Meridian = Proj4MeridianWrapper.Create(crsGeographic.Datum.PrimeMeridian);

            // TODO: set the unit

            return result;
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
