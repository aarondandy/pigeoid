using System.Collections.Generic;
using Pigeoid.Contracts;
using Pigeoid.Interop;

namespace Pigeoid.Unit
{
	public class UnitEqualityComparer : IEqualityComparer<IUnit>
	{

		public static readonly UnitEqualityComparer Default = new UnitEqualityComparer();

		private readonly INameNormalizedComparer _nameNormalizedComparer;

		public UnitEqualityComparer(INameNormalizedComparer nameNormalizedComparer = null) {
			_nameNormalizedComparer = nameNormalizedComparer ?? UnitNameNormalizedComparer.Default;
		}

		public INameNormalizedComparer NameNormalizedComparer { get { return _nameNormalizedComparer; } }

		public bool Equals(IUnit x, IUnit y) {
			if (ReferenceEquals(x, y))
				return true;
			if (null == x || null == y)
				return false;
			return _nameNormalizedComparer.Equals(x.Name, y.Name)
				&& _nameNormalizedComparer.Equals(x.Type, y.Type);
		}

		public int GetHashCode(IUnit obj) {
			if (null == obj)
				return 0;
			return _nameNormalizedComparer.GetHashCode(obj.Name)
				^ -_nameNormalizedComparer.GetHashCode(obj.Type);
		}
	}
}
