namespace Pigeoid.Contracts
{
    /// <summary>
    /// An axis with a name and a general orientation.
    /// </summary>
    public interface IAxis
    {
        /// <summary>
        /// The name of the axis.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The orientation of the axis.
        /// </summary>
        string Orientation { get; }

    }
}
