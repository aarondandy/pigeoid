using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur.Transformation;

namespace Pigeoid.Unit
{

    [ContractClass(typeof(IUnitConversionCodeContracts<>))]
    public interface IUnitConversion<TValue> : ITransformation<TValue>
    {
        IUnit From { get; }
        IUnit To { get; }
        new IUnitConversion<TValue> GetInverse();
    }

    [ContractClassFor(typeof(IUnitConversion<>))]
    internal abstract class IUnitConversionCodeContracts<TValue> : IUnitConversion<TValue>
    {

        public IUnit From {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                throw new NotImplementedException();
            }
        }

        public IUnit To {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                throw new NotImplementedException();
            }
        }

        public IUnitConversion<TValue> GetInverse() {
            Contract.Requires(HasInverse);
            Contract.Ensures(Contract.Result<IUnitConversion<TValue>>() != null);
            throw new NotImplementedException();
        }

        ITransformation<TValue> ITransformation<TValue>.GetInverse() { return GetInverse(); }

        public abstract void TransformValues(TValue[] values);

        ITransformation<TValue, TValue> ITransformation<TValue, TValue>.GetInverse() { return GetInverse(); }

        public abstract TValue TransformValue(TValue value);

        public abstract IEnumerable<TValue> TransformValues(IEnumerable<TValue> values);

        ITransformation ITransformation.GetInverse() { return GetInverse(); }

        public abstract bool HasInverse { get; }

        public abstract Type[] GetInputTypes();

        public abstract Type[] GetOutputTypes(Type inputType);

        public abstract object TransformValue(object value);

        public abstract IEnumerable<object> TransformValues(IEnumerable<object> values);
        
    }

    [ContractClass(typeof(IUnitScalarConversionCodeContracts<>))]
    public interface IUnitScalarConversion<TValue> : IUnitConversion<TValue>
    {
        TValue Factor { [Pure] get; }
        new IUnitScalarConversion<TValue> GetInverse();
    }

    [ContractClassFor(typeof(IUnitScalarConversion<>))]
    internal abstract class IUnitScalarConversionCodeContracts<TValue> : IUnitScalarConversion<TValue>
    {

        public abstract TValue Factor { [Pure] get; }

        public IUnitScalarConversion<TValue> GetInverse() {
            Contract.Requires(HasInverse);
            Contract.Ensures(Contract.Result<IUnitScalarConversion<TValue>>() != null);
            throw new NotImplementedException();
        }

        public abstract IUnit From { get; }

        public abstract IUnit To { get; }

        IUnitConversion<TValue> IUnitConversion<TValue>.GetInverse() { return GetInverse(); }

        ITransformation<TValue> ITransformation<TValue>.GetInverse() { return GetInverse(); }

        public abstract void TransformValues(TValue[] values);

        ITransformation<TValue, TValue> ITransformation<TValue, TValue>.GetInverse() { return GetInverse(); }

        public abstract TValue TransformValue(TValue value);

        public abstract IEnumerable<TValue> TransformValues(IEnumerable<TValue> values);

        ITransformation ITransformation.GetInverse() { return GetInverse(); }

        public abstract bool HasInverse { get; }

        public abstract Type[] GetInputTypes();

        public abstract Type[] GetOutputTypes(Type inputType);

        public abstract object TransformValue(object value);

        public abstract IEnumerable<object> TransformValues(IEnumerable<object> values);
    }

}
