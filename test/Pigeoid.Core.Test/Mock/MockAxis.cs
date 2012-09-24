using Pigeoid.Contracts;

namespace Pigeoid.Core.Test.Mock
{
	public class MockAxis : IAxis
	{
		public string Name { get; set; }

		public string Orientation { get; set; }
	}
}
