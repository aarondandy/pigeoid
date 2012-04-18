// TODO: source header

using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public struct GeographicCoord :
		IGeographicCoord<double>,
		IEquatable<GeographicCoord>,
		IComparable<GeographicCoord>
	{

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GeographicCoord a, GeographicCoord b) {
			return a.Equals(b);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GeographicCoord a, GeographicCoord b) {
			return !a.Equals(b);
		}

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
		/// <param name="lat">The latitude.</param>
		/// <param name="lon">The longitude.</param>
		public GeographicCoord(double lat, double lon)
		{
			Latitude = lat;
			Longitude = lon;
		}

		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		double IGeographicCoord<double>.Latitude {
			get { return Latitude; }
		}

		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		double IGeographicCoord<double>.Longitude {
			get { return Longitude; }
		}

		/// <inheritdoc/>
		public int CompareTo(GeographicCoord other) {
			int c = Longitude.CompareTo(other.Longitude);
			return 0 == c ? Latitude.CompareTo(other.Latitude) : c;
		}

		/// <inheritdoc/>
		public bool Equals(GeographicCoord other) {
			return Latitude == other.Latitude
				&& Longitude == other.Longitude;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) {
			return obj is GeographicCoord && Equals((GeographicCoord) obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			return Longitude.GetHashCode();
		}

	}
}
