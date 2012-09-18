// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An OGC entity with a name and an authority tag.
	/// </summary>
	public abstract class OgcNamedAuthorityBoundEntity : IAuthorityBoundEntity
	{
		/// <summary>
		/// The entity name.
		/// </summary>
		private readonly string _name;
		private readonly IAuthorityTag _authorityTag;

		protected OgcNamedAuthorityBoundEntity(string name, IAuthorityTag authorityTag) {
			_name = name;
			_authorityTag = authorityTag;
		}

		public IAuthorityTag Authority {
			get { return _authorityTag; }
		}

		public string Name {
			get { return _name; }
		}
	}
}
