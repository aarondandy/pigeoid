using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid
{
	/// <summary>
	/// A named parameter.
	/// </summary>
	/// <typeparam name="TValue">The value type of the parameter.</typeparam>
	public class NamedParameter<TValue> : INamedParameter
	{

		/// <summary>
		/// The parameter name.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _name;

		/// <summary>
		/// The parameter value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly TValue _value;

		public NamedParameter(string name, TValue value) {
			_name = name;
			_value = value;
		}

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// Parameter value.
		/// </summary>
		public TValue Value { get { return _value; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object INamedParameter.Value { get { return _value; } }

	}
}
