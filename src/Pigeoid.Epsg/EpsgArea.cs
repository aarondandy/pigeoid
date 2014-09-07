using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Core;
using Pigeoid.Epsg.Resources;
using Vertesaur;

namespace Pigeoid.Epsg
{
    public class EpsgArea :
        IGeographicMbr,
        IRelatableIntersects<EpsgArea>,
        IRelatableContains<EpsgArea>,
        IRelatableWithin<EpsgArea>
    {

        internal EpsgArea(ushort code, string name, string iso2, string iso3, LongitudeDegreeRange longitudeRange, Range latRange) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Code = code;
            Name = name;
            Iso2 = iso2;
            Iso3 = iso3;
            LongitudeRange = longitudeRange;
            LatitudeRange = latRange;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(Code >= 0);
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get; private set; }

        public string Name { get; private set; }

        public string Iso2 { get; private set; }

        public string Iso3 { get; private set; }

        public LongitudeDegreeRange LongitudeRange { get; private set; }

        IPeriodicRange<double> IGeographicMbr.LongitudeRange { get{ return LongitudeRange; } }

        public Range LatitudeRange { get; private set; }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(Code);
            }
        }

        public bool Intersects(IGeographicMbr other) {
            return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
        }

        public bool Intersects(EpsgArea other) {
            return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
        }

        public bool Contains(EpsgArea other) {
            return LongitudeRange.Contains(other.LongitudeRange)
                && LatitudeRange.Contains(other.LatitudeRange);
        }

        public bool Within(EpsgArea other) {
            return LongitudeRange.Within(other.LongitudeRange)
                && LatitudeRange.Within(other.LatitudeRange);
        }

        [Obsolete("A more generic replacement should be used.")]
        private class IntersectionResult : IGeographicMbr
        {

            public IPeriodicRange<double> LongitudeRange { get; set; }

            public Range LatitudeRange { get; set; }

            public bool Intersects(IGeographicMbr other) {
                return LongitudeRange.Intersects(other.LongitudeRange)
                && LatitudeRange.Intersects(other.LatitudeRange);
            }

            public IGeographicMbr Intersection(IGeographicMbr other) {
                if (!LatitudeRange.Intersects(other.LatitudeRange))
                    return null;

                if (LongitudeRange == null)
                    return null;

                var longitude = LongitudeRange.Intersection(other.LongitudeRange);
                if (longitude == null)
                    return null;

                var latitude = new Range(
                    Math.Max(LatitudeRange.Low, other.LatitudeRange.Low),
                    Math.Min(LatitudeRange.High, other.LatitudeRange.High));

                return new IntersectionResult {
                    LatitudeRange = latitude,
                    LongitudeRange = longitude
                };
            }
        }

        public IGeographicMbr Intersection(IGeographicMbr other) {
            if (!LatitudeRange.Intersects(other.LatitudeRange))
                return null;

            var longitude = LongitudeRange.Intersection(other.LongitudeRange);
            if(longitude == null)
                return null;

            var latitude = new Range(
                Math.Max(LatitudeRange.Low, other.LatitudeRange.Low),
                Math.Min(LatitudeRange.High, other.LatitudeRange.High));

            return new IntersectionResult {
                LatitudeRange = latitude,
                LongitudeRange = longitude
            };
        }

    }
}
