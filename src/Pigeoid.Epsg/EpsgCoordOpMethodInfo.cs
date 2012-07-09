// TODO: source header

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCoordOpMethodInfo
	{

		internal class EpsgCoordOpMethodInfoLookup : EpsgDynamicLookupBase<ushort, EpsgCoordOpMethodInfo>
		{
			private const string DatFileName = "opmethod.dat";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = sizeof(ushort) + sizeof(byte);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int CodeSize = sizeof(ushort);

			private static ushort[] GetKeys()
			{
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					var keys = new ushort[reader.ReadUInt16()];
					for (int i = 0; i < keys.Length; i++) {
						keys[i] = reader.ReadUInt16();
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
					return keys;
				}
			}

			public EpsgCoordOpMethodInfoLookup() : base(GetKeys()) { }

			protected override EpsgCoordOpMethodInfo Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
					var reverse = reader.ReadByte() == 'B';
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), "opmethod.txt");
					return new EpsgCoordOpMethodInfo(key, name, reverse);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordOpMethodInfo value) {
				return value._code;
			}
			
		}

		private class EpsgCoordOpMethodParamInfoLookup
		{

			private class OpParamValueDynamicLookup : EpsgDynamicLookupBase<ushort, OpParamValueInfo>
			{

				private readonly EpsgCoordOpMethodParamInfoLookup _parent;
				private readonly int _valueDataOffset;
				private readonly int _opDataSize;

				internal OpParamValueDynamicLookup(EpsgCoordOpMethodParamInfoLookup parent, ushort[] opCodes)
					: base(opCodes) {
					_parent = parent;
					_valueDataOffset = sizeof(byte) // usage count
						+ ((sizeof(ushort) + sizeof(byte)) * _parent._paramUsage.Length) // usage data
						+ sizeof(ushort); // op count
					_opDataSize = sizeof(ushort) // op code
						+ ((sizeof(ushort) * 2) * _parent._paramUsage.Length); // values
				}

				protected override OpParamValueInfo Create(ushort key, int index) {
					using (var numberLookup = new EpsgNumberLookup())
					using (var readerTxt = EpsgDataResource.CreateBinaryReader(ParamTextValueFileName))
					using (var reader = EpsgDataResource.CreateBinaryReader(_parent._paramDatFileName)) {
						reader.BaseStream.Seek(
							_valueDataOffset // header and usage data
							+ (index * _opDataSize) // previous op data
							+ sizeof(ushort) // current opCode
							, SeekOrigin.Begin);
						var paramValues = new List<INamedParameter>(_parent._paramUsage.Length);
						for (int i = 0; i < _parent._paramUsage.Length; i++) {
							var valueCode = reader.ReadUInt16();
							var uomCode = reader.ReadUInt16();
							if(valueCode != 0xffff) {
								string paramName = _parent._paramUsage[i].Parameter.Name;
								var uom = EpsgUom.Get(uomCode);
								paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookup));
							}
						}
						return new OpParamValueInfo(key, paramValues.ToArray());
					}
				}

				protected override ushort GetKeyForItem(OpParamValueInfo value) {
					return (ushort)value.OpCode;
				}
			}

			private const string ParamTextValueFileName = "params.txt";
			private const int FixedLookupThreshold = 8;

			private static INamedParameter CreateParameter(ushort valueCode, string paramName, EpsgUom uom, BinaryReader readerTxt, EpsgNumberLookup numberLookup) {
				return ((valueCode & 0xc000) == 0x8000)
					? new NamedParameter<string>(paramName,
						EpsgTextLookup.GetString((ushort)(valueCode & 0x7fff), readerTxt),
						uom) as INamedParameter
					: new NamedParameter<double>(paramName, numberLookup.Get(valueCode), uom);
			}

			private static EpsgFixedLookupBase<ushort, OpParamValueInfo> CreateFullLookup(BinaryReader reader, int opCount, ParamUsage[] paramUsage) {
				var lookupDictionary = new SortedDictionary<ushort, OpParamValueInfo>();
				var paramValues = new List<INamedParameter>();
				using (var numberLookup = new EpsgNumberLookup())
				using (var readerTxt = EpsgDataResource.CreateBinaryReader(ParamTextValueFileName)) {
					for (int opIndex = 0; opIndex < opCount; opIndex++) {
						var opCode = reader.ReadUInt16();
						paramValues.Clear();
						for (int paramIndex = 0; paramIndex < paramUsage.Length; paramIndex++) {
							var valueCode = reader.ReadUInt16();
							var uomCode = reader.ReadUInt16();
							if (valueCode != 0xffff) {
								string paramName = paramUsage[paramIndex].Parameter.Name;
								var uom = EpsgUom.Get(uomCode);
								paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookup));
							}
						}
						lookupDictionary.Add(opCode, new OpParamValueInfo(opCode, paramValues.ToArray()));
					}
				}
				return new EpsgFixedLookupBase<ushort, OpParamValueInfo>(lookupDictionary);
			}

			private readonly ushort _coordOpCode;
			private readonly ParamUsage[] _paramUsage;
			private readonly EpsgLookupBase<ushort, OpParamValueInfo> _valueLookup;
			private readonly string _paramDatFileName;

			public EpsgCoordOpMethodParamInfoLookup(ushort coordOpCode) {
				_coordOpCode = coordOpCode;
				_paramDatFileName = "param" + _coordOpCode + ".dat";
				using (var reader = EpsgDataResource.CreateBinaryReader(_paramDatFileName)) {
					_paramUsage = new ParamUsage[reader.ReadByte()];
					for (int i = 0; i < _paramUsage.Length; i++) {
						var paramInfo = EpsgParameterInfo.Get(reader.ReadUInt16());
						var signRev = 0x01 == reader.ReadByte();
						_paramUsage[i] = new ParamUsage(paramInfo, signRev);
					}
					var opCount = reader.ReadUInt16();
					if (opCount <= FixedLookupThreshold) {
						_valueLookup = CreateFullLookup(reader, opCount, _paramUsage);
					}
					else {
						var opCodes = new ushort[opCount];
						var opSkip = _paramUsage.Length * (sizeof(ushort) * 2);
						for (int i = 0; i < opCodes.Length; i++) {
							opCodes[i] = reader.ReadUInt16();
							reader.BaseStream.Seek(opSkip, SeekOrigin.Current);
						}
						_valueLookup = new OpParamValueDynamicLookup(this, opCodes);
					}
				}
			}

			public ReadOnlyCollection<ParamUsage> ParameterUsage { get { return Array.AsReadOnly(_paramUsage); } }

			public OpParamValueInfo GetParameterValueInfo(int operationCode) {
				return operationCode < 0 || operationCode > UInt16.MaxValue
					? null
					: _valueLookup.Get((ushort) operationCode);
			}
		}

		public class ParamUsage
		{
			private readonly EpsgParameterInfo _parameter;
			private readonly bool _signRev;

			internal ParamUsage(EpsgParameterInfo parameter, bool signRev) {
				_parameter = parameter;
				_signRev = signRev;
			}

			public EpsgParameterInfo Parameter { get { return _parameter; } }

			public bool SignReversal { get { return _signRev; } }

		}

		public class OpParamValueInfo
		{
			private readonly ushort _opCode;
			private readonly INamedParameter[] _values;

			internal OpParamValueInfo(ushort opCode, INamedParameter[] values) {
				_opCode = opCode;
				_values = values;
			}

			public int OpCode { get { return _opCode; }}
			public ReadOnlyCollection<INamedParameter> Values { get { return Array.AsReadOnly(_values); }}
		}

		internal static readonly EpsgCoordOpMethodInfoLookup Lookup = new EpsgCoordOpMethodInfoLookup();

		public static EpsgCoordOpMethodInfo Get(int code) {
			return code >= 0 && code <= UInt16.MaxValue
				? Lookup.Get(unchecked((ushort)code))
				: null;
		}

		public static IEnumerable<EpsgCoordOpMethodInfo> Values { get { return Lookup.Values; } }

		private readonly ushort _code;
		private readonly string _name;
		private readonly bool _canReverse;
		private readonly Lazy<EpsgCoordOpMethodParamInfoLookup> _paramData;

		private EpsgCoordOpMethodInfo(ushort code, string name, bool canReverse)
		{
			_code = code;
			_name = name;
			_canReverse = canReverse;
			_paramData = new Lazy<EpsgCoordOpMethodParamInfoLookup>(
				CreateParamInfoLookup, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private EpsgCoordOpMethodParamInfoLookup CreateParamInfoLookup() {
			return new EpsgCoordOpMethodParamInfoLookup(_code);
		}

		public int Code { get { return _code; } }
		public string Name { get { return _name; } }
		public bool CanReverse { get { return _canReverse; } }

		public ReadOnlyCollection<ParamUsage> ParameterUsage { get { return _paramData.Value.ParameterUsage; } }

		public ReadOnlyCollection<INamedParameter> GetOperationParameters(int operationCode) {
			var info = _paramData.Value.GetParameterValueInfo(operationCode);
			return null == info ? null : info.Values;
		}


	}
}
