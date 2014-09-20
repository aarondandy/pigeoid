using Pigeoid.Epsg.Resources;
using Pigeoid.Epsg.Utility;
using Pigeoid.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
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
        private readonly EpsgDataResourceReaderGeodeticDatum _readerGeodeticDatums;
        private readonly EpsgDataResourceReaderBasicDatum<EpsgDatumEngineering> _readerEngineeringDatums;
        private readonly EpsgDataResourceReaderBasicDatum<EpsgDatumVertical> _readerVerticalDatums;
        private readonly EpsgDataResourceReaderEllipsoid _readerEllipsoids = new EpsgDataResourceReaderEllipsoid();
        private readonly EpsgDataResourceReaderParameterInfo _readerParamInfos = new EpsgDataResourceReaderParameterInfo();
        private readonly EpsgDataResourceReaderPrimeMeridian _readerPrimeMeridians = new EpsgDataResourceReaderPrimeMeridian();

        private readonly EpsgDataResourceReaderCoordinateConversionInfo _readerOpConversion;
        private readonly EpsgDataResourceReaderCoordinateTransformInfo _readerOpTransform;
        private readonly EpsgDataResourceReaderConcatenatedCoordinateOperationInfo _readerOpConcatenated;
        private readonly EpsgDataResourceAllUnitsReader _readerUnits;
        private readonly ReadOnlyUnitConversionMap _unitConversionMap;
        private readonly EpsgDataResourceReaderCrsNormal _readerCrsNormal;
        private readonly EpsgDataResourceReaderCrsCompound _readerCrsCompound;
        private readonly EpsgMultiCodeMappingUInt16 _readerOpsFrom;
        private readonly EpsgMultiCodeMappingUInt16 _readerOpsTo;
        private readonly EpsgMultiCodeMappingUInt16 _readerCrsFromBase;
        private readonly EpsgMultiCodeMappingInt32 _readerCrsFromBaseWide;

        private readonly MemoryCache _cache;

        public EpsgMicroDatabase() {

            _cache = new MemoryCache("epsg-microdb", new System.Collections.Specialized.NameValueCollection {
                {"cacheMemoryLimitMegabytes","1"}
            });

            _readerUnits = new EpsgDataResourceAllUnitsReader(this); // NOTE: doing this is nasty, but needs to be done until unit conversion is extracted from units
            var unitConversions = _readerUnits.ReadAllValues().Select(x => x.BuildConversionToBase()).Where(x => x != null).ToList();
            var indianFoot = _readerUnits.GetByKey(9080);
            Contract.Assume(indianFoot != null);
            var britishFoot = _readerUnits.GetByKey(9070);
            Contract.Assume(britishFoot != null);
            var degree9102 = _readerUnits.GetByKey(9102);
            Contract.Assume(degree9102 != null);
            var degree9122 = _readerUnits.GetByKey(9122);
            Contract.Assume(degree9122 != null);
            var arcMinute = _readerUnits.GetByKey(9103);
            Contract.Assume(arcMinute != null);
            var arcSecond = _readerUnits.GetByKey(9104);
            Contract.Assume(arcSecond != null);
            var grad = _readerUnits.GetByKey(9105);
            Contract.Assume(grad != null);
            var gon = _readerUnits.GetByKey(9106);
            Contract.Assume(gon != null);
            var centesimalMinute = _readerUnits.GetByKey(9112);
            Contract.Assume(centesimalMinute != null);
            var centesimalSecond = _readerUnits.GetByKey(9113);
            Contract.Assume(centesimalSecond != null);
            var dms = _readerUnits.GetByKey(9110);
            Contract.Assume(dms != null);
            var dm = _readerUnits.GetByKey(9111);
            Contract.Assume(dm != null);
            unitConversions.AddRange(new IUnitConversion<double>[] {
                CreateScalarConversion(9014, 9002, 6), // fathom to foot
                CreateScalarConversion(9093, 9002, 5280), // mile to foot
                CreateScalarConversion(9096, 9002, 3), // yard to foot
                CreateScalarConversion(9097, 9096, 22), // chain to yard
                CreateScalarConversion(9097, 9098, 100), // chain to link

                CreateScalarConversion(9037, 9005, 3),    // clarke yard to clarke foot
                CreateScalarConversion(9038, 9037, 22),   // clarke chain to clarke yard
                CreateScalarConversion(9038, 9039, 100),  // clarke chain to clarke link

                CreateScalarConversion(9040, 9041, 3),    // British yard to foot (Sears 1922)
                CreateScalarConversion(9042, 9040, 22),   // British chain to yard (Sears 1922)
                CreateScalarConversion(9042, 9043, 100),  // British chain to link (Sears 1922)

                CreateScalarConversion(9050, 9051, 3),    // British yard to foot (Benoit 1895 A)
                CreateScalarConversion(9052, 9050, 22),   // British chain to yard (Benoit 1895 A)
                CreateScalarConversion(9052, 9053, 100),  // British chain to link (Benoit 1895 A)

                CreateScalarConversion(9060, 9061, 3),    // British yard to foot (Benoit 1895 B)
                CreateScalarConversion(9062, 9060, 22),   // British chain to yard (Benoit 1895 B)
                CreateScalarConversion(9062, 9063, 100),  // British chain to link (Benoit 1895 B)

                CreateScalarConversion(9084, 9080, 3),    // Indian yard to foot
                CreateScalarConversion(9085, 9081, 3),    // Indian yard to foot (1937)
                CreateScalarConversion(9086, 9082, 3),    // Indian yard to foot (1962)
                CreateScalarConversion(9087, 9083, 3),    // Indian yard to foot (1975)

                CreateScalarConversion(9099, 9300, 3),    // British yard to foot (Sears 1922 truncated)
                CreateScalarConversion(9301, 9099, 22),   // British chain to yard (Sears 1922 truncated)
                CreateScalarConversion(9301, 9302, 100),  // British chain to link (Sears 1922 truncated)

                new UnitRatioConversion(indianFoot, britishFoot, 49999783, 50000000), // indian foot to british foot 1865

                new UnitUnityConversion(degree9102, degree9122), 

                new UnitScalarConversion(degree9102, arcMinute, 60), // degree to arc-minute
                new UnitScalarConversion(arcMinute, arcSecond, 60), // arc-minute to arc-second

                new UnitUnityConversion(grad, gon), // grad and gon are the same
                new UnitRatioConversion(grad, degree9102, 9, 10), // grad to degree
                new UnitRatioConversion(gon, degree9102, 9, 10), // gon to degree
                new UnitScalarConversion(grad, centesimalMinute, 100), // grad to centesimal minute
                new UnitScalarConversion(gon, centesimalMinute, 100), // gon to centesimal minute

                new UnitScalarConversion(centesimalMinute, centesimalSecond, 100), // centesimal minute to centesimal second

                new SexagesimalDmsToDecimalDegreesConversion(dms, degree9102), // sexagesimal dms to dd
                new SexagesimalDmToDecimalDegreesConversion(dm, degree9102), // sexagesimal dm to dd 
            });
            _unitConversionMap = new ReadOnlyUnitConversionMap(unitConversions);

            var crsTextReader = new EpsgDataResourceReaderText("crs.txt");
            _readerCrsNormal = new EpsgDataResourceReaderCrsNormal(crsTextReader);
            _readerCrsCompound = new EpsgDataResourceReaderCrsCompound(crsTextReader);
            var datumTextReader = new EpsgDataResourceReaderText("datums.txt");
            _readerGeodeticDatums = new EpsgDataResourceReaderGeodeticDatum(datumTextReader);
            _readerEngineeringDatums = new EpsgDataResourceReaderBasicDatum<EpsgDatumEngineering>("datumegr.dat", EpsgDatumEngineering.Create, datumTextReader);
            _readerVerticalDatums = new EpsgDataResourceReaderBasicDatum<EpsgDatumVertical>("datumver.dat", EpsgDatumVertical.Create, datumTextReader);
            var opTextReader = new EpsgDataResourceReaderText("op.txt");
            _readerOpConversion = new EpsgDataResourceReaderCoordinateConversionInfo(opTextReader);
            _readerOpTransform = new EpsgDataResourceReaderCoordinateTransformInfo(opTextReader);
            _readerOpConcatenated = new EpsgDataResourceReaderConcatenatedCoordinateOperationInfo(opTextReader);
            _readerOpsFrom = new EpsgMultiCodeMappingUInt16("txfrom.dat");
            _readerOpsTo = new EpsgMultiCodeMappingUInt16("txto.dat");
            _readerCrsFromBase = new EpsgMultiCodeMappingUInt16("crsfrombase.dat");
            _readerCrsFromBaseWide = new EpsgMultiCodeMappingInt32("crsfrombase_wide.dat");
        }


        private TValue GetOrSetCache<TKey, TValue>(string prefix, TKey code, Func<TKey, TValue> getter)
            where TValue : class
            where TKey : struct {
            Contract.Requires(getter != null);
            var key = "epsg" + prefix + code.ToString();
            object rawResult = _cache.Get(key);
            TValue result = rawResult as TValue;
            if (result == null) {
                const byte noItemValue = (byte)0;
                if (!noItemValue.Equals(rawResult)) {
                    result = getter(code);
                    var cachePolicy = new CacheItemPolicy { Priority = CacheItemPriority.Default };
                    if (result == null) {
                        _cache.Set(key, noItemValue, cachePolicy);
                    }
                    else {
                        _cache.Set(key, result, cachePolicy);
                    }
                }
            }
            return result;
        }

        private UnitScalarConversion CreateScalarConversion(ushort fromKey, ushort toKey, double factor) {
            Contract.Requires(!Double.IsNaN(factor));
            Contract.Requires(factor != 0);
            Contract.Ensures(Contract.Result<UnitScalarConversion>() != null);
            var from = _readerUnits.GetByKey(fromKey);
            var to = _readerUnits.GetByKey(toKey);
            Contract.Assume(from != null);
            Contract.Assume(to != null);
            return new UnitScalarConversion(from, to, factor);
        }

        public EpsgArea GetArea(int code) {
            return code >= 0 && code <= UInt16.MaxValue ? GetArea(unchecked((ushort)code)) : null;
        }

        internal EpsgArea GetArea(ushort code) {
            return GetOrSetCache("area", code, _readerAreas.GetByKey);
        }

        public IEnumerable<EpsgArea> GetAreas() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgArea>>() != null);
            return _readerAreas.ReadAllValues();
        }

        internal EpsgAxis[] GetAxisSet(ushort csCode) {
            Contract.Ensures(Contract.Result<EpsgAxis[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<EpsgAxis[]>(), x => x != null));
            var set = GetOrSetCache("axes", csCode, _readerAxes.GetSetByCsKey);
            if (set == null)
                return ArrayUtil<EpsgAxis>.Empty;
            return (EpsgAxis[])set.Axes.Clone();
        }

        public EpsgCoordinateOperationMethodInfo GetOperationMethod(int code) {
            return code >= 0 && code <= UInt16.MaxValue ? GetOperationMethod(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateOperationMethodInfo GetOperationMethod(ushort code) {
            return GetOrSetCache("method", code, _readerOpMethods.GetByKey);
        }

        public IEnumerable<EpsgCoordinateOperationMethodInfo> GetOperationMethods(){
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationMethodInfo>>() != null);
            return _readerOpMethods.ReadAllValues();
        }

        public EpsgCoordinateSystem GetCoordinateSystem(int code) {
            return code >= 0 && code <= UInt16.MaxValue ? GetCoordinateSystem(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateSystem GetCoordinateSystem(ushort code) {
            return GetOrSetCache("cs", code, _readerCoordinateSystems.GetByKey);
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
            if (code == 0)
                return null;
            return GetCrsNormal(code)
                ?? GetCrsCompound(code);
        }

        private EpsgCrs GetCrsNormal(uint code) {
            if (code == 0)
                return null;
            return GetOrSetCache("crsnorm", code, _readerCrsNormal.GetByKey);
        }

        private EpsgCrsCompound GetCrsCompound(uint code) {
            return code > 0 && code <= UInt16.MaxValue ? GetCrsCompound(unchecked((ushort)code)) : null;
        }

        private EpsgCrsCompound GetCrsCompound(ushort code) {
            if (code == 0)
                return null;
            return GetOrSetCache("crscmp", code, _readerCrsCompound.GetByKey);
        }

        public IEnumerable<EpsgCrs> GetAllNormalCrs() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
            return _readerCrsNormal.ReadAllValues();
        }

        public IEnumerable<EpsgCrs> GetAllCrs() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgCrs>>() != null);
            return GetAllNormalCrs()
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
            return GetOrSetCache("datumgeo", code, _readerGeodeticDatums.GetByKey);
        }

        internal EpsgDatumVertical GetVerticalDatum(ushort code) {
            return GetOrSetCache("datumver", code, _readerVerticalDatums.GetByKey);
        }

        internal EpsgDatumEngineering GetEngineeringDatum(ushort code) {
            return GetOrSetCache("datumegr", code, _readerEngineeringDatums.GetByKey);
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
            return GetOrSetCache("ellipsoid", code, _readerEllipsoids.GetByKey);
        }

        public IEnumerable<EpsgEllipsoid> GetEllipsoids() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgEllipsoid>>() != null);
            return _readerEllipsoids.ReadAllValues();
        }

        public EpsgParameterInfo GetParameterInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetParameterInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgParameterInfo GetParameterInfo(ushort code) {
            return GetOrSetCache("param", code, _readerParamInfos.GetByKey);
        }

        public IEnumerable<EpsgParameterInfo> GetParameterInfos() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgParameterInfo>>() != null);
            return _readerParamInfos.ReadAllValues();
        }

        public EpsgPrimeMeridian GetPrimeMeridian(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetPrimeMeridian(unchecked((ushort)code)) : null;
        }

        internal EpsgPrimeMeridian GetPrimeMeridian(ushort code) {
            return GetOrSetCache("meridian", code, _readerPrimeMeridians.GetByKey);
        }

        public IEnumerable<EpsgPrimeMeridian> GetPrimeMeridians() {
            Contract.Ensures(Contract.Result<IEnumerable<EpsgPrimeMeridian>>() != null);
            return _readerPrimeMeridians.ReadAllValues();
        }

        public EpsgCoordinateTransformInfo GetCoordinateTransformInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetCoordinateTransformInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateTransformInfo GetCoordinateTransformInfo(ushort code) {
            return GetOrSetCache("optx", code, _readerOpTransform.GetByKey);
        }

        public EpsgCoordinateOperationInfo GetCoordinateConversionInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetCoordinateConversionInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgCoordinateOperationInfo GetCoordinateConversionInfo(ushort code) {
            return GetOrSetCache("opcon", code, _readerOpConversion.GetByKey);
        }

        public EpsgConcatenatedCoordinateOperationInfo GetConcatenatedCoordinateOperationInfo(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetConcatenatedCoordinateOperationInfo(unchecked((ushort)code)) : null;
        }

        internal EpsgConcatenatedCoordinateOperationInfo GetConcatenatedCoordinateOperationInfo(ushort code) {
            return GetOrSetCache("opcat", code, _readerOpConcatenated.GetByKey);
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

        public EpsgUnit GetUnit(int code) {
            return code >= 0 && code < UInt16.MaxValue ? GetUnit(unchecked((ushort)code)) : null;
        }

        internal EpsgUnit GetUnit(ushort code) {
            return GetOrSetCache("unit", code, _readerUnits.GetByKey);
        }

        public EpsgUnit GetBaseUnit(string type) {
            ushort unitCode;
            if (StringComparer.OrdinalIgnoreCase.Equals("Angle", type))
                unitCode = 9101;
            else if (StringComparer.OrdinalIgnoreCase.Equals("Length", type))
                unitCode = 9001;
            else if (StringComparer.OrdinalIgnoreCase.Equals("Scale", type))
                unitCode = 9201;
            else if (StringComparer.OrdinalIgnoreCase.Equals("Time", type))
                unitCode = 1040;
            else
                return null;
            return GetUnit(unitCode);
        }

        [Obsolete("Exposing conversion info in this way is messy.")]
        internal ReadOnlyUnitConversionMap UnitConversionMap {
            get { return _unitConversionMap; }
        }

        public IEnumerable<EpsgUnit> GetUnits() {
            return _readerUnits.ReadAllValues();
        }

        [Obsolete("Should be a part of the path generator.")]
        [CLSCompliant(false)]
        public ushort[] GetOpsFromCrs(ushort crsCodeFrom) {
            return _readerOpsFrom.GetByKey(crsCodeFrom);
        }

        [Obsolete("Should be a part of the path generator.")]
        [CLSCompliant(false)]
        public ushort[] GetOpsToCrs(ushort crsCodeTo) {
            return _readerOpsTo.GetByKey(crsCodeTo);
        }

        [Obsolete("Should be a part of the path generator.")]
        [CLSCompliant(false)]
        public int[] GetCrsFromBase(ushort crsBaseCode) {
            var shortResult = _readerCrsFromBase.GetByKey(crsBaseCode);
            var wideResult = _readerCrsFromBaseWide.GetByKey(crsBaseCode);
            if (shortResult == null) {
                if (wideResult == null)
                    return null;
                else
                    return wideResult;
            }
            else {
                if (wideResult == null) {
                    return Array.ConvertAll(shortResult, x => (int)x);
                }
                else {
                    var result = new int[shortResult.Length + wideResult.Length];
                    for(int i = 0; i < shortResult.Length; i++){
                        result[i] = shortResult[i];
                    }
                    Array.Copy(wideResult, 0, result, shortResult.Length, wideResult.Length);
                    return result;
                }
                    
            }
        }
        
    }
}
