using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using Vertesaur;

namespace Pigeoid.Interop.Proj4
{

    [Obsolete]
    public class Proj4Spheroid :
        ISpheroidInfo,
        IEquatable<ISpheroid<double>>
    {

        private static readonly ReadOnlyCollection<Proj4Spheroid> DefaultSpheroids;

        static Proj4Spheroid() {
            DefaultSpheroids = new ReadOnlyCollection<Proj4Spheroid>(new[] {
                new Proj4Spheroid("MERIT","MERIT 1983",new SpheroidEquatorialInvF(6378137.0,298.257)),
                new Proj4Spheroid("SGS85","Soviet Geodetic System 85",new SpheroidEquatorialInvF(6378136.0,298.257)), 
                new Proj4Spheroid("GRS80","GRS 1980(IUGG, 1980)",new SpheroidEquatorialInvF(6378137.0,298.257222101)), 
                new Proj4Spheroid("IAU76","IAU 1976",new SpheroidEquatorialInvF(6377563.396,298.257)), 
                new Proj4Spheroid("airy","Airy 1830",new SpheroidEquatorialPolar(6377563.396,6356256.910)), 
                new Proj4Spheroid("APL4.9","Appl. Physics. 1965",new SpheroidEquatorialInvF(6378137.0,298.25)), 
                new Proj4Spheroid("NWL9D","Naval Weapons Lab., 1965",new SpheroidEquatorialInvF(6378145.0,298.25)), 
                new Proj4Spheroid("mod_airy","Modified Airy",new SpheroidEquatorialPolar(6377340.189,6356034.446)), 
                new Proj4Spheroid("andrae","Andrae 1876 (Den., Iclnd.)",new SpheroidEquatorialInvF(6377104.43,300.0)), 
                new Proj4Spheroid("aust_SA","Australian Natl & S. Amer. 1969",new SpheroidEquatorialInvF(6378160.0,298.25)), 
                new Proj4Spheroid("GRS67","GRS 67(IUGG 1967)",new SpheroidEquatorialInvF(6378160.0,298.2471674270)), 
                new Proj4Spheroid("bessel","Bessel 1841",new SpheroidEquatorialInvF(6377397.155,299.1528128)), 
                new Proj4Spheroid("bess_nam","Bessel 1841 (Namibia)",new SpheroidEquatorialInvF(6377483.865,299.1528128)), 
                new Proj4Spheroid("clrk66","Clarke 1866",new SpheroidEquatorialPolar(6378206.4,6356583.8)), 
                new Proj4Spheroid("clrk80","Clarke 1880 mod.",new SpheroidEquatorialInvF(6378249.145,293.4663)), 
                new Proj4Spheroid("CPM","Comm. des Poids et Mesures 1799",new SpheroidEquatorialInvF(6375738.7,334.29)), 
                new Proj4Spheroid("delmbr","Delambre 1810 (Belgium)",new SpheroidEquatorialInvF(6376428,311.5)), 
                new Proj4Spheroid("engelis","Engelis 1985",new SpheroidEquatorialInvF(6378136.05,298.2566)), 
                new Proj4Spheroid("evrst30","Everest 1830",new SpheroidEquatorialInvF(6377276.345,300.8017)), 
                new Proj4Spheroid("evrst48","Everest 1948",new SpheroidEquatorialInvF(6377304.063,300.8017)),
                new Proj4Spheroid("evrst56","Everest 1956",new SpheroidEquatorialInvF(6377301.243,300.8017)),
                new Proj4Spheroid("evrst69","Everest 1969",new SpheroidEquatorialInvF(6377295.664,300.8017)), 
                new Proj4Spheroid("evrstSS","Everest (Sabah & Sarawak)",new SpheroidEquatorialInvF(6377298.556,300.8017)), 
                new Proj4Spheroid("fschr60","Fischer (Mercury Datum) 1960",new SpheroidEquatorialInvF(6378166,298.3)), 
                new Proj4Spheroid("fschr60m","Modified Fischer 1960",new SpheroidEquatorialInvF(6378155,298.3)), 
                new Proj4Spheroid("fschr68","Fischer 1968",new SpheroidEquatorialInvF(6378150,298.3)), 
                new Proj4Spheroid("helmert","Helmert 1906",new SpheroidEquatorialInvF(6378200,298.3)), 
                new Proj4Spheroid("hough","Hough",new SpheroidEquatorialInvF(6378270.0,297)), 
                new Proj4Spheroid("intl","International 1909 (Hayford)",new SpheroidEquatorialInvF(6378388.0,297)),
                new Proj4Spheroid("krass","Krassovsky, 1942",new SpheroidEquatorialInvF(6378245.0,298.3)), 
                new Proj4Spheroid("kaula","Kaula 1961",new SpheroidEquatorialInvF(6378163,298.24)),
                new Proj4Spheroid("lerch","Lerch 1979",new SpheroidEquatorialInvF(6378139,298.257)),
                new Proj4Spheroid("mprts","Maupertius 1738",new SpheroidEquatorialInvF(6397300,191)),
                new Proj4Spheroid("new_intl","New International 1967",new SpheroidEquatorialPolar(6378157.5,6356772.2)),
                new Proj4Spheroid("plessis","Plessis 1817 (France)",new SpheroidEquatorialPolar(6376523,6355863)),
                new Proj4Spheroid("SEasia","Southeast Asia",new SpheroidEquatorialPolar(6378155.0,6356773.3205)),
                new Proj4Spheroid("walbeck","Walbeck",new SpheroidEquatorialPolar(6376896.0,6355834.8467)),
                new Proj4Spheroid("WGS60","WGS 60",new SpheroidEquatorialInvF(6378165.0,298.3)),
                new Proj4Spheroid("WGS66","WGS 66",new SpheroidEquatorialInvF(6378145.0,298.25)),
                new Proj4Spheroid("WGS72","WGS 72",new SpheroidEquatorialInvF(6378135.0,298.26)),
                new Proj4Spheroid("WGS84","WGS 84",new SpheroidEquatorialInvF(6378137.0,298.257223563)),
                new Proj4Spheroid("sphere","Normal Sphere (r=6370997)", new Sphere(6370997.0))
            });
        }

        /// <summary>
        /// All Proj4 ellipses.
        /// </summary>
        public static IList<Proj4Spheroid> Spheroids {
            get { return DefaultSpheroids; }
        }

        /// <summary>
        /// Gets an ellipse by its name or codename.
        /// </summary>
        /// <param name="name">The name or codename to search for.</param>
        /// <returns>An ellipse; null on failure.</returns>
        public static Proj4Spheroid GetSpheroid(string name) {
            return DefaultSpheroids.FirstOrDefault(e => e.Code.Equals(name) || e.Name.Equals(name));
        }

        private readonly string _code;
        private readonly ISpheroid<double> _spheroid;

        private Proj4Spheroid(string code, string name, ISpheroid<double> spheroid) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(spheroid != null);
            _code = code;
            Name = name;
            _spheroid = spheroid;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(_spheroid != null);
        }

        public string Name { get; private set; }

        public IUnit AxisUnit { get { return OgcLinearUnit.DefaultMeter; } }

        /// <summary>
        /// The ellipse codename.
        /// </summary>
        public string Code {
            get { return _code; }
        }

        public double A {
            get { return _spheroid.A; }
        }

        public double B {
            get { return _spheroid.B; }
        }

        public double F {
            get { return _spheroid.F; }
        }

        public double InvF {
            get { return _spheroid.InvF; }
        }

        public double E {
            get { return _spheroid.E; }
        }

        public double ESquared {
            get { return _spheroid.ESquared; }
        }

        public double ESecond {
            get { return _spheroid.ESecond; }
        }

        public double ESecondSquared {
            get { return _spheroid.ESecondSquared; }
        }

        public bool Equals(ISpheroid<double> other) {
            return _spheroid.Equals(other);
        }

        public IAuthorityTag Authority {
            get { return new AuthorityTag("PROJ4", Code); }
        }

    }
}
