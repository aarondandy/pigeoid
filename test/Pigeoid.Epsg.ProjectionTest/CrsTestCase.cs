using Pigeoid.Contracts;

namespace Pigeoid.Epsg.ProjectionTest
{
	public class CrsTestCase
	{
		public ICrs From { get; set; }
		public ICrs To { get; set; }

		public override string ToString() {
			return string.Format("{0} to {1}", From, To);
		}

		public void Run() {
			var generator = new EpsgCrsCoordinateOperationPathGenerator();
			var path = generator.Generate(From, To);
		}

	}
}
