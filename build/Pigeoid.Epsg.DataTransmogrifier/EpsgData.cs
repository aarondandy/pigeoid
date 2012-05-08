using System;
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

		public IList<EpsgCoordinateOperationMethod> CoordinateOperationMethods {
			get { return GetAllItems<EpsgCoordinateOperationMethod>(); }
		}

		public IList<EpsgCrs> Crs {
			get { return GetAllItems<EpsgCrs>(); }
		}

		public IList<EpsgDatum> Datums {
			get { return GetAllItems<EpsgDatum>(); }
		}

		public IList<EpsgEllipsoid> Ellipsoids {
			get { return GetAllItems<EpsgEllipsoid>(); }
		}

		public IList<EpsgDeprecation> Deprecations {
			get { return GetAllItems<EpsgDeprecation>(); }
		}

		public IList<EpsgNamingSystem> NamingSystems {
			get { return GetAllItems<EpsgNamingSystem>(); }
		}

		public IList<EpsgPrimeMeridian> PrimeMeridians {
			get { return GetAllItems<EpsgPrimeMeridian>(); }
		}

		public IList<EpsgSupersession> Supersessions {
			get { return GetAllItems<EpsgSupersession>(); }
		}

		public IList<EpsgUom> Uoms {
			get { return GetAllItems<EpsgUom>(); }
		}

		public IList<EpsgParameter> Parameters {
			get { return GetAllItems<EpsgParameter>(); }
		}

		public IList<EpsgParamUse> ParamUses {
			get { return GetAllItems<EpsgParamUse>(); }
		}

		public IList<EpsgParamValue> ParamValues {
			get { return GetAllItems<EpsgParamValue>(); }
		}

		public IList<EpsgCoordOpPathItem> CoordOpPathItems {
			get { return GetAllItems<EpsgCoordOpPathItem>(); }
		}

		public List<string> WordLookupList { get; set; }

		public List<double> NumberLookupList { get; set; }

		public byte[] GenerateWordIndexBytes(string text) {
			return StringUtils.GenerateWordIndexBytes(this.WordLookupList, text);
		}

		private IList<T> GetAllItems<T>() where T:class {
			return Session.CreateCriteria(typeof (T)).List<T>();
		}

	}
}
