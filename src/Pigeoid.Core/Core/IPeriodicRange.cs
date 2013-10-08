﻿namespace Pigeoid.Core
{
    public interface IPeriodicRange<out T>
    {
        T Start { get; }

        T End { get; }

    }
}
