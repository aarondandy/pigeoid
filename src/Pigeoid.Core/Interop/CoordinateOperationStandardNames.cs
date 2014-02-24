using System;
using System.Collections.Generic;
using System.Linq;

namespace Pigeoid.Interop
{
    public static class CoordinateOperationStandardNames
    {
        public static readonly string AlbersEqualAreaConic = "Albers Equal-Area Conic";
        public static readonly string AzimuthalEquidistant = "Azimuthal Equidistant";
        public static readonly string BipolarObliqueConformalConic = "Bipolar Oblique Conformal Conic";
        public static readonly string CassiniSoldner = "Cassini-Soldner";
        public static readonly string CrasterParabolic = "Craster Parabolic";
        public static readonly string CylindricalEqualArea = "Cylindrical Equal Area";
        public static readonly string Eckert1 = "Eckert I";
        public static readonly string Eckert2 = "Eckert II";
        public static readonly string Eckert3 = "Eckert III";
        public static readonly string Eckert4 = "Eckert IV";
        public static readonly string Eckert5 = "Eckert V";
        public static readonly string Eckert6 = "Eckert VI";
        public static readonly string EquidistantConic = "Equidistant Conic";
        public static readonly string EquidistantCylindrical = "Equidistant Cylindrical";
        public static readonly string EquidistantCylindricalSpherical = "Equidistant Cylindrical (Spherical)";
        public static readonly string Equirectangular = "Equirectangular";
        public static readonly string Foucaut = "Foucaut";
        public static readonly string GallStereographic = "Gall Stereographic";
        public static readonly string GeographicOffsets = "Geographic2D Offsets";
        public static readonly string Geos = "Geostationary Satellite View";
        public static readonly string GeneralSinusoidal = "General Sinusoidal";
        public static readonly string Guam = "Guam";
        public static readonly string Gnomonic = "Gnomonic";
        public static readonly string GoodeHomolosine = "Goode Homolosine";
        public static readonly string HammerAitoff = "Hammer Aitoff";
        public static readonly string HotineObliqueMercator = "Hotine Oblique Mercator";
        public static readonly string HyperbolicCassiniSoldner = "Hyperbolic Cassini-Soldner";
        public static readonly string Kavraisky5 = "Kavraisky V";
        public static readonly string Kavraisky7 = "Kavraisky VII";
        public static readonly string Krovak = "Krovak";
        public static readonly string KrovakNorth = "Krovak (North Orientated)";
        public static readonly string KrovakModified = "Krovak Modified";
        public static readonly string KrovakModifiedNorth = "Krovak Modified (North Orientated)";
        public static readonly string KrovakObliqueConicConformal = "Krovak Oblique Conic Conformal";
        public static readonly string LabordeObliqueMercator = "Laborde Oblique Mercator";
        public static readonly string LambertAzimuthalEqualArea = "Lambert Azimuthal Equal Area";
        public static readonly string LambertAzimuthalEqualAreaSpherical = "Lambert Azimuthal Equal Area Spherical";
        public static readonly string LambertConicConformal1Sp = "Lambert Conic Conformal (1SP)";
        public static readonly string LambertConicConformal2Sp = "Lambert Conic Conformal (2SP)";
        public static readonly string LambertConicConformal2SpBelgium = "Lambert Conic Conformal (2SP Belgium)";
        public static readonly string LambertConicNearConformal = "Lambert Conic Near Conformal";
        public static readonly string LambertCylindricalEqualAreaSpherical = "Lambert Cylindrical Equal Area (Spherical)";
        public static readonly string LambertEqualAreaConic = "Lambert Equal Area Conic";
        public static readonly string Loximuthal = "Loximuthal";
        public static readonly string McBrydeThomasFlatPolarSine = "McBryde Thomas Flat Polar Sine";
        public static readonly string Mercator1Sp = "Mercator (1SP)";
        public static readonly string Mercator2Sp = "Mercator (2SP)";
        public static readonly string MercatorAuxiliarySphere = "Mercator Auxiliary Sphere";
        public static readonly string MillerCylindrical = "Miller Cylindrical";
        public static readonly string ModifiedAzimuthalEquidistant = "Modified Azimuthal Equidistant";
        public static readonly string Mollweide = "Mollweide";
        public static readonly string NewZealandMapGrid = "New Zealand Map Grid";
        public static readonly string ObliqueMercator = "Oblique Mercator";
        public static readonly string ObliqueStereographic = "Oblique Stereographic";
        public static readonly string ObliqueCylindricalEqualArea = "Oblique Cylindrical Equal Area";
        public static readonly string Orthographic = "Orthographic";
        public static readonly string PolarStereographic = "Polar Stereographic";
        public static readonly string Polyconic = "Polyconic";
        public static readonly string PopularVisualisationPseudoMercator = "Popular Visualisation Pseudo Mercator";
        public static readonly string PutinsP1 = "Putins P1";
        public static readonly string QuarticAuthalic = "Quartic Authalic";
        public static readonly string Robinson = "Robinson";
        public static readonly string RosenmundObliqueMercator = "Rosenmund Oblique Mercator";
        public static readonly string Sinusoidal = "Sinusoidal";
        public static readonly string SwissObliqueCylindrical = "Swiss Oblique Cylindrical";
        public static readonly string Stereographic = "Stereographic";
        public static readonly string TransverseMercator = "Transverse Mercator";
        public static readonly string TransverseMercatorZonedGridSystem = "Transverse Mercator Zoned Grid System";
        public static readonly string TransverseMercatorSouthOriented = "Transverse Mercator (South Orientated)";
        public static readonly string TunisiaMiningGrid = "Tunisia Mining Grid";
        public static readonly string TwoPointEquidistant = "Two Point Equidistant";
        public static readonly string VanDerGrinten = "VanDerGrinten";
        public static readonly string Wagner4 = "Wagner IV";
        public static readonly string Wagner5 = "Wagner V";
        public static readonly string Wagner6 = "Wagner VI";
        public static readonly string Winkel1 = "Winkel I";
        public static readonly string Winkel2 = "Winkel II";
        public static readonly string WinkelTripel = "Winkel Tripel";

        private static readonly HashSet<string> AllNames;
        private static readonly HashSet<string> NormalizedNames;

        static CoordinateOperationStandardNames() {
            AllNames = new HashSet<string> {
                AlbersEqualAreaConic,
                AzimuthalEquidistant,
                BipolarObliqueConformalConic,
                CassiniSoldner,
                CrasterParabolic,
                CylindricalEqualArea,
                Eckert1,
                Eckert2,
                Eckert3,
                Eckert4,
                Eckert5,
                Eckert6,
                EquidistantConic,
                EquidistantCylindrical,
                EquidistantCylindricalSpherical,
                Equirectangular,
                Foucaut,
                GallStereographic,
                GeographicOffsets,
                Geos,
                GeneralSinusoidal,
                Gnomonic,
                Guam,
                GoodeHomolosine,
                HammerAitoff,
                HotineObliqueMercator,
                HyperbolicCassiniSoldner,
                Kavraisky5,
                Kavraisky7,
                Krovak,
                KrovakModified,
                KrovakModifiedNorth,
                KrovakNorth,
                KrovakObliqueConicConformal,
                LabordeObliqueMercator,
                LambertAzimuthalEqualArea,
                LambertAzimuthalEqualAreaSpherical,
                LambertConicConformal1Sp,
                LambertConicConformal2Sp,
                LambertConicConformal2SpBelgium,
                LambertConicNearConformal,
                LambertCylindricalEqualAreaSpherical,
                LambertEqualAreaConic,
                Mercator1Sp,
                Mercator2Sp,
                MercatorAuxiliarySphere,
                MillerCylindrical,
                ModifiedAzimuthalEquidistant,
                Mollweide,
                NewZealandMapGrid,
                ObliqueMercator,
                ObliqueStereographic,
                ObliqueCylindricalEqualArea,
                Orthographic,
                PolarStereographic,
                Polyconic,
                PopularVisualisationPseudoMercator,
                PutinsP1,
                QuarticAuthalic,
                Robinson,
                RosenmundObliqueMercator,
                Sinusoidal,
                SwissObliqueCylindrical,
                Stereographic,
                TransverseMercator,
                TransverseMercatorZonedGridSystem,
                TransverseMercatorSouthOriented,
                TunisiaMiningGrid,
                TwoPointEquidistant,
                VanDerGrinten,
                Wagner4,
                Wagner5,
                Wagner6,
                Winkel1,
                Winkel2,
                WinkelTripel
            };
            NormalizedNames = new HashSet<string>(AllNames.Select(NameNormalizedComparerBase.NormalizeBasic), StringComparer.OrdinalIgnoreCase);
        }

        internal static bool IsNormalizedName(string text) {
            return NormalizedNames.Contains(text);
        }
    }
}
