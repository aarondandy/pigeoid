using Pigeoid.Contracts;

namespace Pigeoid.Core.Test.Mock
{
	public class MockDatum : IDatum
	{
		public string Name { get; set; }

		public string Type { get; set; }

		public IAuthorityTag Authority { get; set; }
	}
}
