using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using DotSpatial.Projections;
using Vertesaur;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4DatumWrapper : IDatumGeodetic
    {

        public static Datum Create(IDatumGeodetic datum) {
            if(datum == null) throw new ArgumentNullException("datum");
            Contract.Ensures(Contract.Result<Datum>() != null);

            var result = new Datum {
                Name = datum.Name
            };

            if (datum.IsTransformableToWgs84) {
                var helmert = datum.BasicWgs84Transformation;
                if (helmert.ScaleDeltaPartsPerMillion == 0 && Vector3.Zero.Equals(helmert.RotationArcSeconds)) {
                    if (Vector3.Zero.Equals(helmert.Delta)) {
                        result.DatumType = DatumType.WGS84;
                    }
                    else{
                        result.ToWGS84 = new[] {
                            helmert.Delta.X, helmert.Delta.Y, helmert.Delta.Z
                        };
                        result.DatumType = DatumType.Param3;
                    }
                }
                else {
                    result.ToWGS84 = new[] {
                        helmert.Delta.X, helmert.Delta.Y, helmert.Delta.Z,
                        helmert.RotationArcSeconds.X, helmert.RotationArcSeconds.Y, helmert.RotationArcSeconds.Z,
                        helmert.ScaleDeltaPartsPerMillion
                    };
                    result.DatumType = DatumType.Param7;
                }
            }
            else {
                result.DatumType = DatumType.Unknown;
            }

            result.Spheroid = Proj4SpheroidWrapper.Create(datum.Spheroid);

            return result;
        }

        public static Proj4DatumWrapper CreateWrapper(GeographicInfo geographicInfo) {
            if(geographicInfo == null) throw new ArgumentNullException("geographicInfo");
            Contract.Ensures(Contract.Result<Proj4DatumWrapper>() != null);
            return new Proj4DatumWrapper(
                geographicInfo.Datum,
                new Proj4MeridianWrapper(geographicInfo.Meridian));
        }

        public Proj4DatumWrapper(Datum datum, Proj4MeridianWrapper meridian) {
            if (datum == null) throw new ArgumentNullException("datum");
            if (meridian == null) throw new ArgumentNullException("meridian");
            if (datum.Spheroid == null) throw new ArgumentException("Datum has no spheroid.", "datum");
            Contract.EndContractBlock();
            Core = datum;
            PrimeMeridian = meridian;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
            Contract.Invariant(Core.Spheroid != null);
            Contract.Invariant(PrimeMeridian != null);
        }

        protected Datum Core { get; private set; }

        public ISpheroidInfo Spheroid {
            get {
                return new Proj4SpheroidWrapper(Core.Spheroid);
            }
        }

        public Proj4MeridianWrapper PrimeMeridian { get; private set; }

        IPrimeMeridianInfo IDatumGeodetic.PrimeMeridian { get { return PrimeMeridian; } }

        public Helmert7Transformation BasicWgs84Transformation {
            get {
                if (Core.ToWGS84 == null) {
                    Contract.Assume(!IsTransformableToWgs84);
                    return null;
                }

                if (Core.ToWGS84.Length == 3)
                    return new Helmert7Transformation(
                        new Vector3(Core.ToWGS84[0], Core.ToWGS84[1], Core.ToWGS84[2]));
                if (Core.ToWGS84.Length == 7)
                    return Helmert7Transformation.CreatePositionVectorFormat(
                        new Vector3(Core.ToWGS84[0], Core.ToWGS84[1], Core.ToWGS84[2]),
                        new Vector3(Core.ToWGS84[3], Core.ToWGS84[4], Core.ToWGS84[5]),
                        Core.ToWGS84[6]);
                throw new InvalidOperationException("Unexpected ToWGS84 length: " + Core.ToWGS84.Length);
            }
        }

        public bool IsTransformableToWgs84 {
            get {
                return Core.ToWGS84 != null;
            }
        }

        public string Type {
            get { return "Geodetic"; }
        }

        public string Name {
            get { return Core.Name; }
        }

        public IAuthorityTag Authority {
            get { return new AuthorityTag("PROJ4", Name); }
        }
    }
}
