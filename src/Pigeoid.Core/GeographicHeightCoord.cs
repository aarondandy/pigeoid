// TODO: source header

using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public struct GeographicHeightCoord :
		IGeographicHeightCoord<double>,
		IEquatable<GeographicHeightCoord>,
		IComparable<GeographicHeightCoord>
	{

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GeographicHeightCoord a, GeographicHeightCoord b) {
			return a.Equals(b);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">A coordinate.</param>
		/// <param name="b">A coordinate.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GeographicHeightCoord a, GeographicHeightCoord b) {
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
		/// The height above the reference surface.
		/// </summary>
		public readonly double Height;

		/// <summary>
		/// Creates a new geographical coordinate.
		/// </summary>
		/// <param name="lat">The latitude.</param>
		/// <param name="lon">The longitude.</param>
		/// <param name="h">The height above the reference surface.</param>
		public GeographicHeightCoord(double lat, double lon, double h)
		{
			Latitude = lat;
			Longitude = lon;
			Height = h;
		}

		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		double IGeographicHeightCoord<double>.Height {
			get { return Height; }
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
		public int CompareTo(GeographicHeightCoord other) {
			int c = Longitude.CompareTo(other.Longitude);
			if (0 != c)
				return c;			
			c = Latitude.CompareTo(other.Latitude);
			return 0 == c ? Height.CompareTo(other.Height) : c;
		}

		/// <inheritdoc/>
		public bool Equals(GeographicHeightCoord other) {
			return Latitude == other.Latitude
				&& Longitude == other.Longitude
				&& Height == other.Height;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj) {
			return obj is GeographicHeightCoord && Equals((GeographicHeightCoord)obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			return Longitude.GetHashCode();
		}

	}
}
