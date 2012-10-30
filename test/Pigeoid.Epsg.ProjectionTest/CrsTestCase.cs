using Pigeoid.Contracts;

namespace Pigeoid.Epsg.ProjectionTest
{
	public class CrsTestCase
	{

		public CrsTestCase(ICrs from, ICrs to) {
			From = from;
			To = to;
		}

		public ICrs From { get; private set; }
		public ICrs To { get; private set; }

		public override string ToString() {
			return string.Format("{0} to {1}", From, To);
		}

		public void Run(EpsgCrsCoordinateOperationPathGenerator generator = null) {
			if(null == generator)
				generator = new EpsgCrsCoordinateOperationPathGenerator();

			var path = generator.Generate(From, To);
		}

	}
}
