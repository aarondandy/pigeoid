using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Pigeoid.Epsg.DataTransmogrifier
{

	public class EpsgRepository : IDisposable
	{

		public EpsgRepository(FileInfo databaseFilePath) {
            SessionFactory = Fluently.Configure()
                .Database(
                    JetDriverConfiguration.Standard.ConnectionString(c =>
                        c.DatabaseFile(databaseFilePath.FullName)
                        .Provider("Microsoft.Jet.OLEDB.4.0")
                    )
                //.ShowSql()
                )
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<EpsgRepository>())
                .ExposeConfiguration(c => {
                    c.SetProperty("cache.provider_class", "NHibernate.Cache.HashtableCacheProvider");
                    c.SetProperty("cache.use_second_level_cache", "true");
                    c.SetProperty("cache.use_query_cache", "true");
                })
                .BuildSessionFactory();
			Session = SessionFactory.OpenSession();
		}

		private ISessionFactory SessionFactory { get; set; }

		public ISession Session { get; private set; }

        public IEnumerable<EpsgArea> Areas {
			get { return GetAllItems<EpsgArea>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> AreaNames {
            get { return GetAllItems<EpsgArea>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgAxis> Axes {
            get { return GetAllItems<EpsgAxis>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgAxisName> AxisNames {
            get { return GetAllItems<EpsgAxisName>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgCoordinateSystem> CoordinateSystems {
            get { return GetAllItems<EpsgCoordinateSystem>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> CoordinateSystemNames {
            get { return GetAllItems<EpsgCoordinateSystem>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgCoordinateOperation> CoordinateOperations {
            get { return GetAllItems<EpsgCoordinateOperation>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgCoordinateOperation> CrsBoundCoordinateOperations {
            get { return GetAllItems<EpsgCoordinateOperation>().Where(x => x.SourceCrs != null && x.TargetCrs != null).OrderBy(x => x.Code); }
        }

        public IEnumerable<EpsgCoordinateOperation> CoordinateConversions {
            get { return GetAllItems<EpsgCoordinateOperation>().Where(x => x.TypeName == "conversion").OrderBy(x => x.Code); }
        }

        public IEnumerable<EpsgCoordinateOperation> CoordinateTransforms {
            get { return GetAllItems<EpsgCoordinateOperation>().Where(x => x.TypeName == "transformation").OrderBy(x => x.Code); }
        }

        public IEnumerable<EpsgCoordinateOperation> CoordinateOperationsConcatenated {
            get { return GetAllItems<EpsgCoordinateOperation>().Where(x => x.TypeName == "concatenated Operation").OrderBy(x => x.Code); }
        }

        public IEnumerable<string> CoordinateOperationNames {
            get { return GetAllItems<EpsgCoordinateOperation>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgCoordinateOperationMethod> CoordinateOperationMethods {
            get { return GetAllItems<EpsgCoordinateOperationMethod>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> CoordinateOperationMethodNames {
            get { return GetAllItems<EpsgCoordinateOperationMethod>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgCrs> Crs {
            get { return GetAllItems<EpsgCrs>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgCrs> CrsProjected {
            get { return GetAllItems<EpsgCrs>().Where(x => x.Kind == "projected").OrderBy(x => x.Code); }
        }

        public IEnumerable<EpsgCrs> CrsCompound {
            get { return GetAllItems<EpsgCrs>().Where(x => x.Kind == "compound").OrderBy(x => x.Code); }
        }

        public IEnumerable<EpsgCrs> CrsNotCompound {
            get { return GetAllItems<EpsgCrs>().Where(x => x.Kind != "compound").OrderBy(x => x.Code); }
        }

        public IEnumerable<string> CrsNames {
            get { return GetAllItems<EpsgCrs>().Select(x => x.Name); }
        }

        public EpsgCrs GetCrsByProjectionCode(int projectionCode) {
            return GetAllItems<EpsgCrs>()
                .Single(x => x.Projection != null && x.Projection.Code == projectionCode);
        }

        public IEnumerable<EpsgDatum> Datums {
            get { return GetAllItems<EpsgDatum>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgDatum> DatumsGeodetic {
            get { return GetAllItems<EpsgDatum>().Where(x => x.Type == "geodetic").OrderBy(x => x.Code); }
        }

        public IEnumerable<string> DatumNames {
            get { return GetAllItems<EpsgDatum>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgEllipsoid> Ellipsoids {
            get { return GetAllItems<EpsgEllipsoid>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> EllipsoidNames {
            get { return GetAllItems<EpsgEllipsoid>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgDeprecation> Deprecations {
            get { return GetAllItems<EpsgDeprecation>().OrderBy(x => x.Id); }
		}

        public IEnumerable<EpsgNamingSystem> NamingSystems {
            get { return GetAllItems<EpsgNamingSystem>().OrderBy(x => x.Code); }
		}

        public IEnumerable<EpsgPrimeMeridian> PrimeMeridians {
            get { return GetAllItems<EpsgPrimeMeridian>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> PrimeMeridianNames {
            get { return GetAllItems<EpsgPrimeMeridian>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgSupersession> Supersessions {
            get { return GetAllItems<EpsgSupersession>().OrderBy(x => x.Id); }
		}

        public IEnumerable<EpsgUom> Uoms {
            get { return GetAllItems<EpsgUom>().OrderBy(x => x.Code); }
		}

        public IEnumerable<string> UomNames {
            get { return GetAllItems<EpsgUom>().Select(x => x.Name); }
        }

        public IEnumerable<EpsgParameter> Parameters {
            get { return GetAllItems<EpsgParameter>().OrderBy(x => x.Code); }
		}


        public IEnumerable<EpsgParamUse> ParamUses {
            get { return GetAllItems<EpsgParamUse>().OrderBy(x => x.Method.Code).ThenBy(x => x.SortOrder); }
		}

        public IEnumerable<EpsgParamValue> ParamValues {
            get { return GetAllItems<EpsgParamValue>().OrderBy(x => x.Operation.Code).ThenBy(x => x.Parameter.Code); }
		}

        public IEnumerable<string> ParamTextValues {
            get {
                return GetAllItems<EpsgParamValue>().Select(x => x.TextValue).Where(x => x != null);
            }
        }

        public IEnumerable<EpsgCoordOpPathItem> CoordOpPathItems {
            get { return GetAllItems<EpsgCoordOpPathItem>().OrderBy(x => x.CatCode).ThenBy(x => x.Step); }
		}

		private readonly ConcurrentDictionary<Type, bool> _hasCodeProperty = new ConcurrentDictionary<Type, bool>();

		private IQueryable<T> GetAllItems<T>() where T : class {
            return Session
                .Query<T>();
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
