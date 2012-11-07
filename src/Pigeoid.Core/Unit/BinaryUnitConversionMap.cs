using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{

	public class BinaryUnitConversionMap : SimpleUnitConversionMapBase
	{

		private readonly IUnitConversion<double> _forwardOperation;

		public BinaryUnitConversionMap([NotNull] IUnitConversion<double> forwardOperation, IEqualityComparer<IUnit> unitEqualityComparer = null)
			: base(unitEqualityComparer)
		{
			if (null == forwardOperation)
				throw new ArgumentNullException("forwardOperation");
			_forwardOperation = forwardOperation;
		}

		private IUnit FromDefined { get { return _forwardOperation.From; } }

		private IUnit ToDefined { get { return _forwardOperation.To; } }

		public override IEnumerable<IUnit> AllUnits {
			get { return new[] { FromDefined, ToDefined }.Distinct(EqualityComparer); }
		}

		public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
			if (AreUnitsMatching(ToDefined, to))
				return new[] { _forwardOperation };
			if (_forwardOperation.HasInverse && AreUnitsMatching(FromDefined, to))
				return new[] { _forwardOperation.GetInverse() };
			return Enumerable.Empty<IUnitConversion<double>>();
		}

		public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
			if (AreUnitsMatching(FromDefined, from))
				return new[] { _forwardOperation };
			if (_forwardOperation.HasInverse && AreUnitsMatching(ToDefined, from))
				return new[] { _forwardOperation.GetInverse() };
			return Enumerable.Empty<IUnitConversion<double>>();
		}

	}

}
