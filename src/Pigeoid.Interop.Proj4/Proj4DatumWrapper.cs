using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Ogc;
using DotSpatial.Projections;
using Vertesaur;

namespace Pigeoid.Interop.Proj4
{
    public class Proj4DatumWrapper : IDatumGeodetic
    {

        public Proj4DatumWrapper(Datum datum) {
            if(datum ==null) throw new ArgumentNullException("datum");
            if(datum.Spheroid == null) throw new ArgumentException("Datum has no spheroid.","datum");
            Core = datum;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Core != null);
        }

        protected Datum Core { get; private set; }

        public ISpheroidInfo Spheroid { get { return new Proj4SpheroidWrapper(Core.Spheroid); } }

        public IPrimeMeridianInfo PrimeMeridian {
            get { throw new NotImplementedException(); }
        }

        public Helmert7Transformation BasicWgs84Transformation {
            get {
                if (Core.ToWGS84 == null)
                    return null;
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
            get { return BasicWgs84Transformation != null; }
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
