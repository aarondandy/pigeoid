// TODO: source header

using System;
using Pigeoid.Contracts;
using JetBrains.Annotations;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A fitted CRS.
	/// </summary>
	public class OgcCrsFitted : OgcNamedAuthorityBoundEntity, ICrsFitted
	{

		//private readonly string _name;
		private readonly ICoordinateOperationInfo _toBaseOperation;
		private readonly ICrs _baseCrs;

		/// <summary>
		/// Constructs a new fitted CRS.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="toBaseOperation">The operation which converts to <paramref name="baseCrs"/>.</param>
		/// <param name="baseCrs">The base CRS.</param>
		/// <param name="authority">The authority code of the CRS.</param>
		public OgcCrsFitted(
			string name,
			[NotNull] ICoordinateOperationInfo toBaseOperation,
			[NotNull] ICrs baseCrs,
			IAuthorityTag authority = null
		) :base(name,authority) {
			if (null == toBaseOperation)
				throw new ArgumentNullException("toBaseOperation");
			if (null == baseCrs)
				throw new ArgumentNullException("baseCrs");

			_toBaseOperation = toBaseOperation;
			_baseCrs = baseCrs;
		}

		/// <inheritdoc/>
		public ICrs BaseCrs {
			get { return _baseCrs; }
		}

		/// <inheritdoc/>
		public ICoordinateOperationInfo ToBaseOperation {
			get { return _toBaseOperation; }
		}

	}
}
