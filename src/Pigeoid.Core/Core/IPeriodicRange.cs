namespace Pigeoid.Core
{
    public interface IPeriodicRange<out T>
    {
        T Start { get; }

        T End { get; }

        bool Intersects(IPeriodicRange<double> other);

        IPeriodicRange<double> Intersection(IPeriodicRange<double> other);

    }
}
