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
		public ICoordinateOperationCrsPathInfo Operations { get; private set; }

		public override string ToString() {
			return string.Format("{0} to {1}", From, To);
		}

		public void Run(
			EpsgCrsCoordinateOperationPathGenerator pathGenerator = null,
			ICoordinateOperationToTransformationGenerator transformationGenerator = null
		) {
			if(null == pathGenerator)
				pathGenerator = new EpsgCrsCoordinateOperationPathGenerator();

			Operations = pathGenerator.Generate(From, To);

			if (null != Operations) {
				if (null == transformationGenerator)
					transformationGenerator = new BasicCoordinateOperationToTransformationGenerator();

				var transform = transformationGenerator.Create(Operations);
			}
		}

	}
}
