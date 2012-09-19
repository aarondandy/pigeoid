using System;
using System.Collections;
using System.Collections.Generic;
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

		private TextReader _reader;
		private char _current;

		public WktReader(TextReader reader, WktOptions options = null)
		{
			if(null == reader)
				throw new ArgumentNullException("reader");

			_reader = reader;
			Options = options ?? new WktOptions();
		}

		public WktOptions Options { get; private set; }


		public char Current { get { return _current; } }

		public char CurrentUpperInvariant { get { return Char.ToUpperInvariant(Current); }}

		public void Dispose() {
			_reader = null;
		}

		object IEnumerator.Current { get { return Current; } }

		public bool MoveNext() {
			var iChar = _reader.Read();
			if (iChar >= 0) {
				_current = (char)iChar;
				return true;
			}
			_current = '\0';
			return false;
		}

		public void Reset() {
			throw new NotSupportedException();
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>This method will advance the stream.</remarks>
		public double? ReadDouble() {
			var builder = new StringBuilder();
			while(IsValidForDoubleValue) {
				builder.Append(Current);
				if(!MoveNext()) {
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
			var paramResults = new List<object>();
			while (true) {
				if (!SkipWhiteSpace())
					return null;

				var paramEntity = ReadEntity();
				if (null == paramEntity)
					return null;

				paramResults.Add(paramEntity);
				if (!SkipWhiteSpace())
					return null;

				if (IsComma) {
					if (!MoveNext())
						return null;
				}
				else {
					if (IsCloseBracket && ReadCloseBracket())
						break;

					return null;
				}
			}
			return paramResults.ToArray();
		}

		public IAuthorityTag ReadAuthorityFromParams() {
			var names = ReadParams()
				.Cast<string>() // TODO: convert?
				.ToArray();
			if (names.Length < 2)
				return null;

			var tag = Options.ResolveAuthorities
				? Options.GetAuthorityTag(names[0], names[1])
				: null;
			return tag ?? new AuthorityTag(names[0], names[1]);
		}

		private bool IsDirectionKeyword(WktKeyword keyword) {
			switch(keyword) {
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
			var allParams = ReadParams();
			
			var name = allParams.OfType<string>().FirstOrDefault();
			if (null == name)
				name = String.Empty;

			var direction = allParams.OfType<WktKeyword>().FirstOrDefault();
			if (!IsDirectionKeyword(direction))
				direction = WktKeyword.Other;

			return new OgcAxis(name, ToOgcOrientationType(direction));
		}

		public INamedParameter ReadParameterFromParams() {
			var allParams = ReadParams();
			if (allParams.Length < 2)
				return null;

			var name = allParams[0] as string;
			if (null == name)
				name = String.Empty;
			if (Options.CorrectNames)
				name = name.Replace('_', ' ');

			var value = allParams[1];
			if (value is double)
				return new NamedParameter<double>(name, (double)value);
			if (value is string)
				return new NamedParameter<string>(name, (string)value);
			return new NamedParameter<object>(name, value);
		}

		public ICoordinateOperationInfo ReadParamMtFromParams() {
			var allParams = ReadParams();
			if (allParams.Length == 0)
				return null;

			var name = allParams[0] as string;
			if (null == name)
				name = String.Empty;

			return new CoordinateOperationInfo(name, allParams.Skip(1).OfType<INamedParameter>());
		}

		public IConcatenatedCoordinateOperationInfo ReadConcatMtFromParams() {
			return new ConcatenatedCoordinateOperationInfo(
				ReadParams()
				.OfType<ICoordinateOperationInfo>()
			);
		}

		public ICrsCompound ReadCompoundCsFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var crs = Options.GetCrs(authority) as ICrsCompound;
			if(null != crs)
				return crs;

			var allCrs = allParams.OfType<ICrs>().ToArray();
			if(allCrs.Length != 2 && Options.ThrowOnError)
				throw new InvalidDataException("A compound CRS requires a head and tail CRS.");

			var name = allParams.OfType<string>().FirstOrDefault();
			if (null == name)
				name = String.Empty;

			return new OgcCrsCompound(
				name,
				allCrs.Length > 0 ? allCrs[0] : null,
				allCrs.Length > 1 ? allCrs[1] : null,
				authority
			);
		}

		public ICrsProjected ReadProjectedCsFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var crs = Options.GetCrs(authority) as ICrsProjected;
			if (null != crs)
				return crs;

			var name = allParams.OfType<string>().FirstOrDefault();
			var baseCrs = allParams.OfType<ICrsGeodetic>().FirstOrDefault();
			var operationMethod = allParams.OfType<ICoordinateOperationMethodInfo>().FirstOrDefault();
			var operationMethodName = null == operationMethod
				? String.Empty
				: (operationMethod.Name ?? String.Empty);
			var parameters = allParams.OfType<INamedParameter>().ToArray();
			var linearUnit = allParams.OfType<IUom>().FirstOrDefault();
			var axes = allParams.OfType<IAxis>();

			return new OgcCrsProjected(
				name,
				baseCrs,
				new CoordinateOperationInfo(operationMethodName, parameters), 
				linearUnit,
				axes,
				authority
			);
		}

		private ICrsGeocentric ReadGeocentricCsFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var crs = Options.GetCrs(authority) as ICrsGeocentric;
			if (null != crs)
				return crs;

			var name = allParams.OfType<string>().FirstOrDefault();

			var datum = allParams.OfType<IDatumGeodetic>().FirstOrDefault();
			var primeMeridian = allParams.OfType<IPrimeMeridianInfo>().FirstOrDefault();
			datum = AttemptDatumPrimeMeridianCorrection(datum, primeMeridian);

			var uom = allParams.OfType<IUom>().FirstOrDefault();
			var axes = allParams.OfType<IAxis>().ToArray();
			
			return new OgcCrsGeocentric(
				name,
				datum,
				uom,
				axes,
				authority
			);
		}

		private IDatumGeodetic AttemptDatumPrimeMeridianCorrection(IDatumGeodetic datum, IPrimeMeridianInfo primeMeridian) {
			var datumAuthority = Options.GetAuthorityTag(datum);
			var authoritativeDatum = null == datumAuthority ? null : Options.GetDatum(datumAuthority) as IDatumGeodetic;
			if (null != authoritativeDatum)
				return authoritativeDatum; // that is that, leave it as is!

			return new OgcDatumHorizontal(Options.GetEntityName(datum), datum.Spheroid, primeMeridian, datum.BasicWgs84Transformation, datumAuthority);
		}

		private ICrsGeographic ReadGeographicCsFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var crs = Options.GetCrs(authority) as ICrsGeographic;
			if (null != crs)
				return crs;

			var name = allParams.OfType<string>().FirstOrDefault();

			var datum = allParams.OfType<IDatumGeodetic>().FirstOrDefault();
			var primeMeridian = allParams.OfType<IPrimeMeridianInfo>().FirstOrDefault();
			datum = AttemptDatumPrimeMeridianCorrection(datum, primeMeridian);

			var uom = allParams.OfType<IUom>().FirstOrDefault();
			var axes = allParams.OfType<IAxis>().ToArray();

			return new OgcCrsGeographic(
				name,
				datum,
				uom,
				axes,
				authority
			);
		}

		public ICoordinateOperationInfo ReadInverseFromParams() {
			var allParams = ReadParams().Cast<ICoordinateOperationInfo>().ToArray();
			return allParams.Length == 1
				? allParams[0].GetInverse()
				: null;
		}

		public ICoordinateOperationInfo ReadPassThroughFromParams() {
			var allParams = ReadParams();
			var index = Convert.ToInt32(
				allParams.First(o => null != o && (o is int || o is double)));

			return new OgcPassThroughCoordinateOperationInfo(
				allParams.Skip(1).First() as ICoordinateOperationInfo,
				index
			);
		}

		public ISpheroidInfo ReadSpheroidFromParams() {
			var allParams = ReadParams();
			if (allParams == null || allParams.Length < 3) 
				return null;

			var name = allParams[0].ToString();
			var majorAxis = Convert.ToDouble(allParams[1]);
			var inverseF = Convert.ToDouble(allParams[2]);
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();

			var spheroid = new SpheroidEquatorialInvF(majorAxis, inverseF);
			return Options.GetSpheroid(authority)
				?? new OgcSpheroid(spheroid, name, authority);
		}

		[Obsolete("Singleton?")]
		private OgcAngularUnit GenerateDegreeUnit() {
			return new OgcAngularUnit(
				"degree",
				0.0174532925199433,
				new AuthorityTag("EPSG","9101")
			);
		}

		public IPrimeMeridianInfo ReadPrimeMeridianFromParams(IUom angularUnit = null) {
			var allParams = ReadParams();

			var authorityTag = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var primeMeridian = Options.GetPrimeMeridian(authorityTag);
			if (null != primeMeridian)
				return primeMeridian;

			var name = allParams.OfType<string>().FirstOrDefault();
			var longitude = allParams.OfType<double>().FirstOrDefault();
			angularUnit = angularUnit ?? GenerateDegreeUnit();

			return new OgcPrimeMeridian(name, longitude, angularUnit, authorityTag);
		}

		public IDatum ReadDatumFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var datum = Options.GetDatum(authority);
			if (null != datum)
				return datum;

			var name = allParams.OfType<string>().FirstOrDefault();
			if(null == name)
				name = String.Empty;

			var spheroid = allParams.OfType<ISpheroidInfo>().FirstOrDefault();
			var wgs84Transformation = allParams.OfType<Helmert7Transformation>().FirstOrDefault();
			var primeMeridian = allParams.OfType<IPrimeMeridianInfo>().FirstOrDefault(); // NOTE: should be null
			return new OgcDatumHorizontal(
				name,
				spheroid,
				primeMeridian,
				wgs84Transformation,
				authority
			);
		}

		private ICoordinateOperationMethodInfo ReadCoordinateOperationMethodFromParams() {
			var allParams = ReadParams();
			var authority = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var method = Options.GetCoordinateOperationMethod(authority);
			if (null != method)
				return method;

			var name = allParams.OfType<string>().FirstOrDefault();
			if (null == name)
				name = String.Empty;

			return new OgcCoordinateOperationMethodInfo(name, authority);
		}

		public Helmert7Transformation ReadToWgs84FromParams() {
			var allParams = ReadParams();
			
			var allDoubles = allParams.OfType<double>().ToList();
			while(allDoubles.Count < 6)
				allDoubles.Add(0);
			if (allDoubles.Count < 7)
				allDoubles.Add(1);

			return new Helmert7Transformation(
				new Vector3(allDoubles[0], allDoubles[1], allDoubles[2]),
				new Vector3(allDoubles[3], allDoubles[4], allDoubles[5]),
 				allDoubles[6]
			);
		}

		public IUom ReadUnitFromParams(bool isLength = true) {
			var allParams = ReadParams();
			if (allParams == null || allParams.Length == 0)
				return null;

			var authorityTag = allParams.OfType<IAuthorityTag>().FirstOrDefault();
			var uom = Options.GetUom(authorityTag);
			if (null != uom)
				return uom;

			var name = allParams[0] as string;
			var factor = allParams.Length > 1 ? Convert.ToDouble(allParams[1]) : 1.0;
			
			return  isLength
				? (IUom)new OgcLinearUnit(name, factor, authorityTag)
				: new OgcAngularUnit(name, factor, authorityTag);
		}

		public object ReadObject() {
			var keyword = ReadKeyword();
			if (WktKeyword.Invalid == keyword) { return null; }

			switch(keyword) {
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
			case WktKeyword.Spheroid:
				return ReadSpheroidFromParams();
			case WktKeyword.PrimeMeridian:
				return ReadPrimeMeridianFromParams();
			case WktKeyword.Unit:
				return ReadUnitFromParams();
			case WktKeyword.Datum:
				return ReadDatumFromParams();
			case WktKeyword.ToWgs84:
				return ReadToWgs84FromParams();
			case WktKeyword.Projection:
				return ReadCoordinateOperationMethodFromParams();
			default:
				throw new NotSupportedException("Object type not supported: " + keyword);
			}
		}

		public object ReadEntity() {
			if (!SkipWhiteSpace()) {
				return null;
			}
			if (IsDoubleQuote) {
				return ReadQuotedString();
			}
			if (IsKeyword) {
				return ReadObject();
			}
			if (IsValidForDoubleValue) {
				return ReadDouble();
			}
			return null;
		}

		private WktKeyword ReadKeywordSubmatch(string submatch, WktKeyword keyword) {
			return ReadKeywordSubmatch(submatch) ? keyword : WktKeyword.Invalid;
		}

		private bool ReadKeywordSubmatch(string submatch) {
			for (var i = 0; i < submatch.Length; i++) {
				if (
					(CurrentUpperInvariant != submatch[i])
					||
					(!MoveNext() && (i + 1) != submatch.Length)
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
			if(MoveNext()) {
				switch(previousChar) {
				case 'O': return ReadKeywordSubmatch("UTH", WktKeyword.South);
				case 'P': return ReadKeywordSubmatch("HEROID", WktKeyword.Spheroid);
				}
			}
			return WktKeyword.Invalid;
		}

		private WktKeyword ReadKeywordU() {
			var previousChar = CurrentUpperInvariant;
			if(MoveNext()) {
				switch(previousChar) {
				case 'N': return ReadKeywordSubmatch("IT", WktKeyword.Unit);
				case 'P': return WktKeyword.Up;
				}
			}
			return WktKeyword.Invalid;
		}

		private WktKeyword ReadKeywordD() {
			var previousChar = CurrentUpperInvariant;
			if(MoveNext()) {
				switch(previousChar) {
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
