using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;

namespace Pigeoid.CoordinateOperation
{
    public class ConcatenatedCoordinateOperationInfo : IConcatenatedCoordinateOperationInfo
    {

        public static IEnumerable<ICoordinateOperationInfo> LinearizeOperations(ICoordinateOperationInfo operation) {
            Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
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

        /// <exception cref="System.ArgumentException">No coordinate operations given.</exception>
        /// <exception cref="System.ArgumentException">Null coordinate operations are not allowed.</exception>
        public ConcatenatedCoordinateOperationInfo(IEnumerable<ICoordinateOperationInfo> coordinateOperations) {
            if (null == coordinateOperations) throw new ArgumentNullException("coordinateOperations");
            Contract.EndContractBlock();

            RawCoordinateOperations = coordinateOperations.ToArray();
            if (RawCoordinateOperations.Length < 1)
                throw new ArgumentException("coordinateOperations must have at least one element", "coordinateOperations");
            if(!Array.TrueForAll(RawCoordinateOperations, x => x != null))
                throw new ArgumentException("Null coordinate operations are not allowed.","coordinateOperations");
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(RawCoordinateOperations != null);
            Contract.Invariant(RawCoordinateOperations.Length >= 1);
            Contract.Invariant(Contract.ForAll(RawCoordinateOperations, x => x != null));
        }

        private ICoordinateOperationInfo[] RawCoordinateOperations { get; set; }

        string ICoordinateOperationInfo.Name {
            get { return "Concatenated Operation"; }
        }

        public bool HasInverse {
            [Pure] get {
                return Array.TrueForAll(RawCoordinateOperations, x => x.HasInverse);
            }
        }

        public ICoordinateOperationInfo GetInverse() {
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return new Inverse(this);
        }

        public IEnumerable<ICoordinateOperationInfo> Steps {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>().Any());
                return RawCoordinateOperations.AsReadOnly();
            }
        }

        bool ICoordinateOperationInfo.IsInverseOfDefinition { get { return false; } }

    }
}
