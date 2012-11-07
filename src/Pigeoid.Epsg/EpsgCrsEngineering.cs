
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgCrsEngineering : EpsgCrsDatumBased, ICrsLocal
	{

		private readonly EpsgDatumEngineering _datum;

		internal EpsgCrsEngineering(
			int code, string name, EpsgArea area, bool deprecated,
			EpsgCoordinateSystem cs, EpsgDatumEngineering datum
		)
			: base(code,name,area,deprecated,cs)
		{
			_datum = datum;
		}

		public override EpsgDatum Datum { get { return _datum; } }

		public EpsgDatumEngineering EngineeringDatum { get { return _datum; } }

		IDatum ICrsLocal.Datum { get { return _datum; } }

		public EpsgUnit Unit { get { return CoordinateSystem.Axes.First().Unit; } }

		IUnit ICrsLocal.Unit { get { return Unit; } }

		public IList<EpsgAxis> Axes { get { return CoordinateSystem.Axes.ToArray(); } }

		IList<IAxis> ICrsLocal.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }
	}
}
