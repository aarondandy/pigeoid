using Pigeoid.Epsg.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public class EpsgMiniDatabase
    {


        private readonly EpsgArea.EpsgAreaLookUp _areaLookup;

        public EpsgMiniDatabase() {
            _areaLookup = new EpsgArea.EpsgAreaLookUp();
        }

        public IEnumerable<EpsgArea> AllAreas {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
                return _areaLookup.Values;
            }
        }

        public EpsgArea GetArea(int epsgCode) {
            return (epsgCode < 0 || epsgCode >= UInt16.MaxValue)
                ? null
                : _areaLookup.Get(unchecked((ushort)epsgCode));
        }



    }
}
