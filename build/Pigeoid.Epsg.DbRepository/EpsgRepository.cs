using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Criterion;

namespace Pigeoid.Epsg.DataTransmogrifier
{

	public class EpsgRepository : IDisposable
	{

		public EpsgRepository(FileInfo databaseFilePath) {
			SessionFactory = Fluently.Configure()
				.Database(JetDriverConfiguration.Standard.ConnectionString(c =>
					c.DatabaseFile(databaseFilePath.FullName)
					.Provider("Microsoft.Jet.OLEDB.4.0")
				))
				.Mappings(m => m.FluentMappings.AddFromAssemblyOf<EpsgRepository>())
				.BuildSessionFactory();
			Session = SessionFactory.OpenSession();
		}

		private ISessionFactory SessionFactory { get; set; }

		public ISession Session { get; private set; }

		public IList<EpsgAlias> Aliases {
			get {
				return Session
					.CreateSQLQuery("SELECT * FROM Alias ORDER BY AREA_CODE")
					.AddEntity(typeof(EpsgAlias))
					.List<EpsgAlias>();
			}
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

		private readonly ConcurrentDictionary<Type, bool> _hasCodeProperty = new ConcurrentDictionary<Type, bool>();

		private IList<T> GetAllItems<T>() where T : class {
			var criteria = Session.CreateCriteria(typeof(T));

			if(_hasCodeProperty.GetOrAdd(typeof(T), x => x.GetProperties().Any(p => p.Name == "Code")))
				criteria = criteria.AddOrder(Order.Asc("Code"));

			return criteria.SetReadOnly(true).SetCacheable(true).List<T>();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(null != Session)
				Session.Dispose();
			if(null != SessionFactory)
				SessionFactory.Dispose();
		}

		~EpsgRepository() {
			Dispose(false);
		}

	}
}
