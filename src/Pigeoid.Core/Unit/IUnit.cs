using System.Diagnostics.Contracts;

namespace Pigeoid.Unit
{
    /// <summary>
    /// A unit of measure.
    /// </summary>
    [ContractClass(typeof(IUnitCodeContracts))]
    public interface IUnit
    {
        /// <summary>
        /// The name of the unit.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the unit type or category for this unit of measure.
        /// </summary>
        string Type { get; }

        IUnitConversionMap<double> ConversionMap { get; }
    }

    [ContractClassFor(typeof(IUnit))]
    internal abstract class IUnitCodeContracts : IUnit
    {

        public string Name {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        public string Type {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new System.NotImplementedException();
            }
        }

        public abstract IUnitConversionMap<double> ConversionMap { get; }
    }

}
