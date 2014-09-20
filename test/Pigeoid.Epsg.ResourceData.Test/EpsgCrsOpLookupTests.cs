using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.ResourceData.Test
{

    [TestFixture]
    public class EpsgCrsOpLookupTests
    {

        private DataTransmogrifier.EpsgRepository _repository;

        [CLSCompliant(false)]
        public DataTransmogrifier.EpsgRepository Repository { get { return _repository; } }

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            var asmDirectory = new FileInfo(new Uri(typeof(EpsgDataTestBase<,>).Assembly.CodeBase).LocalPath).Directory;
            var dataFolderDirectory = new DirectoryInfo("../../build/data");
            var file = dataFolderDirectory.GetFiles("EPSG_v*.mdb").OrderByDescending(x => x.Name).First();
            _repository = new DataTransmogrifier.EpsgRepository(file);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {
            if (null != _repository)
                _repository.Dispose();
        }

        [Test]
        public void verify_from_mappings(){

            var crsOpDbMap = Repository.CoordinateOperations
                .Where(x => x.TargetCrs != null && x.SourceCrs != null)
                .ToLookup(x => checked((ushort)x.SourceCrs.Code), x => checked((ushort)x.Code))
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y).ToArray());

            foreach (var crs in Repository.Crs.Where(x => x.Code <= UInt16.MaxValue)) {
                ushort[] relatedDbOpCodes;
                crsOpDbMap.TryGetValue((ushort)crs.Code, out relatedDbOpCodes);
                var relatedAsmOpCodes = EpsgMicroDatabase.Default.GetOpsFromCrs((ushort)crs.Code);
                Assert.AreEqual(relatedDbOpCodes, relatedAsmOpCodes);
            }
        }
        
        [Test]
        public void verify_to_mappings(){
            var crsOpDbMap = Repository.CoordinateOperations
                .Where(x => x.TargetCrs != null && x.SourceCrs != null)
                .ToLookup(x => checked((ushort)x.TargetCrs.Code), x => checked((ushort)x.Code))
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y).ToArray());

            foreach (var crs in Repository.Crs.Where(x => x.Code <= UInt16.MaxValue)) {
                ushort[] relatedDbOpCodes;
                crsOpDbMap.TryGetValue((ushort)crs.Code, out relatedDbOpCodes);
                var relatedAsmOpCodes = EpsgMicroDatabase.Default.GetOpsToCrs((ushort)crs.Code);
                Assert.AreEqual(relatedDbOpCodes, relatedAsmOpCodes);
            }
        }

        [Test]
        public void verify_crs_from_base_lookup() {
            var crsReverseMapping = Repository.Crs
                .Where(x => x.SourceGeographicCrs != null)
                .GroupBy(x => checked((ushort)x.SourceGeographicCrs.Code), x => x.Code);

            foreach (var set in crsReverseMapping) {
                var dbCodes = set.OrderBy(x => x).ToArray();
                var asmCodes = EpsgMicroDatabase.Default.GetCrsFromBase(set.Key);
                Assert.AreEqual(dbCodes, asmCodes);
            }

        }

    }
}
