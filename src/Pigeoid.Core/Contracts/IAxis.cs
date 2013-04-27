using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Contracts
{
    /// <summary>
    /// An axis with a name and a general orientation.
    /// </summary>
    [ContractClass(typeof(IAxisCodeContracts))]
    public interface IAxis
    {
        /// <summary>
        /// The name of the axis.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The orientation of the axis.
        /// </summary>
        string Orientation { get; }
    }

    [ContractClassFor(typeof(IAxis))]
    internal abstract class IAxisCodeContracts : IAxis
    {

        public string Name {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }

        public string Orientation {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }
    }

}
