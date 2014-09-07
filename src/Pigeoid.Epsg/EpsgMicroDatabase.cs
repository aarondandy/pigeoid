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

        private readonly EpsgDataResourceReaderArea _readerAreas = new EpsgDataResourceReaderArea();
        private readonly EpsgDataResourceAllAxisSetReaders _readerAxes = new EpsgDataResourceAllAxisSetReaders();
        private readonly EpsgDataResourceReaderOperationMethod _readerOpMethods = new EpsgDataResourceReaderOperationMethod();
        private readonly EpsgDataResourceReaderCoordinateSystem _readerCoordinateSystems = new EpsgDataResourceReaderCoordinateSystem();
        private readonly EpsgDataResourceReaderCrsNormal _readerCrsNormal = new EpsgDataResourceReaderCrsNormal();
        private readonly EpsgDataResourceReaderCrsCompound _readerCrsCompound = new EpsgDataResourceReaderCrsCompound();
        private readonly EpsgDataResourceReaderBasicDatum<EpsgDatumEngineering> _readerEngineeringDatums
            = new EpsgDataResourceReaderBasicDatum<EpsgDatumEngineering>("datumegr.dat", EpsgDatumEngineering.Create);
        private readonly EpsgDataResourceReaderBasicDatum<EpsgDatumVertical> _readerVerticalDatums
            = new EpsgDataResourceReaderBasicDatum<EpsgDatumVertical>("datumver.dat", EpsgDatumVertical.Create);
        private readonly EpsgDataResourceReaderGeodeticDatum _readerGeodeticDatums = new EpsgDataResourceReaderGeodeticDatum();
        private readonly EpsgDataResourceReaderEllipsoid _readerEllipsoids = new EpsgDataResourceReaderEllipsoid();
        private readonly EpsgDataResourceReaderParameterInfo _readerParamInfos = new EpsgDataResourceReaderParameterInfo();
        private readonly EpsgDataResourceReaderPrimeMeridian _readerPrimeMeridians = new EpsgDataResourceReaderPrimeMeridian();
        private readonly EpsgDataResourceReaderCoordinateConversionInfo _readerOpConversion = new EpsgDataResourceReaderCoordinateConversionInfo();
        private readonly EpsgDataResourceReaderCoordinateTransformInfo _readerOpTransform = new EpsgDataResourceReaderCoordinateTransformInfo();
        private readonly EpsgDataResourceReaderConcatenatedCoordinateOperationInfo _readerOpConcatenated = new EpsgDataResourceReaderConcatenatedCoordinateOperationInfo();

        public EpsgMicroDatabase() { }

        public EpsgArea GetArea(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? GetArea(unchecked((ushort)code))
                : null;
        }

        internal EpsgArea GetArea(ushort code) {
            return _readerAreas.GetByKey(code);
        }

        public IEnumerable<EpsgArea> GetAreas() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
            return _readerAreas.ReadAllValues();
        }

        internal EpsgAxis[] GetAxisSet(ushort csCode) {
            Contract.Ensures(Contract.Result<EpsgAxis[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<EpsgAxis[]>(), x => x != null));
            var set = _readerAxes.GetSetByCsKey(csCode);
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
            return _readerOpMethods.GetByKey(code);
        }

        public IEnumerable<EpsgCoordinateOperationMethodInfo> GetOperationMethods(){
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationMethodInfo>>() != null);
            return _readerOpMethods.ReadAllValues();
        }

        public EpsgCoordinateSystem GetCoordinateSystem(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? GetCoordinateSystem(unchecked((ushort)code))
                : null;
        }

        internal EpsgCoordinateSystem GetCoordinateSystem(ushort code) {
            return _readerCoordinateSystems.GetByKey(code);
        }

        public IEnumerable<EpsgCoordinateSystem> GetCoordinateSystems() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateSystem>>() != null);
            return _readerCoordinateSystems.ReadAllValues();
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
            return _readerCrsNormal.GetByKey(code);
        }

        private EpsgCrsCompound GetCrsCompound(uint code) {
            return code <= UInt16.MaxValue
                ? GetCrsCompound(unchecked((ushort)code))
                : null;
        }

        private EpsgCrsCompound GetCrsCompound(ushort code) {
            return _readerCrsCompound.GetByKey(code);
        }

        public IEnumerable<EpsgCrs> GetAllCrs() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
            return _readerCrsNormal.ReadAllValues()
                .Concat(_readerCrsCompound.ReadAllValues())
                .OrderBy(x => x.Code);
        }

        public EpsgDatum GetDatum(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetDatum(unchecked((ushort)code)) : null;
        }

        internal EpsgDatum GetDatum(ushort code) {
            return GetGeodeticDatum(code)
                ?? GetVerticalDatum(code)
                ?? (EpsgDatum)GetEngineeringDatum(code);
        }

        public EpsgDatumGeodetic GetGeodeticDatum(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetGeodeticDatum(unchecked((ushort)code)) : null;
        }

        internal EpsgDatumGeodetic GetGeodeticDatum(ushort code) {
            return _readerGeodeticDatums.GetByKey(code);
        }

        internal EpsgDatumVertical GetVerticalDatum(ushort code) {
            return _readerVerticalDatums.GetByKey(code);
        }

        internal EpsgDatumEngineering GetEngineeringDatum(ushort code) {
            return _readerEngineeringDatums.GetByKey(code);
        }

        public IEnumerable<EpsgDatum> GetDatums() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgDatum>>() != null);
            return _readerGeodeticDatums.ReadAllValues().Cast<EpsgDatum>()
                .Concat(_readerVerticalDatums.ReadAllValues())
                .Concat(_readerEngineeringDatums.ReadAllValues())
                .OrderBy(x => x.Code);
        }

        public IEnumerable<EpsgDatumGeodetic> GetGeodeticDatums() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgDatum>>() != null);
            return _readerGeodeticDatums.ReadAllValues();
        }

        public EpsgEllipsoid GetEllipsoid(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetEllipsoid(unchecked((ushort)code)) : null;
        }

        internal EpsgEllipsoid GetEllipsoid(ushort code) {
            return _readerEllipsoids.GetByKey(code);
        }

        public IEnumerable<EpsgEllipsoid> GetEllipsoids() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgEllipsoid>>() != null);
            return _readerEllipsoids.ReadAllValues();
        }

        public EpsgParameterInfo GetParameterInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetParameterInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgParameterInfo GetParameterInfo(ushort code) {
            return _readerParamInfos.GetByKey(code);
        }

        public IEnumerable<EpsgParameterInfo> GetParameterInfos() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgParameterInfo>>() != null);
            return _readerParamInfos.ReadAllValues();
        }

        public EpsgPrimeMeridian GetPrimeMeridian(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetPrimeMeridian(unchecked((ushort)code)) : null;
        }

        internal EpsgPrimeMeridian GetPrimeMeridian(ushort code) {
            return _readerPrimeMeridians.GetByKey(code);
        }

        public IEnumerable<EpsgPrimeMeridian> GetPrimeMeridians() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgPrimeMeridian>>() != null);
            return _readerPrimeMeridians.ReadAllValues();
        }

        public EpsgCoordinateTransformInfo GetCoordinateTransformInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetCoordinateTransformInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateTransformInfo GetCoordinateTransformInfo(ushort code) {
            return _readerOpTransform.GetByKey(code);
        }

        public EpsgCoordinateOperationInfo GetCoordinateConversionInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetCoordinateConversionInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateOperationInfo GetCoordinateConversionInfo(ushort code) {
            return _readerOpConversion.GetByKey(code);
        }

        public EpsgConcatenatedCoordinateOperationInfo GetConcatenatedCoordinateOperationInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetConcatenatedCoordinateOperationInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgConcatenatedCoordinateOperationInfo GetConcatenatedCoordinateOperationInfo(ushort code) {
            return _readerOpConcatenated.GetByKey(code);
        }

        public EpsgCoordinateOperationInfo GetSingleCoordinateOperationInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetSingleCoordinateOperationInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateOperationInfo GetSingleCoordinateOperationInfo(ushort code) {
            return GetCoordinateTransformInfo(code)
                ?? GetCoordinateConversionInfo(code);
        }

        public IEnumerable<EpsgCoordinateTransformInfo> GetCoordinateTransformInfos() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateTransformInfo>>() != null);
            return _readerOpTransform.ReadAllValues();
        }

        public IEnumerable<EpsgCoordinateOperationInfo> GetCoordinateConversionInfos() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationInfo>>() != null);
            return _readerOpConversion.ReadAllValues();
        }

        public IEnumerable<EpsgConcatenatedCoordinateOperationInfo> GetConcatenatedCoordinateOperationInfos() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgConcatenatedCoordinateOperationInfo>>() != null);
            return _readerOpConcatenated.ReadAllValues();
        }
        
    }
}
