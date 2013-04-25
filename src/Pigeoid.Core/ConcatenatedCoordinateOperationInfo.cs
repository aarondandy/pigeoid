using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
    public class ConcatenatedCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
    {

        public static IEnumerable<ICoordinateOperationInfo> LinearizeOperations(ICoordinateOperationInfo operation) {
            if (null == operation)
                return Enumerable.Empty<ICoordinateOperationInfo>();
            var concatOperations = operation as IConcatenatedCoordinateOperationInfo;
            if (null == concatOperations)
                return new[] { operation };
            return concatOperations.Steps.SelectMany(LinearizeOperations);
        }

        private class Inverse : IConcatenatedCoordinateOperationInfo
        {

            private readonly ConcatenatedCoordinateOperationInfo _core;

            public Inverse(ConcatenatedCoordinateOperationInfo core) {
                Contract.Requires(core != null);
                _core = core;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_core != null);
            }

            public IEnumerable<ICoordinateOperationInfo> Steps {
                get {
                    Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                    Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
                    return _core.Steps.Reverse().Select(x => x.GetInverse());
                }
            }

            string ICoordinateOperationInfo.Name {
                get {
                    Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                    return "Inverse " + ((ICoordinateOperationInfo)_core).Name;
                }
            }

            public bool HasInverse { [Pure] get { return true; } }

            public ICoordinateOperationInfo GetInverse() {
                Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
                return _core;
            }

            public bool IsInverseOfDefinition { [Pure] get { return true; } }

        }

        private readonly ReadOnlyCollection<ICoordinateOperationInfo> _coordinateOperations;

        public ConcatenatedCoordinateOperationInfo(IEnumerable<ICoordinateOperationInfo> coordinateOperations) {
            if (null == coordinateOperations) throw new ArgumentNullException("coordinateOperations");
            Contract.EndContractBlock();

            _coordinateOperations = Array.AsReadOnly(coordinateOperations.ToArray());
            if (_coordinateOperations.Count == 0)
                throw new ArgumentException("coordinateOperations must have at least one element", "coordinateOperations");
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_coordinateOperations != null);
        }

        string ICoordinateOperationInfo.Name {
            get { return "Concatenated Operation"; }
        }

        public bool HasInverse {
            get { return _coordinateOperations.All(x => x.HasInverse); }
        }

        public ICoordinateOperationInfo GetInverse() {
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return new Inverse(this);
        }

        public IEnumerable<ICoordinateOperationInfo> Steps {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
                return _coordinateOperations;
            }
        }

        public bool IsInverseOfDefinition {
            [Pure] get { return false; }
        }

    }
}
