using Pigeoid.Epsg.Resources;
using Pigeoid.Epsg.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public sealed class EpsgMicroDatabase
    {

        [Obsolete]
        private static readonly EpsgMicroDatabase _default = new EpsgMicroDatabase();

        [Obsolete]
        public static EpsgMicroDatabase Default { get { return _default; } }

        private readonly EpsgDataResourceReaderArea ReaderAreas = new EpsgDataResourceReaderArea();
        private readonly EpsgDataResourceAllAxisSetReaders ReaderAxes = new EpsgDataResourceAllAxisSetReaders();
        private readonly EpsgDataResourceReaderOperationMethod ReaderOpMethods = new EpsgDataResourceReaderOperationMethod();
        private readonly EpsgDataResourceReaderCoordinateSystem ReaderCoordinateSystems = new EpsgDataResourceReaderCoordinateSystem();
        private readonly EpsgDataResourceReaderCrsNormal ReaderCrsNormal = new EpsgDataResourceReaderCrsNormal();
        private readonly EpsgDataResourceReaderCrsCompound ReaderCrsCompound = new EpsgDataResourceReaderCrsCompound();

        public EpsgMicroDatabase() {
        }

        public EpsgArea GetArea(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? GetArea(unchecked((ushort)code))
                : null;
        }

        internal EpsgArea GetArea(ushort code) {
            return ReaderAreas.GetByKey(code);
        }

        public IEnumerable<EpsgArea> GetAreas() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
            return ReaderAreas.ReadAllValues();
        }

        internal EpsgAxis[] GetAxisSet(ushort csCode) {
            Contract.Ensures(Contract.Result<EpsgAxis[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<EpsgAxis[]>(), x => x != null));
            var set = ReaderAxes.GetSetByCsKey(csCode);
            if (set == null)
                return ArrayUtil<EpsgAxis>.Empty;
            return (EpsgAxis[])set.Axes.Clone();
        }

        public EpsgCoordinateOperationMethodInfo GetOperationMethod(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? GetOperationMethod(unchecked((ushort)code))
                : null;
        }

        internal EpsgCoordinateOperationMethodInfo GetOperationMethod(ushort code) {
            return ReaderOpMethods.GetByKey(code);
        }

        public IEnumerable<EpsgCoordinateOperationMethodInfo> GetOperationMethods(){
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationMethodInfo>>() != null);
            return ReaderOpMethods.ReadAllValues();
        }

        public EpsgCoordinateSystem GetCoordinateSystem(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? GetCoordinateSystem(unchecked((ushort)code))
                : null;
        }

        internal EpsgCoordinateSystem GetCoordinateSystem(ushort code) {
            return ReaderCoordinateSystems.GetByKey(code);
        }

        public IEnumerable<EpsgCoordinateSystem> GetCoordinateSystems() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateSystem>>() != null);
            return ReaderCoordinateSystems.ReadAllValues();
        }

        public EpsgCrs GetCrs(int code) {
            if (code <= 0)
                return null; // NOTE: assume 0 is an invalid code
                
            var uintCode = unchecked((uint)code);
            return GetCrsNormal(uintCode)
                ?? GetCrsCompound(uintCode);
        }

        internal EpsgCrs GetCrs(ushort code){
            return GetCrsNormal(code)
                ?? GetCrsCompound(code);
        }

        private EpsgCrs GetCrsNormal(uint code) {
            return ReaderCrsNormal.GetByKey(code);
        }

        private EpsgCrsCompound GetCrsCompound(uint code) {
            return code <= UInt16.MaxValue
                ? GetCrsCompound(unchecked((ushort)code))
                : null;
        }

        private EpsgCrsCompound GetCrsCompound(ushort code) {
            return ReaderCrsCompound.GetByKey(code);
        }

        public IEnumerable<EpsgCrs> GetAllCrs() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
            return ReaderCrsNormal.ReadAllValues()
                .Concat(ReaderCrsCompound.ReadAllValues())
                .OrderBy(x => x.Code);
        }

    }
}
