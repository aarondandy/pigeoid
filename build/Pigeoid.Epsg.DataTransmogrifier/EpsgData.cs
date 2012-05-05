using System.Collections.Generic;
using NHibernate;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgData
	{

		public EpsgData(ISession session) {
			Session = session;
		}

		public ISession Session { get; set; }

		public IList<EpsgAlias> Aliases {
			get { return Session.CreateSQLQuery("SELECT * FROM Alias").AddEntity(typeof(EpsgAlias)).List<EpsgAlias>(); }
			//get { return GetAllItems<EpsgAlias>(); } // fails because of a space in the column name. Jet driver bug?
		}

		public IList<EpsgArea> Areas {
			get { return GetAllItems<EpsgArea>(); }
		}

		public IList<EpsgAxis> Axes {
			get { return GetAllItems<EpsgAxis>(); }
		}

		public IList<EpsgAxisName> AxisNames {
			get { return GetAllItems<EpsgAxisName>(); }
		}

		public IList<EpsgCoordinateSystem> CoordinateSystems {
			get { return GetAllItems<EpsgCoordinateSystem>(); }
		}

		public IList<EpsgCoordinateOperation> CoordinateOperations {
			get { return GetAllItems<EpsgCoordinateOperation>(); }
		}

		public IList<EpsgCrs> Crs {
			get { return GetAllItems<EpsgCrs>(); }
		}

		private IList<T> GetAllItems<T>() where T:class {
			return Session.CreateCriteria(typeof (T)).List<T>();
		}

		/*public List<string> AllWords { get; set; }

		public List<double> AllNumbers { get; set; }

		public List<dynamic> Ellipsoids { get; set; }

		public List<dynamic> Areas { get; set; }*/

	}
}
