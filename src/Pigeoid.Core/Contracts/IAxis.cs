
namespace Pigeoid.Contracts
{
	public interface IAxis
	{
		/// <summary>
		/// The name of the axis.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// The orientation of the axis.
		/// </summary>
		string Direction { get; }

	}
}
