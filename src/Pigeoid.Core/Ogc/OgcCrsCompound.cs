﻿// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A compound CRS composed of a head and tail CRS.
	/// </summary>
	public sealed class OgcCrsCompound : OgcNamedAuthorityBoundEntity, ICrsCompound
	{

		private readonly ICrs _head;
		private readonly ICrs _tail;

		/// <summary>
		/// Creates a new compound CRS.
		/// </summary>
		/// <param name="name">The name of the compound.</param>
		/// <param name="head">The head CRS.</param>
		/// <param name="tail">The tail CRS.</param>
		/// <param name="authority">The authority.</param>
		public OgcCrsCompound(
			string name,
			ICrs head,
			ICrs tail,
			IAuthorityTag authority
		) : base(name, authority) {
			_head = head;
			_tail = tail;
		}

		/// <summary>
		/// The head CRS.
		/// </summary>
		public ICrs Head { get { return _head; } }

		/// <summary>
		/// The tail CRS.
		/// </summary>
		public ICrs Tail { get { return _tail; } }

	}
}
