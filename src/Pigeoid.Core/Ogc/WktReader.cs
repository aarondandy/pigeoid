using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

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

			return Options.CreateAuthority(names[0], names[1]);
		}

		public INamedParameter ReadParameterFromParams() {
			var allParams = ReadParams();
			if (allParams.Length < 2)
				return null;

			var name = allParams[0] as string;
			if (String.IsNullOrEmpty(name))
				return null;

			var value = allParams[1];
			name = name.Replace('_', ' ');
			if (value is double) {
				return new NamedParameter<double>(name, (double)value);
			}
			return new NamedParameter<object>(name, value);
		}

		public object ReadParamMtFromParams() {
			var allParams = ReadParams();
			if (allParams.Length == 0)
				return null;

			var name = allParams[0] as string;
			if (String.IsNullOrEmpty(name))
				return null;

			return new CoordinateOperationInfo(name, allParams.Skip(1).Cast<INamedParameter>());
		}

		public object ReadConcatMtFromParams() {
			return new ConcatenatedCoordinateOperationInfo(ReadParams().Cast<ICoordinateOperationInfo>());
		}

		public object ReadInverseFromParams() {
			var allParams = ReadParams().Cast<object>().ToArray();
			if (allParams.Length != 1)
				return null;
			//return new CoordinateOperationInverse(allParams[0]);
			throw new NotImplementedException();
		}

		public object ReadPassThroughFromParams() {
			var allParams = ReadParams();
			var index = Convert.ToInt32(
				allParams.First(o => null != o && (o is int || o is double)));
			/*return allParams.Length > 1
				? new OgcPassthroughOperation(
					index,
					allParams
						.Skip(1)
						.OfType<ITransformation>()
						.FirstOrDefault()
				)
				: null
			;*/
			throw new NotImplementedException();
		}

		public ISpheroid<double> ReadSpheroidFromParams() {
			var allParams = ReadParams();
			if (allParams == null || allParams.Length < 3) 
				return null;

			var name = allParams[0] as string;
			var majorAxis = Convert.ToDouble(allParams[1]);
			var inverseF = Convert.ToDouble(allParams[2]);
			var authority = allParams.Length > 3
				? allParams[3] as IAuthorityTag
				: null;
			var spheroid = new SpheroidEquatorialInvF(majorAxis, inverseF);
			/*return
				(String.IsNullOrEmpty(name) && null == authority)
				? spheroid
				: (
					_authFacs.CreateSpheroid(authority)
					?? new OgcSpheroid(spheroid, name, authority)
				)
			;*/
			throw new NotImplementedException();
		}

		public IPrimeMeridian ReadPrimeMFromParams(IUom angularUnit = null) {
			/*angularUnit = angularUnit
				?? _uomGraph.GetUnit("angle", "degree")
			;*/
			throw new NotImplementedException();

			var all = ReadParams();
			if (all == null || all.Length < 2) {
				return null;
			}
			var name = all[0] as string;
			var lon = Convert.ToDouble(all[1]);
			var auth = all.Length > 2 ? all[2] as IAuthorityTag : null;
			/*return
				_authFacs.CreatePrimeMeridian(auth)
				?? new OgcPrimeMeridian(name, lon, angularUnit, auth)
			;*/
			throw new NotImplementedException();
		}

		public IUom ReadUnitFromParams(bool isLength = true) {
			var all = ReadParams();
			if (all == null || all.Length < 2) {
				return null;
			}
			var name = all[0] as string;
			var fac = Convert.ToDouble(all[1]);
			var auth = all.Length > 2 ? all[2] as IAuthorityTag : null;
			/*return
				_authFacs.CreateUom(auth)
				?? (
					isLength
					? (IUom)new OgcLinearUnit(name, fac, auth)
					: new OgcAngularUnit(name, fac, auth)
				)
			;*/
			throw new NotImplementedException();
		}

		public object ReadObject() {
			WktKeyword keyword = ReadKeyword();
			if (WktKeyword.Invalid == keyword) { return null; }
			if (!ReadOpenBracket()) { return null; }
			switch (keyword) {
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
				case WktKeyword.Spheroid:
					return ReadSpheroidFromParams();
				case WktKeyword.PrimeMeridian:
					return ReadPrimeMFromParams();
				case WktKeyword.Unit:
					return ReadUnitFromParams();
				default:
					throw new NotSupportedException("Object type not supported.");
			}
		}

		public object ReadEntity() {
			if (!SkipWhiteSpace()) {
				return null;
			}
			if (IsValidForDoubleValue) {
				return ReadDouble();
			}
			if (IsDoubleQuote) {
				return ReadQuotedString();
			}
			if (IsKeyword) {
				return ReadObject();
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

		public WktKeyword ReadKeyword() {
			var previousChar = CurrentUpperInvariant;
			if (!MoveNext())
				return WktKeyword.Invalid;
			
			switch (previousChar) {
				case 'A': return ReadKeywordA();
				case 'C': return ReadKeywordC();
				case 'D': return ReadKeywordSubmatch("ATUM", WktKeyword.Datum);
				case 'F': return ReadKeywordSubmatch("ITTED_CS", WktKeyword.FittedCs);
				case 'G': return ReadKeywordG();
				case 'I': return ReadKeywordSubmatch("NVERSE_MT", WktKeyword.InverseMt);
				case 'L': return ReadKeywordL();
				case 'P': return ReadKeywordP();
				case 'S': return ReadKeywordSubmatch("PHEROID", WktKeyword.Spheroid);
				case 'T': return ReadKeywordSubmatch("OWGS84", WktKeyword.ToWgs84);
				case 'U': return ReadKeywordSubmatch("NIT", WktKeyword.Unit);
				case 'V': return ReadKeywordV();
				default: return WktKeyword.Invalid;
			}
		}

	}
}
