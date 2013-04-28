using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;

namespace Pigeoid.CoordinateOperation
{
    public class CoordinateOperationInfo : IParameterizedCoordinateOperationInfo, IAuthorityBoundEntity
    {

        private static readonly ReadOnlyCollection<INamedParameter> EmptyParameterList = Array.AsReadOnly(new INamedParameter[0]);

        public CoordinateOperationInfo(
            string name,
            IEnumerable<INamedParameter> parameters = null,
            ICoordinateOperationMethodInfo method = null,
            IAuthorityTag authority = null,
            bool hasInverse = true
        ) {
            Name = name ?? String.Empty;
            Parameters = null == parameters
                ? EmptyParameterList
                : Array.AsReadOnly(parameters.ToArray());
            HasInverse = hasInverse;
            Method = method;
            Authority = authority;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Name != null);
            Contract.Invariant(Parameters != null);
        }

        public string Name { get; protected set; }

        public ReadOnlyCollection<INamedParameter> Parameters { get; private set; }

        IEnumerable<INamedParameter> IParameterizedCoordinateOperationInfo.Parameters { get { return Parameters; } }

        public bool HasInverse { get; protected set; }

        public ICoordinateOperationInfo GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return new CoordinateOperationInfoInverse(this);
        }

        public bool IsInverseOfDefinition { [Pure] get { return false; } }

        public ICoordinateOperationMethodInfo Method { get; private set; }

        public IAuthorityTag Authority { get; private set; }

    }

}
