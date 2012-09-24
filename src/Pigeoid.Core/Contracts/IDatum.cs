// TODO: source header

namespace Pigeoid.Contracts
{
	public interface IDatum : IAuthorityBoundEntity
	{

		string Name { get; }
		string Type { get; }

	}
}
