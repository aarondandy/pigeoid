using System.Diagnostics.Contracts;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
    public class ScaleUnitPartsPerMillion : IUnit
    {

        public static readonly ScaleUnitPartsPerMillion Value = new ScaleUnitPartsPerMillion();

        private ScaleUnitPartsPerMillion() { }

        public string Name {
            get { return "parts per million"; }
        }

        public string Type {
            get { return "Scale"; }
        }

        public IUnitConversionMap<double> ConversionMap {
            get {
                Contract.Ensures(Contract.Result<IUnitConversionMap<double>>() != null);
                return BasicScaleUnitConversionMap.Default;
            }
        }
    }
}
