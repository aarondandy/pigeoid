using System.Diagnostics.Contracts;

namespace Pigeoid.Unit
{
    public class ScaleUnitUnity : IUnit
    {

        public static readonly ScaleUnitUnity Value = new ScaleUnitUnity();

        private ScaleUnitUnity() { }

        public string Name {
            get { return "unity"; }
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
