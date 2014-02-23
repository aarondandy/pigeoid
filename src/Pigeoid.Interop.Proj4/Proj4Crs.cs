using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using DotSpatial.Projections;
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

            throw new NotImplementedException();

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
            : base(projectionInfo.Name, CreateAuthorityTag(projectionInfo)) {
            Contract.Requires(projectionInfo != null);
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
                throw new NotImplementedException();
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
            Contract.Ensures(Contract.Result<GeographicInfo>() != null);

            var result = new GeographicInfo {
                Name = crsGeographic.Name
            };

            result.Datum = Proj4DatumWrapper.Create(crsGeographic.Datum);

            result.Meridian = Proj4MeridianWrapper.Create(crsGeographic.Datum.PrimeMeridian);

            return result;
        }

        public Proj4CrsGeographic(GeographicInfo geographicInfo)
            : base(geographicInfo.Name, new AuthorityTag("PROJ4", String.Empty))
        {
            Contract.Requires(geographicInfo != null);
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
                throw new NotImplementedException();
            }
        }

        public IList<IAxis> Axes {
            get {
                throw new NotImplementedException();
            }
        }
    }


}
