using System.Diagnostics.Contracts;
using Pigeoid.Unit;

namespace Pigeoid
{
    /// <summary>
    /// A prime meridian.
    /// </summary>
    [ContractClass(typeof(IPrimeMeridianInfoCodeContracts))]
    public interface IPrimeMeridianInfo : INamedAuthorityBoundEntity
    {
        /// <summary>
        /// The longitude of the prime meridian.
        /// </summary>
        double Longitude { get; }
        /// <summary>
        /// The unit of measure for the longitude.
        /// </summary>
        IUnit Unit { get; }
    }

    [ContractClassFor(typeof(IPrimeMeridianInfo))]
    internal abstract class IPrimeMeridianInfoCodeContracts : IPrimeMeridianInfo
    {

        public abstract double Longitude { get; }

        public IUnit Unit {
            get {
                Contract.Ensures(Contract.Result<IUnit>() != null);
                throw new System.NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
