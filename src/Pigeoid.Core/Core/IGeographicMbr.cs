using Vertesaur;

namespace Pigeoid.Core
{
    public interface IGeographicMbr
    {

        IPeriodicRange<double> LongitudeRange { get; }

        Range LatitudeRange { get; }

    }
}
