using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur;

namespace Pigeoid.Ogc
{
    public class WktReader : IEnumerator<char>
    {
        private static readonly object[] _emptyObjectArray = new object[0];

        private TextReader _reader;

        public WktReader(TextReader reader, WktOptions options = null) {
            if (null == reader) throw new ArgumentNullException("reader");
            Contract.EndContractBlock();
            _reader = reader;
            Options = options ?? new WktOptions();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_reader != null);
            Contract.Invariant(Options != null);
        }

        public WktOptions Options { get; private set; }

        public char Current { get; private set; }

        public char CurrentUpperInvariant { get { return Char.ToUpperInvariant(Current); } }

        public void Dispose() {
            _reader = null;
        }

        object IEnumerator.Current { get { return Current; } }

        public string FixName(string name) {
            Contract.Ensures(Contract.Result<string>() != null);
            if (String.IsNullOrEmpty(name))
                return String.Empty;
            if (Options.CorrectNames)
                name = name.Replace('_', ' ');
            return name;
        }

        public bool MoveNext() {
            var iChar = _reader.Read();
            if (iChar >= 0) {
                Current = (char)iChar;
                return true;
            }
            Current = '\0';
            return false;
        }

        public void Reset() {
            throw new NotSupportedException();
            Contract.EndContractBlock();
        }

        public bool SkipWhiteSpace() {
            var ok = true;
            while (ok && IsWhiteSpace) {
                ok = MoveNext();
            }
            return ok;
        }

        public bool IsWhiteSpace {
            get { return Options.IsWhiteSpace(Current); }
        }

        public bool IsDigit {
            get { return Options.IsDigit(Current); }
        }

        public bool IsDoubleQuote { get { return Current == '\"'; } }

        public bool IsLetter {
            get { return Options.IsLetter(Current); }
        }

        public bool IsComma {
            get { return Options.IsComma(Current); }
        }

        public bool IsOpenBracket {
            get { return '[' == Current || '(' == Current; }
        }

        public bool IsCloseBracket {
            get { return ']' == Current || ')' == Current; }
        }

        public bool IsValidForDoubleValue {
            get { return Options.IsValidForDoubleValue(Current); }
        }

        public bool IsKeyword { get { return IsLetter || '_' == Current; } }

        public double? ReadDouble() {
            var builder = new StringBuilder();
            while (IsValidForDoubleValue) {
                builder.Append(Current);
                if (!MoveNext()) {
                    break;
                }
            }
            double value;
            return Double.TryParse(builder.ToString(), out value)
                ? (double?)value
                : null;
        }

        public string ReadQuotedString() {
            var readOk = true;
            var builder = new StringBuilder();

            if (IsDoubleQuote) {
                if (!(readOk = MoveNext())) {
                    return null;
                }
            }

            while (!IsDoubleQuote) {
                builder.Append(Current);
                if (!(readOk = MoveNext())) {
                    break;
                }
            }

            if (readOk)
                MoveNext();

            return builder.ToString();
        }

        public bool ReadOpenBracket() {
            return SkipWhiteSpace()
                && IsOpenBracket
                && MoveNext();
        }

        public bool ReadCloseBracket() {
            if (!SkipWhiteSpace())
                return false;

            var ok = IsCloseBracket;
            MoveNext();
            return ok;
        }

        public object[] ReadParams() {
            Contract.Ensures(Contract.Result<object[]>() != null);
            var paramResults = new List<object>();
            while (true) {
                if (!SkipWhiteSpace())
                    return _emptyObjectArray;

                var paramEntity = ReadEntity();
                if (null == paramEntity)
                    return _emptyObjectArray;

                paramResults.Add(paramEntity);
                if (!SkipWhiteSpace())
                    return _emptyObjectArray;

                if (IsComma) {
                    if (!MoveNext())
                        return _emptyObjectArray;
                }
                else {
                    if (IsCloseBracket && ReadCloseBracket())
                        break;

                    return _emptyObjectArray;
                }
            }
            return paramResults.ToArray();
        }

        public IAuthorityTag ReadAuthorityFromParams() {
            var names = Array.ConvertAll(ReadParams(), x => null == x ? String.Empty : x.ToString());
            if (names.Length == 0)
                return null;

            var authorityName = names[0] ?? String.Empty;
            var authorityCode = (names.Length > 1 ? names[1] : null) ?? String.Empty;

            IAuthorityTag tag = null;
            if (Options.ResolveAuthorities)
                tag = Options.GetAuthorityTag(authorityName, authorityCode);
            return tag ?? new AuthorityTag(authorityName, authorityCode);
        }

        private bool IsDirectionKeyword(WktKeyword keyword) {
            switch (keyword) {
                case WktKeyword.North:
                case WktKeyword.South:
                case WktKeyword.East:
                case WktKeyword.West:
                case WktKeyword.Up:
                case WktKeyword.Down:
                case WktKeyword.Other:
                    return true;
                default: return false;
            }
        }

        private OgcOrientationType ToOgcOrientationType(WktKeyword keyword) {
            switch (keyword) {
                case WktKeyword.North: return OgcOrientationType.North;
                case WktKeyword.South: return OgcOrientationType.South;
                case WktKeyword.East: return OgcOrientationType.East;
                case WktKeyword.West: return OgcOrientationType.West;
                case WktKeyword.Up: return OgcOrientationType.Up;
                case WktKeyword.Down: return OgcOrientationType.Down;
                default: return OgcOrientationType.Other;
            }
        }

        public IAxis ReadAxisFromParams() {
            Contract.Ensures(Contract.Result<IAxis>() != null);
            var directionKeyword = WktKeyword.Invalid;
            string name = null;
            foreach (var parameter in ReadParams()) {
                if (null == name && parameter is string)
                    name = parameter as string;
                else if (WktKeyword.Invalid == directionKeyword && parameter is WktKeyword)
                    directionKeyword = (WktKeyword)parameter;
            }

            return new OgcAxis(
                name ?? String.Empty,
                ToOgcOrientationType(IsDirectionKeyword(directionKeyword) ? directionKeyword : WktKeyword.Other)
            );
        }

        public INamedParameter ReadParameterFromParams() {
            var allParams = ReadParams();
            if (allParams.Length == 0)
                return null;

            var name = allParams[0] == null
                ? String.Empty
                : FixName(allParams[0].ToString());

            var value = allParams.Length > 1 ? allParams[1] : null;
            if (value is double || value is int)
                return new NamedParameter<double>(name, (double)value);
            if (value is string)
                return new NamedParameter<string>(name, (string)value);
            return new NamedParameter<object>(name, value);
        }

        public ICoordinateOperationInfo ReadParamMtFromParams() {
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            var name = String.Empty;
            var transformParams = new List<INamedParameter>();
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is INamedParameter)
                    transformParams.Add((INamedParameter)parameter);
            }

            return new CoordinateOperationInfo(
                name,
                transformParams,
                new OgcCoordinateOperationMethodInfo(name)
            );
        }

        public IConcatenatedCoordinateOperationInfo ReadConcatMtFromParams() {
            Contract.Ensures(Contract.Result<IConcatenatedCoordinateOperationInfo>() != null);
            return new ConcatenatedCoordinateOperationInfo(ReadParams().OfType<ICoordinateOperationInfo>());
        }

        public ICrsCompound ReadCompoundCsFromParams() {
            IAuthorityTag authority = null;
            ICrs headCrs = null;
            ICrs tailCrs = null;
            string name = String.Empty;
            foreach (var parameter in ReadParams()) {
                if (parameter is IAuthorityTag) {
                    if (null == authority)
                        authority = (IAuthorityTag)parameter;
                }
                else if (parameter is ICrs) {
                    if (null == headCrs)
                        headCrs = (ICrs)parameter;
                    else if (null == tailCrs)
                        tailCrs = (ICrs)parameter;
                }
                else if (parameter is string) {
                    name = (string)parameter;
                }
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsCompound;
                if (null != crs)
                    return crs;
            }

            if (headCrs == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Compoint CRS","No head CRS.");
                return null;
            }
            if (tailCrs == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Compoint CRS", "No tail CRS.");
                return null;
            }

            return new OgcCrsCompound(
                name,
                headCrs,
                tailCrs,
                authority
            );
        }

        public ICrsProjected ReadProjectedCsFromParams() {
            IAuthorityTag authority = null;
            string name = null;
            ICrsGeodetic baseCrs = null;
            ICoordinateOperationMethodInfo operationMethodInfo = null;
            var operationParameters = new List<INamedParameter>();
            IUnit linearUnit = null;
            var axes = new List<IAxis>();
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is ICrsGeodetic)
                    baseCrs = (ICrsGeodetic)parameter;
                else if (parameter is ICoordinateOperationMethodInfo)
                    operationMethodInfo = (ICoordinateOperationMethodInfo)parameter;
                else if (parameter is INamedParameter)
                    operationParameters.Add((INamedParameter)parameter);
                else if (parameter is IUnit)
                    linearUnit = (IUnit)parameter;
                else if (parameter is IAxis)
                    axes.Add((IAxis)parameter);
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsProjected;
                if (null != crs)
                    return crs;
            }

            if (null == baseCrs) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Project CRS", "No base CRS.");
                return null;
            }

            return new OgcCrsProjected(
                name ?? String.Empty,
                baseCrs,
                new CoordinateOperationInfo(
                    null == operationMethodInfo ? String.Empty : FixName(operationMethodInfo.Name),
                    operationParameters,
                    operationMethodInfo
                ),
                linearUnit ?? OgcLinearUnit.DefaultMeter,
                axes,
                authority
            );
        }

        private ICrsGeocentric ReadGeocentricCsFromParams() {
            Contract.Ensures(Contract.Result<ICrsGeocentric>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            IDatumGeodetic datum = null;
            IPrimeMeridianInfo primeMeridian = null;
            IUnit unit = OgcLinearUnit.DefaultMeter;
            var axes = new List<IAxis>();

            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is IDatumGeodetic)
                    datum = (IDatumGeodetic)parameter;
                else if (parameter is IPrimeMeridianInfo)
                    primeMeridian = (IPrimeMeridianInfo)parameter;
                else if (parameter is IUnit)
                    unit = (IUnit)parameter;
                else if (parameter is IAxis)
                    axes.Add((IAxis)parameter);
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsGeocentric;
                if (null != crs)
                    return crs;
            }

            if (null != datum && datum.PrimeMeridian == null && null != primeMeridian) {
                // in this case the datum must have NOT been created from an authority source so we should remake it with a prime meridian
                datum = new OgcDatumHorizontal(
                    datum.Name,
                    datum.Spheroid,
                    primeMeridian,
                    datum.BasicWgs84Transformation,
                    datum.Authority
                );
            }

            if (datum == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Geocentric CRS","No datum.");
                datum = OgcDatumHorizontal.DefaultWgs84;
            }

            return new OgcCrsGeocentric(
                name,
                datum,
                unit,
                axes,
                authority
            );
        }

        public ICrsVertical ReadVerticalCsFromParams() {
            IAuthorityTag authority = null;
            var name = String.Empty;
            IDatum datum = null;
            IUnit unit = null;
            IAxis axis = null;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is IDatum)
                    datum = (IDatum)parameter;
                else if (parameter is IUnit)
                    unit = (IUnit)parameter;
                else if (parameter is IAxis)
                    axis = (IAxis)parameter;
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsVertical;
                if (null != crs)
                    return crs;
            }

            if (null == datum) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Vertical CRS","No datum.");
                datum = OgcDatumHorizontal.DefaultWgs84;
            }
            if (null == unit) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Vertical CRS","No unit.");
                unit = OgcLinearUnit.DefaultMeter;
            }
            if (null == axis) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Vertical CRS","No axis.");
                return null;
            }

            return new OgcCrsVertical(
                name,
                datum,
                unit,
                axis,
                authority
            );
        }

        private ICrsFitted ReadFittedCsFromParams() {
            var name = String.Empty;
            ICoordinateOperationInfo toBaseOperation = null;
            ICrs baseCrs = null;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is ICoordinateOperationInfo)
                    toBaseOperation = (ICoordinateOperationInfo)parameter;
                else if (parameter is ICrs)
                    baseCrs = (ICrs)parameter;
            }

            if (baseCrs == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Fitted CS","No base CRS.");
                return null;
            }
            if (toBaseOperation == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Fitted CS","No to-base operation.");
                return null;
            }

            return new OgcCrsFitted(
                name,
                toBaseOperation,
                baseCrs
            );
        }

        private ICrsLocal ReadLocalCsFromParams() {
            Contract.Ensures(Contract.Result<ICrsLocal>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            IDatum datum = null;
            IUnit unit = null;
            var axes = new List<IAxis>();
            foreach (var parameter in ReadParams()) {
                if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IDatum)
                    datum = (IDatum)parameter;
                else if (parameter is IUnit)
                    unit = (IUnit)parameter;
                else if (parameter is IAxis)
                    axes.Add((IAxis)parameter);
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsLocal;
                if (null != crs)
                    return crs;
            }

            if (datum == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Local CRS","No datum.");
                datum = OgcDatumHorizontal.DefaultWgs84;
            }
            if (unit == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Local CRS","No unit.");
                unit = OgcLinearUnit.DefaultMeter;
            }

            return new OgcCrsLocal(
                name,
                datum,
                unit,
                axes,
                authority
            );
        }

        private ICrsGeographic ReadGeographicCsFromParams() {
            Contract.Ensures(Contract.Result<ICrsGeographic>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            IDatumGeodetic datum = null;
            IPrimeMeridianInfo primeMeridian = null;
            IUnit unit = OgcAngularUnit.DefaultDegrees;
            var axes = new List<IAxis>();
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is IDatumGeodetic)
                    datum = (IDatumGeodetic)parameter;
                else if (parameter is IPrimeMeridianInfo)
                    primeMeridian = (IPrimeMeridianInfo)parameter;
                else if (parameter is IUnit)
                    unit = (IUnit)parameter;
                else if (parameter is IAxis)
                    axes.Add((IAxis)parameter);
            }

            if (null != authority) {
                var crs = Options.GetCrs(authority) as ICrsGeographic;
                if (null != crs)
                    return crs;
            }

            if (null != datum && datum.PrimeMeridian == null && null != primeMeridian) {
                // in this case the datum must have NOT been created from an authority source so we should remake it with a prime meridian
                datum = new OgcDatumHorizontal(
                    datum.Name,
                    datum.Spheroid,
                    primeMeridian,
                    datum.BasicWgs84Transformation,
                    datum.Authority
                );
            }

            if (datum == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Geographic CRS","No datum.");
                datum = OgcDatumHorizontal.DefaultWgs84;
            }

            return new OgcCrsGeographic(
                name,
                datum,
                unit,
                axes,
                authority
            );
        }

        public ICoordinateOperationInfo ReadInverseFromParams() {
            var nestedOperation = ReadParams()
                .OfType<ICoordinateOperationInfo>()
                .FirstOrDefault();
            return null != nestedOperation ? nestedOperation.GetInverse() : null;
        }

        public ICoordinateOperationInfo ReadPassThroughFromParams() {
            int index = 0;
            ICoordinateOperationInfo coordinateOperationInfo = null;
            foreach (var parameter in ReadParams()) {
                if (parameter is int)
                    index = (int)parameter;
                else if (parameter is double)
                    index = Convert.ToInt32(parameter);
                else if (null == coordinateOperationInfo && parameter is ICoordinateOperationInfo)
                    coordinateOperationInfo = (ICoordinateOperationInfo)parameter;
            }

            if (coordinateOperationInfo == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Passthrough Transform", "No coordinate operation.");
                return null;
            }

            return new OgcPassThroughCoordinateOperationInfo(
                coordinateOperationInfo,
                index
            );
        }

        public ISpheroidInfo ReadSpheroidFromParams() {
            Contract.Ensures(Contract.Result<ISpheroidInfo>() != null);
            var name = String.Empty;
            double? majorAxis = null;
            double? inverseF = null;
            IAuthorityTag authority = null;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is double) {
                    var value = (double)parameter;
                    if (!majorAxis.HasValue) {
                        majorAxis = value;
                    }
                    else if (!inverseF.HasValue) {
                        inverseF = value;
                    }
                }
            }

            if (null != authority) {
                var spheroid = Options.GetSpheroid(authority);
                if (null != spheroid)
                    return spheroid;
            }

            if (!majorAxis.HasValue) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Spheroid", "No major axis.");
                majorAxis = Double.NaN;
            }
            if (!inverseF.HasValue) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Spheroid","No inverse flattening.");
                inverseF = Double.NaN;
            }

            return new OgcSpheroid(
                new SpheroidEquatorialInvF(
                    majorAxis.GetValueOrDefault(),
                    inverseF.GetValueOrDefault()
                ),
                name,
                OgcLinearUnit.DefaultMeter,
                authority
            );
        }

        public IPrimeMeridianInfo ReadPrimeMeridianFromParams() {
            Contract.Ensures(Contract.Result<IPrimeMeridianInfo>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            double longitude = 0;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is double)
                    longitude = (double)parameter;
            }

            if (null != authority) {
                var primeMeridian = Options.GetPrimeMeridian(authority);
                if (null != primeMeridian)
                    return primeMeridian;
            }

            return new OgcPrimeMeridian(
                name,
                longitude,
                OgcAngularUnit.DefaultDegrees,// TODO: from options?
                authority
            );
        }

        public IDatumGeodetic ReadHorizontalDatumFromParams() {
            Contract.Ensures(Contract.Result<IDatumGeodetic>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            ISpheroidInfo spheroid = null;
            Helmert7Transformation toWgs84 = null;
            IPrimeMeridianInfo primeMeridian = null;
            foreach (var parameter in ReadParams()) {
                if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is string)
                    name = (string)parameter;
                else if (parameter is ISpheroidInfo)
                    spheroid = (ISpheroidInfo)parameter;
                else if (parameter is Helmert7Transformation)
                    toWgs84 = (Helmert7Transformation)parameter;
                else if (parameter is IPrimeMeridianInfo)
                    primeMeridian = (IPrimeMeridianInfo)parameter; // NOTE: this may not happen due to the spec... but just in case?
            }

            if (null != authority) {
                var datum = Options.GetDatum(authority) as IDatumGeodetic;
                if (null != datum)
                    return datum;
            }

            if (spheroid == null) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Horizontal Datum","No spheroid.");
                spheroid = OgcSpheroid.DefaultWgs84;
            }

            return new OgcDatumHorizontal(
                name,
                spheroid,
                primeMeridian,
                toWgs84,
                authority
            );
        }

        public IDatum ReadBasicDatumFromParams(OgcDatumType defaultDatumType) {
            Contract.Ensures(Contract.Result<IDatum>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            var datumType = OgcDatumType.None;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is double)
                    datumType = (OgcDatumType)(int)(double)parameter;
                else if (parameter is int)
                    datumType = (OgcDatumType)(int)parameter;
            }

            if (null != authority) {
                var datum = Options.GetDatum(authority);
                if (null != datum)
                    return datum;
            }

            return new OgcDatum(
                name,
                datumType,
                authority
            );
        }

        private ICoordinateOperationMethodInfo ReadCoordinateOperationMethodFromParams() {
            Contract.Ensures(Contract.Result<ICoordinateOperationMethodInfo>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            foreach (var parameter in ReadParams()) {
                if (parameter is string)
                    name = (string)parameter;
                else if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter; // NOTE: this is in the spec for PROJECTION but does not appear to be used in practice
            }

            if (null != authority) {
                var methodInfo = Options.GetCoordinateOperationMethod(authority);
                if (null != methodInfo)
                    return methodInfo;
            }

            return new OgcCoordinateOperationMethodInfo(
                name,
                authority
            );
        }

        public Helmert7Transformation ReadToWgs84FromParams() {
            Contract.Ensures(Contract.Result<Helmert7Transformation>() != null);
            var allDoubles = ReadParams().OfType<double>().ToList();

            while (allDoubles.Count < 7)
                allDoubles.Add(0);

            return new Helmert7Transformation(
                new Vector3(allDoubles[0], allDoubles[1], allDoubles[2]),
                new Vector3(allDoubles[3], allDoubles[4], allDoubles[5]),
                allDoubles[6]
            );
        }

        public IUnit ReadUnitFromParams(bool isLength = true) {
            Contract.Ensures(Contract.Result<IUnit>() != null);
            IAuthorityTag authority = null;
            var name = String.Empty;
            double? factor = null;

            foreach (var parameter in ReadParams()) {
                if (parameter is IAuthorityTag)
                    authority = (IAuthorityTag)parameter;
                else if (parameter is string)
                    name = (string)parameter;
                else if (parameter is double)
                    factor = (double)parameter;
            }

            if (null != authority) {
                var unit = Options.GetUom(authority);
                if (null != unit)
                    return unit;
            }

            if (!factor.HasValue) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Unit","No base conversion factor.");
                factor = 1.0;
            }

            return isLength
                ? (IUnit)new OgcLinearUnit(name, factor.GetValueOrDefault(), authority)
                : new OgcAngularUnit(name, factor.GetValueOrDefault(), authority);
        }

        public object ReadObject() {
            var keyword = ReadKeyword();
            if (WktKeyword.Invalid == keyword) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Unknown keyword.");
                return null;
            }

            switch (keyword) {
            case WktKeyword.North:
            case WktKeyword.South:
            case WktKeyword.East:
            case WktKeyword.West:
            case WktKeyword.Up:
            case WktKeyword.Down:
            case WktKeyword.Other:
                return keyword;
            }

            if (!ReadOpenBracket()) {
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Open bracket expected.");
                return null;
            }

            switch (keyword) {
            case WktKeyword.Axis:
                return ReadAxisFromParams();
            case WktKeyword.Authority:
                return ReadAuthorityFromParams();
            case WktKeyword.Parameter:
                return ReadParameterFromParams();
            case WktKeyword.ParamMt:
                return ReadParamMtFromParams();
            case WktKeyword.ConcatMt:
                return ReadConcatMtFromParams();
            case WktKeyword.InverseMt:
                return ReadInverseFromParams();
            case WktKeyword.PassThroughMt:
                return ReadPassThroughFromParams();
            case WktKeyword.CompoundCs:
                return ReadCompoundCsFromParams();
            case WktKeyword.ProjectedCs:
                return ReadProjectedCsFromParams();
            case WktKeyword.GeographicCs:
                return ReadGeographicCsFromParams();
            case WktKeyword.GeocentricCs:
                return ReadGeocentricCsFromParams();
            case WktKeyword.VerticalCs:
                return ReadVerticalCsFromParams();
            case WktKeyword.Spheroid:
                return ReadSpheroidFromParams();
            case WktKeyword.PrimeMeridian:
                return ReadPrimeMeridianFromParams();
            case WktKeyword.Unit:
                return ReadUnitFromParams();
            case WktKeyword.Datum:
                return ReadHorizontalDatumFromParams();
            case WktKeyword.VerticalDatum:
                return ReadBasicDatumFromParams(OgcDatumType.VerticalOther);
            case WktKeyword.LocalDatum:
                return ReadBasicDatumFromParams(OgcDatumType.LocalOther);
            case WktKeyword.ToWgs84:
                return ReadToWgs84FromParams();
            case WktKeyword.Projection:
                return ReadCoordinateOperationMethodFromParams();
            case WktKeyword.FittedCs:
                return ReadFittedCsFromParams();
            case WktKeyword.LocalCs:
                return ReadLocalCsFromParams();
            default:
                if(Options.ThrowOnError)
                    throw new WktParseExceptioncs("Object type not supported: " + keyword);
                return null;
            }
        }

        public object ReadEntity() {
            if (!SkipWhiteSpace())
                return null;
            if (IsDoubleQuote)
                return ReadQuotedString();
            if (IsKeyword)
                return ReadObject();
            if (IsValidForDoubleValue)
                return ReadDouble();
            return null;
        }

        private WktKeyword ReadKeywordSubmatch(string subMatch, WktKeyword keyword) {
            Contract.Requires(!String.IsNullOrEmpty(subMatch));
            return ReadKeywordSubmatch(subMatch) ? keyword : WktKeyword.Invalid;
        }

        private bool ReadKeywordSubmatch(string subMatch) {
            Contract.Requires(!String.IsNullOrEmpty(subMatch));
            for (var i = 0; i < subMatch.Length; i++) {
                if (
                    (CurrentUpperInvariant != subMatch[i])
                    ||
                    (!MoveNext() && (i + 1) != subMatch.Length)
                ) {
                    return false;
                }
            }
            return true;
        }

        private WktKeyword ReadKeywordA() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'U': return ReadKeywordSubmatch("THORITY", WktKeyword.Authority);
                case 'X': return ReadKeywordSubmatch("IS", WktKeyword.Axis);
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordC() {
            if (ReadKeywordSubmatch("O")) {
                var previousChar = CurrentUpperInvariant;
                if (MoveNext()) {
                    switch (previousChar) {
                    case 'M':
                        return ReadKeywordSubmatch("PD_CS", WktKeyword.CompoundCs);
                    case 'N':
                        return ReadKeywordSubmatch("CAT_MT", WktKeyword.ConcatMt);
                    }
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordG() {
            if (ReadKeywordSubmatch("EO")) {
                var previousChar = CurrentUpperInvariant;
                if (MoveNext()) {
                    WktKeyword temp;
                    switch (previousChar) {
                    case 'C': {
                            temp = WktKeyword.GeocentricCs;
                            break;
                        }
                    case 'G': {
                            temp = WktKeyword.GeographicCs;
                            break;
                        }
                    default: return WktKeyword.Invalid;
                    }
                    return ReadKeywordSubmatch("CS")
                        ? temp
                        : WktKeyword.Invalid; // TODO: can the overload be used to eliminate the ternary operator?
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordL() {
            if (ReadKeywordSubmatch("OCAL_")) {
                var previousChar = CurrentUpperInvariant;
                if (MoveNext()) {
                    switch (previousChar) {
                    case 'C': return ReadKeywordSubmatch("S", WktKeyword.LocalCs);
                    case 'D': return ReadKeywordSubmatch("ATUM", WktKeyword.LocalDatum);
                    }
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordV() {
            if (ReadKeywordSubmatch("ERT_")) {
                var previousChar = CurrentUpperInvariant;
                if (MoveNext()) {
                    switch (previousChar) {
                    case 'D': return ReadKeywordSubmatch("ATUM", WktKeyword.VerticalDatum);
                    case 'C': return ReadKeywordSubmatch("S", WktKeyword.VerticalCs);
                    }
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordPa() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'S': return ReadKeywordSubmatch("STHROUGH_MT", WktKeyword.PassThroughMt);
                case 'R': {
                        if (ReadKeywordSubmatch("AM")) {
                            previousChar = CurrentUpperInvariant;
                            if (MoveNext()) {
                                switch (previousChar) {
                                case 'E': return ReadKeywordSubmatch("TER", WktKeyword.Parameter);
                                case '_': return ReadKeywordSubmatch("MT", WktKeyword.ParamMt);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordPr() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'I': return ReadKeywordSubmatch("MEM", WktKeyword.PrimeMeridian);
                case 'O': {
                        if (ReadKeywordSubmatch("J")) {
                            previousChar = CurrentUpperInvariant;
                            if (MoveNext()) {
                                switch (previousChar) {
                                case 'C': return ReadKeywordSubmatch("S", WktKeyword.ProjectedCs);
                                case 'E': return ReadKeywordSubmatch("CTION", WktKeyword.Projection);
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordP() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'A': return ReadKeywordPa();
                case 'R': return ReadKeywordPr();
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordS() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'O': return ReadKeywordSubmatch("UTH", WktKeyword.South);
                case 'P': return ReadKeywordSubmatch("HEROID", WktKeyword.Spheroid);
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordU() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'N': return ReadKeywordSubmatch("IT", WktKeyword.Unit);
                case 'P': return WktKeyword.Up;
                }
            }
            return WktKeyword.Invalid;
        }

        private WktKeyword ReadKeywordD() {
            var previousChar = CurrentUpperInvariant;
            if (MoveNext()) {
                switch (previousChar) {
                case 'A': return ReadKeywordSubmatch("TUM", WktKeyword.Datum);
                case 'O': return ReadKeywordSubmatch("WN", WktKeyword.Down);
                }
            }
            return WktKeyword.Invalid;
        }

        public WktKeyword ReadKeyword() {
            var previousChar = CurrentUpperInvariant;
            if (!MoveNext())
                return WktKeyword.Invalid;

            switch (previousChar) {
            case 'A': return ReadKeywordA();
            case 'C': return ReadKeywordC();
            case 'D': return ReadKeywordD();
            case 'E': return ReadKeywordSubmatch("AST", WktKeyword.East);
            case 'F': return ReadKeywordSubmatch("ITTED_CS", WktKeyword.FittedCs);
            case 'G': return ReadKeywordG();
            case 'I': return ReadKeywordSubmatch("NVERSE_MT", WktKeyword.InverseMt);
            case 'L': return ReadKeywordL();
            case 'O': return ReadKeywordSubmatch("THER", WktKeyword.Other);
            case 'P': return ReadKeywordP();
            case 'N': return ReadKeywordSubmatch("ORTH", WktKeyword.North);
            case 'S': return ReadKeywordS();
            case 'T': return ReadKeywordSubmatch("OWGS84", WktKeyword.ToWgs84);
            case 'U': return ReadKeywordU();
            case 'V': return ReadKeywordV();
            case 'W': return ReadKeywordSubmatch("EST", WktKeyword.West);
            default: return WktKeyword.Invalid;
            }
        }

    }
}
