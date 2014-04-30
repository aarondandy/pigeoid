using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.CoordinateOperation
{
    public interface IParameterizedCoordinateOperationInfo : ICoordinateOperationInfo
    {

        /// <summary>
        /// The operation parameters.
        /// </summary>
        IEnumerable<INamedParameter> Parameters { get; }

        ICoordinateOperationMethodInfo Method { get; }

    }

    internal abstract class ParameterizedCoordinateOperationInfoContracts : IParameterizedCoordinateOperationInfo
    {

        public IEnumerable<INamedParameter> Parameters {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<INamedParameter>>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract ICoordinateOperationMethodInfo Method { get; }

        public abstract string Name { get; }

        public abstract bool HasInverse { get; }

        public abstract ICoordinateOperationInfo GetInverse();

        public abstract bool IsInverseOfDefinition { get; }
    }

}
