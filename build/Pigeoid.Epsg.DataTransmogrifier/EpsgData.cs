using System;
using System.Collections.Generic;
using System.IO;
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


		public void SetNumberLists(IEnumerable<double> numbers) {
			var dList = new List<double>();
			var iList = new List<double>();
			var sList = new List<double>();
			foreach(var n in numbers) {
				List<double> target;
				unchecked {
					if ((short) n == n) {
					    target = sList;
					}
					else if ((int) n == n) {
					    target = iList;
					}
					else {
						target = dList;
					}
				}
				target.Add(n);
			}
			NumberLookupDouble = dList;
			NumberLookupInt = iList;
			NumberLookupShort = sList;
		}

		public List<double> NumberLookupDouble { get; private set; }

		public List<double> NumberLookupInt { get; private set; }

		public List<double> NumberLookupShort { get; private set; }

		public ushort GetNumberIndex(double n) {
			int i;
			i = NumberLookupDouble.IndexOf(n);
			if (i >= 0)
				return (ushort)i;
			i = NumberLookupInt.IndexOf(n);
			if (i >= 0)
				return (ushort)(0xc000 | i);
			i = NumberLookupShort.IndexOf(n);
			if (i >= 0)
				return (ushort)(0x4000 | i);

			throw new InvalidDataException();
		}

		public byte[] GenerateWordIndexBytes(string text) {
			return StringUtils.GenerateWordIndexBytes(this.WordLookupList, text);
		}

		private IList<T> GetAllItems<T>() where T:class {
			return Session.CreateCriteria(typeof (T)).List<T>();
		}

	}
}
