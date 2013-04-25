using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using Pigeoid.Contracts;

namespace Pigeoid
{
    /// <summary>
    /// A geographical coordinate with height.
    /// </summary>
    public struct GeographicHeightCoordinate :
        IGeographicHeightCoordinate<double>,
        IEquatable<GeographicHeightCoordinate>,
        IComparable<GeographicHeightCoordinate>
    {

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">A coordinate.</param>
        /// <param name="b">A coordinate.</param>
        /// <returns>The result of the operator.</returns>
        [Pure] public static bool operator ==(GeographicHeightCoordinate a, GeographicHeightCoordinate b) {
            return a.Equals(b);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">A coordinate.</param>
        /// <param name="b">A coordinate.</param>
        /// <returns>The result of the operator.</returns>
        [Pure] public static bool operator !=(GeographicHeightCoordinate a, GeographicHeightCoordinate b) {
            return !a.Equals(b);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly GeographicHeightCoordinate Zero = new GeographicHeightCoordinate(0, 0, 0);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static readonly GeographicHeightCoordinate Invalid = new GeographicHeightCoordinate(Double.NaN, Double.NaN, Double.NaN);

        /// <summary>
        /// The latitude component of the coordinate.
        /// </summary>
        public readonly double Latitude;

        /// <summary>
        /// The longitude component of the coordinate.
        /// </summary>
        public readonly double Longitude;

        /// <summary>
        /// The height above the reference surface.
        /// </summary>
        public readonly double Height;

        /// <summary>
        /// Creates a new geographical coordinate with height.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="height">The height above the reference surface.</param>
        public GeographicHeightCoordinate(double latitude, double longitude, double height) {
            Latitude = latitude;
            Longitude = longitude;
            Height = height;
        }

        /// <summary>
        /// Creates a new geographical coordinate with default height.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <remarks>
        /// The default height is 0.
        /// </remarks>
        public GeographicHeightCoordinate(double latitude, double longitude) {
            Latitude = latitude;
            Longitude = longitude;
            Height = 0.0;
        }

        /// <summary>
        /// Creates a new geographical coordinate with height.
        /// </summary>
        /// <param name="coordinate">The latitude and longitude.</param>
        /// <param name="h">The height above the reference surface.</param>
        public GeographicHeightCoordinate(GeographicCoordinate coordinate, double h)
            : this(coordinate.Latitude, coordinate.Longitude, h) { }

        /// <inheritdoc/>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        double IGeographicHeightCoordinate<double>.Height {
            [Pure] get { return Height; }
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
        [Pure] public int CompareTo(GeographicHeightCoordinate other) {
            int c = Longitude.CompareTo(other.Longitude);
            if (0 != c)
                return c;
            c = Latitude.CompareTo(other.Latitude);
            return 0 == c ? Height.CompareTo(other.Height) : c;
        }

        /// <inheritdoc/>
        [Pure] public bool Equals(GeographicHeightCoordinate other) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return Latitude == other.Latitude && Longitude == other.Longitude && Height == other.Height;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <inheritdoc/>
        [Pure] public override bool Equals(object obj) {
            return obj is GeographicHeightCoordinate
                && Equals((GeographicHeightCoordinate)obj);
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
                Longitude.ToString(CultureInfo.InvariantCulture),
                " h:",
                Height.ToString(CultureInfo.InvariantCulture));
        }

    }
}
