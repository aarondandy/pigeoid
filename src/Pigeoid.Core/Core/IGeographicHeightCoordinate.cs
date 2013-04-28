namespace Pigeoid
{
    public interface IGeographicHeightCoordinate<out TValue> : IGeographicCoordinate<TValue>
    {

        /// <summary>
        /// The height above the reference surface.
        /// </summary>
        double Height { get; }

    }
}
