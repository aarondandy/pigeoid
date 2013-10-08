using System;
using System.Collections.Generic;
using Pigeoid.Core;
using Pigeoid.Epsg;
using Vertesaur;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordinateOpTestCase
    {

        public double[] GetValues(double start, double end, int count) {
            var result = new double[count];
            var lastIndex = count - 1;
            var distance = end - start;
            for (int i = 1; i < lastIndex; i++) {
                var ratio = i / (double)(lastIndex);
                result[i] = ratio * distance;
            }
            result[0] = start;
            result[lastIndex] = end;
            return result;
        }

        public double[] GetValues(LongitudeDegreeRange range, int count) {
            return GetValues(range.Start, range.End, count);
        }

        public double[] GetValues(Range range, int count) {
            return GetValues(range.Low, range.High, count);
        }

        public IEnumerable<GeographicCoordinate> CreateTestPoints(IGeographicMbr mbr, int lonValueCount = 10, int latValueCount = 10) {
            var lonValues = GetValues(mbr.LongitudeRange.Start, mbr.LongitudeRange.End, lonValueCount);
            var latValues = GetValues(mbr.LatitudeRange.Low, mbr.LatitudeRange.High, latValueCount);
            for (int r = 0; r < latValues.Length; r++)
                for (int c = 0; c < lonValues.Length; c++)
                    yield return new GeographicCoordinate(latValues[r], lonValues[c]);
        }

        public CoordinateOpTestCase(EpsgCrs from, EpsgCrs to) {
            From = from;
            To = to;
        }

        public EpsgCrs From { get; private set; }
        public EpsgCrs To { get; private set; }

        public IGeographicMbr IntersectingArea {
            get { return From.Area.Intersection(To.Area); }
        }

        public override string ToString() {
            return string.Format("{0} to {1}", From, To);
        }

    }
}
