using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Vertesaur;
using Vertesaur.Periodic;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordOpTestCase
    {

        public static double[] GetValues(double start, double end, int count) {
            var result = new double[count];
            var lastIndex = count - 1;
            var distance = end - start;
            for (int i = 1; i < lastIndex; i++) {
                var ratio = i / (double)(lastIndex);
                result[i] = start + (ratio * distance);
            }
            result[0] = start;
            result[lastIndex] = end;
            return result;
        }

        public static double[] GetValues(LongitudeDegreeRange range, int count) {
            var distance = range.GetMagnitude();
            var result = GetValues(range.Start, range.Start + distance, count);
            for (int i = 0; i < result.Length; i++)
                result[i] = LongitudeDegreeRange.DefaultPeriodicOperations.Fix(result[i]);

            return result;

        }

        public static double[] GetValues(IPeriodicRange<double> range, int count) {
            if (range is LongitudeDegreeRange)
                return GetValues((LongitudeDegreeRange) range, count);
            throw new NotImplementedException();
        }

        public static double[] GetValues(Range range, int count) {
            return GetValues(range.Low, range.High, count);
        }

        public static IEnumerable<GeographicCoordinate> CreateTestPoints(IGeographicMbr mbr, int lonValueCount = 10, int latValueCount = 10) {
            var lonValues = GetValues(mbr.LongitudeRange, lonValueCount);
            var latValues = GetValues(mbr.LatitudeRange, latValueCount);
            for (int r = 0; r < latValues.Length; r++)
                for (int c = 0; c < lonValues.Length; c++)
                    yield return new GeographicCoordinate(latValues[r], lonValues[c]);
        }

        [Obsolete]
        private Type GetCoordinateType(ICrs crs) {
            if (crs is ICrsGeocentric)
                return typeof(Point3);
            if (crs is ICrsGeographic)
                return typeof(GeographicCoordinate);
            if (crs is ICrsProjected)
                return typeof(Point2);
            return null;
        }

        public ICrs From { get; set; }
        public ICrs To { get; set; }
        public IGeographicMbr Area { get; set; }
        public ICoordinateOperationCrsPathInfo Path { get; set; }

        public void Execute() {
            var testPoints = CreateTestPoints(Area).ToArray();
        }


        public override string ToString() {
            return String.Format("{0} to {1}", From, To);
        }

    }
}
