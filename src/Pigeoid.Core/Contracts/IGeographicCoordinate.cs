namespace Pigeoid.Contracts
{
    /// <summary>
    /// A geographic coordinate.
    /// </summary>
    /// <typeparam name="TValue">The element type.</typeparam>
    public interface IGeographicCoordinate<out TValue>
    {

        /// <summary>
        /// The latitude component of the coordinate.
        /// </summary>
        TValue Latitude { get; }
        /// <summary>
        /// The longitude component of the coordinate.
        /// </summary>
        TValue Longitude { get; }

    }
}
