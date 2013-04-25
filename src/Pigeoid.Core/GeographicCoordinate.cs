using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using Pigeoid.Contracts;

namespace Pigeoid
{
    public struct GeographicCoordinate :
        IGeographicCoordinate<double>,
        IEquatable<GeographicCoordinate>,
        IComparable<GeographicCoordinate>
    {

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">A coordinate.</param>
        /// <param name="b">A coordinate.</param>
        /// <returns>The result of the operator.</returns>
        [Pure]
        public static bool operator ==(GeographicCoordinate a, GeographicCoordinate b) {
            return a.Equals(b);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">A coordinate.</param>
        /// <param name="b">A coordinate.</param>
        /// <returns>The result of the operator.</returns>
        [Pure]
        public static bool operator !=(GeographicCoordinate a, GeographicCoordinate b) {
            return !a.Equals(b);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly GeographicCoordinate Zero = new GeographicCoordinate(0, 0);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly GeographicCoordinate Invalid = new GeographicCoordinate(Double.NaN, Double.NaN);

        /// <summary>
        /// The latitude component of the coordinate.
        /// </summary>
        public readonly double Latitude;

        /// <summary>
        /// The longitude component of the coordinate.
        /// </summary>
        public readonly double Longitude;

        /// <summary>
        /// Creates a new geographical coordinate.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        public GeographicCoordinate(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <inheritdoc/>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        double IGeographicCoordinate<double>.Latitude {
            [Pure] get { return Latitude; }
        }

        /// <inheritdoc/>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        double IGeographicCoordinate<double>.Longitude {
            [Pure] get { return Longitude; }
        }

        /// <inheritdoc/>
        [Pure] public int CompareTo(GeographicCoordinate other) {
            int c = Longitude.CompareTo(other.Longitude);
            return 0 == c ? Latitude.CompareTo(other.Latitude) : c;
        }

        /// <inheritdoc/>
        [Pure] public bool Equals(GeographicCoordinate other) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return Latitude == other.Latitude && Longitude == other.Longitude;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <inheritdoc/>
        [Pure] public override bool Equals(object obj) {
            return obj is GeographicCoordinate
                && Equals((GeographicCoordinate)obj);
        }

        /// <inheritdoc/>
        [Pure] public override int GetHashCode() {
            return Longitude.GetHashCode();
        }

        /// <inheritdoc/>
        [Pure] public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return String.Concat(
                "lat:",
                Latitude.ToString(CultureInfo.InvariantCulture),
                " lon:",
                Longitude.ToString(CultureInfo.InvariantCulture));
        }

    }
}
