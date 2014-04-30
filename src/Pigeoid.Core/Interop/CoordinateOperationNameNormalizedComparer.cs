using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Pigeoid.Interop
{
    public class CoordinateOperationNameNormalizedComparer : NameNormalizedComparerBase
    {

        private static readonly Regex VariantEndingRegex = new Regex("VARIANT([A-Z])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly CoordinateOperationNameNormalizedComparer Default = new CoordinateOperationNameNormalizedComparer();

        private static readonly Dictionary<string, string> _proj4Abbreviations =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"AEA", CoordinateOperationStandardNames.AlbersEqualAreaConic},
                {"AEQD", CoordinateOperationStandardNames.AzimuthalEquidistant},
                {"BIPC", CoordinateOperationStandardNames.BipolarObliqueConformalConic},
                {"CASS", CoordinateOperationStandardNames.CassiniSoldner},
                {"CASSINI", CoordinateOperationStandardNames.CassiniSoldner},
                {"CEA", CoordinateOperationStandardNames.CylindricalEqualArea},
                {"CRAST", CoordinateOperationStandardNames.CrasterParabolic},
                {"ECK1", CoordinateOperationStandardNames.Eckert1},
                {"ECK2", CoordinateOperationStandardNames.Eckert2},
                {"ECK3", CoordinateOperationStandardNames.Eckert3},
                {"ECK4", CoordinateOperationStandardNames.Eckert4},
                {"ECK5", CoordinateOperationStandardNames.Eckert5},
                {"ECK6", CoordinateOperationStandardNames.Eckert6},
                {"EQC", CoordinateOperationStandardNames.EquidistantCylindrical},
                {"EQDC", CoordinateOperationStandardNames.EquidistantConic},
                {"FOUC", CoordinateOperationStandardNames.Foucaut},
                {"GALL", CoordinateOperationStandardNames.GallStereographic},
                {"GNSINU", CoordinateOperationStandardNames.GeneralSinusoidal},
                {"GEOS", CoordinateOperationStandardNames.Geos},
                {"GNOM", CoordinateOperationStandardNames.Gnomonic},
                {"GOODE", CoordinateOperationStandardNames.GoodeHomolosine},
                {"HAMMER", CoordinateOperationStandardNames.HammerAitoff},
                {"OMERC", CoordinateOperationStandardNames.ObliqueMercator},
                {"KAV5", CoordinateOperationStandardNames.Kavraisky5},
                {"KAV7", CoordinateOperationStandardNames.Kavraisky7},
                {"LAEA", CoordinateOperationStandardNames.LambertAzimuthalEqualArea},
                {"LCC", CoordinateOperationStandardNames.LambertConicConformal2Sp},
                {"LEAC", CoordinateOperationStandardNames.LambertEqualAreaConic},
                {"LOXIM", CoordinateOperationStandardNames.Loximuthal},
                {"MBTS", CoordinateOperationStandardNames.McBrydeThomasFlatPolarSine},
                {"MERC", CoordinateOperationStandardNames.Mercator2Sp},
                {"MILL", CoordinateOperationStandardNames.MillerCylindrical},
                {"MOLL", CoordinateOperationStandardNames.Mollweide},
                {"NZMG", CoordinateOperationStandardNames.NewZealandMapGrid},
                {"STEREA", CoordinateOperationStandardNames.ObliqueStereographic},
                {"OCEA", CoordinateOperationStandardNames.ObliqueCylindricalEqualArea},
                {"ORTHO", CoordinateOperationStandardNames.Orthographic},
                {"POLY", CoordinateOperationStandardNames.Polyconic},
                {"PUTP1",CoordinateOperationStandardNames.PutinsP1},
                {"QUAAUT", CoordinateOperationStandardNames.QuarticAuthalic},
                {"ROBIN", CoordinateOperationStandardNames.Robinson},
                {"SINU", CoordinateOperationStandardNames.Sinusoidal},
                {"STERE", CoordinateOperationStandardNames.Stereographic},
                {"SOMERC", CoordinateOperationStandardNames.SwissObliqueCylindrical},
                {"TPEQD", CoordinateOperationStandardNames.TwoPointEquidistant},
                {"UPS", CoordinateOperationStandardNames.PolarStereographic},
                {"UTM", CoordinateOperationStandardNames.TransverseMercator},
                {"VANDG", CoordinateOperationStandardNames.VanDerGrinten},
                {"WAG4", CoordinateOperationStandardNames.Wagner4},
                {"WAG5", CoordinateOperationStandardNames.Wagner5},
                {"WAG6", CoordinateOperationStandardNames.Wagner6},
                {"WINK1", CoordinateOperationStandardNames.Winkel1},
                {"WINK2", CoordinateOperationStandardNames.Winkel2},
                {"WINTRI", CoordinateOperationStandardNames.WinkelTripel},
            };

        public CoordinateOperationNameNormalizedComparer() : this(null) { }

        public CoordinateOperationNameNormalizedComparer(StringComparer comparer) : base(comparer) { }

        public override string Normalize(string text) {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            text = base.Normalize(text);

            string abbreviationExpansion;
            if (_proj4Abbreviations.TryGetValue(text, out abbreviationExpansion))
                return base.Normalize(abbreviationExpansion);

            if (text == "GEOSTATIONARYSATELLITE")
                return base.Normalize(CoordinateOperationStandardNames.Geos);

            // take the 'orientated' off the end
            if (text.EndsWith("NORTHORIENTATED"))
                text = text.Substring(0, text.Length - 10);

            text = text.Replace("LONGITUDEROTATION", "GEOGRAPHICOFFSET");
            if (text.EndsWith("OFFSETS"))
                text = text.Substring(0, text.Length - 1);

            if (CoordinateOperationStandardNames.IsNormalizedName(text))
                return text;

            if (text.StartsWith("HOTINEOBLIQUEMERCATOR"))
                return base.Normalize(CoordinateOperationStandardNames.HotineObliqueMercator);

            if (text.EndsWith("LAMBERTCONFORMALCONIC2SP"))
                return base.Normalize(CoordinateOperationStandardNames.LambertConicConformal2Sp);

            if (text == "MERCATOR")
                return base.Normalize(CoordinateOperationStandardNames.Mercator2Sp);

            if (text.StartsWith("MILLERCYLINDRICAL"))
                return base.Normalize(CoordinateOperationStandardNames.MillerCylindrical);

            if (text == "OBLIQUESTEREOGRAPHICALTERNATIVE" || text == "DOUBLESTEREOGRAPHIC")
                return base.Normalize(CoordinateOperationStandardNames.ObliqueStereographic);

            if (text == "STEREOGRAPHICNORTHPOLE")
                return base.Normalize(CoordinateOperationStandardNames.Stereographic);

            if (text == "SWISSOBLIQUEMERCATOR")
                return base.Normalize(CoordinateOperationStandardNames.SwissObliqueCylindrical);

            if (text == "UNIVERSALPOLARSTEREOGRAPHIC")
                return base.Normalize(CoordinateOperationStandardNames.PolarStereographic);

            if (text == "VANDERGRINTEN1")
                return base.Normalize(CoordinateOperationStandardNames.VanDerGrinten);

            var orientated = text.Replace("ORIENTED", "ORIENTATED");
            if (CoordinateOperationStandardNames.IsNormalizedName(orientated))
                return orientated;

            var variantMatch = VariantEndingRegex.Match(text);
            if (variantMatch.Success) {
                Contract.Assume(variantMatch.Index < text.Length);
                var variantLetter = variantMatch.Groups[1].Value;
                var nonVariant = text.Substring(0, variantMatch.Index);

                if (variantLetter == "A") {
                    var sp1Replace = String.Concat(nonVariant, "1SP");
                    if (CoordinateOperationStandardNames.IsNormalizedName(sp1Replace))
                        return sp1Replace;
                }

                if (variantLetter == "B") {
                    var sp2Replace = String.Concat(nonVariant, "2SP");
                    if (CoordinateOperationStandardNames.IsNormalizedName(sp2Replace))
                        return sp2Replace;
                }

                if (CoordinateOperationStandardNames.IsNormalizedName(nonVariant))
                    return nonVariant;
            }

            if (text.EndsWith("PROJECTION")) {
                var removedProjectionText = text.Substring(0, text.Length - 10);
                if (CoordinateOperationStandardNames.IsNormalizedName(removedProjectionText))
                    return removedProjectionText;
            }

            if (text.EndsWith("SPHERICAL")) {
                if (text.StartsWith("MERCATOR")) {
                    var nonSpherical = text.Substring(0, text.Length - 9);
                    if (CoordinateOperationStandardNames.IsNormalizedName(nonSpherical))
                        return nonSpherical;
                }
            }

            var conicFlipText = text.Replace("CONFORMALCONIC", "CONICCONFORMAL");
            if (CoordinateOperationStandardNames.IsNormalizedName(conicFlipText))
                return conicFlipText;

            if (text.EndsWith("AREA")) {
                var areaConicText = text + "CONIC";
                if (CoordinateOperationStandardNames.IsNormalizedName(areaConicText))
                    return areaConicText;
            }

            if (text.Equals("GAUSSKRUGER"))
                return base.Normalize(CoordinateOperationStandardNames.TransverseMercator);

            if (text == "ALBERSCONICEQUALAREA" || text == "ALBERS")
                return base.Normalize(CoordinateOperationStandardNames.AlbersEqualAreaConic);

            return text;
        }

    }
}
