// TODO: source header

using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class PolarStereographic : ObliqueStereographic
	{

		public PolarStereographic(ISpheroid<double> spheroid)
			: base(spheroid) { }

		public override string Name {
			get { return "Polar Stereographic"; }
		}
	}
}
