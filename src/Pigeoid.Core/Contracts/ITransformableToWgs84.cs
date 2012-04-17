
using System.Collections.Generic;
using Pigeoid.Transformation;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// Allows an entity or CRS to be transformed to WGS84.
	/// </summary>
	public interface ITransformableToWgs84
	{
		/// <summary>
		/// Determines if this CRS or entity can be transformed to WGS84.
		/// </summary>
		bool IsTransformableToWgs84 { get; }
		/// <summary>
		/// The transformation which can be used to convert this CRS or entity to WGS84.
		/// </summary>
		Helmert7Transformation PrimaryWgs84Transformation { get; }
		/// <summary>
		/// The transformations which can be used to convert this CRS or entity to WGS84.
		/// </summary>
		IEnumerable<Helmert7Transformation> Wgs84Transformations { get; }
	}
}
