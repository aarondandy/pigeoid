using System;
using System.Collections.Generic;
using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{

    [Obsolete("Maybe this should be in Vertesaur?")]
    public interface IConcatenatedOperation : ITransformation
    {
        IEnumerable<ITransformation> Transformations { get; }
    }

    public interface IConcatenatedOperation<TFrom, TTo> :
        IConcatenatedOperation,
        ITransformation<TFrom, TTo>
    {
    }

    public interface IConcatenatedOperation<TValue> :
        IConcatenatedOperation<TValue, TValue>,
        ITransformation<TValue>
    {
    }

}
