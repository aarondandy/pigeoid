// TODO: source header

using System;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A fitted CRS.
	/// </summary>
	public class OgcCrsFitted : ICrsFitted
	{

		private readonly string _name;
		private readonly ITransformation _toBaseOperation;
		private readonly ICrs _baseCrs;

		/// <summary>
		/// Constructs a new fitted CRS.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="toBaseOperation">The operation which converts to <paramref name="baseCrs"/>.</param>
		/// <param name="baseCrs">The base CRS.</param>
		public OgcCrsFitted(string name, ITransformation toBaseOperation, ICrs baseCrs) {
			if (null == toBaseOperation)
				throw new ArgumentNullException("toBaseOperation");
			if (null == baseCrs)
				throw new ArgumentNullException("baseCrs");

			_name = name;
			_toBaseOperation = toBaseOperation;
			_baseCrs = baseCrs;
		}

		/// <summary>
		/// The name of the CRS.
		/// </summary>
		public string Name {
			get { return _name; }
		}

		/// <inheritdoc/>
		public ICrs BaseCrs {
			get { return _baseCrs; }
		}

		/// <inheritdoc/>
		public ITransformation ToBaseOperation {
			get { return _toBaseOperation; }
		}

	}
}
