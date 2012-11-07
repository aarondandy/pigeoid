// TODO: source header

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCoordinateOperationMethodInfo : ICoordinateOperationMethodInfo
	{

		internal class EpsgCoordinateOperationMethodInfoLookUp : EpsgDynamicLookUpBase<ushort, EpsgCoordinateOperationMethodInfo>
		{
			private const string DatFileName = "opmethod.dat";
			private const string TxtFileName = "opmethod.txt";
			private const int FileHeaderSize = sizeof(ushort);
			private const int RecordDataSize = sizeof(ushort) + sizeof(byte);
			private const int RecordSize = sizeof(ushort) + RecordDataSize;
			private const int CodeSize = sizeof(ushort);

			private static readonly EpsgTextLookUp TextLookUp = new EpsgTextLookUp(TxtFileName);

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

			public EpsgCoordinateOperationMethodInfoLookUp() : base(GetKeys()) { }

			protected override EpsgCoordinateOperationMethodInfo Create(ushort key, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + FileHeaderSize + CodeSize, SeekOrigin.Begin);
					var reverse = reader.ReadByte() == 'B';
					var name = TextLookUp.GetString(reader.ReadUInt16());
					return new EpsgCoordinateOperationMethodInfo(key, name, reverse);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordinateOperationMethodInfo value) {
				return value._code;
			}
			
		}

		private class EpsgCoordinateOperationMethodParamInfoLookUp
		{

			private class OpParamValueDynamicLookUp : EpsgDynamicLookUpBase<ushort, OpParamValueInfo>
			{

				private readonly EpsgCoordinateOperationMethodParamInfoLookUp _parent;
				private readonly int _valueDataOffset;
				private readonly int _opDataSize;

				internal OpParamValueDynamicLookUp(EpsgCoordinateOperationMethodParamInfoLookUp parent, ushort[] opCodes)
					: base(opCodes) {
					_parent = parent;
					_valueDataOffset = sizeof(byte) // usage count
						+ ((sizeof(ushort) + sizeof(byte)) * _parent._paramUsage.Length) // usage data
						+ sizeof(ushort); // op count
					_opDataSize = sizeof(ushort) // op code
						+ ((sizeof(ushort) * 2) * _parent._paramUsage.Length); // values
				}

				protected override OpParamValueInfo Create(ushort key, int index) {
					using (var numberLookUp = new EpsgNumberLookUp())
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
								var uom = EpsgUnit.Get(uomCode);
								paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookUp));
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
			private const int FixedLookUpThreshold = 8;

			private static INamedParameter CreateParameter(ushort valueCode, string paramName, EpsgUnit unit, BinaryReader readerTxt, EpsgNumberLookUp numberLookUp) {
				return ((valueCode & 0xc000) == 0x8000)
					? new NamedParameter<string>(paramName,
						EpsgTextLookUp.GetString((ushort)(valueCode & 0x7fff), readerTxt),
						unit) as INamedParameter
					: new NamedParameter<double>(paramName, numberLookUp.Get(valueCode), unit);
			}

			private static EpsgFixedLookUpBase<ushort, OpParamValueInfo> CreateFullLookUp(BinaryReader reader, int opCount, ParamUsage[] paramUsage) {
				var lookUpDictionary = new SortedDictionary<ushort, OpParamValueInfo>();
				var paramValues = new List<INamedParameter>();
				using (var numberLookUp = new EpsgNumberLookUp())
				using (var readerTxt = EpsgDataResource.CreateBinaryReader(ParamTextValueFileName)) {
					for (int opIndex = 0; opIndex < opCount; opIndex++) {
						var opCode = reader.ReadUInt16();
						paramValues.Clear();
						for (int paramIndex = 0; paramIndex < paramUsage.Length; paramIndex++) {
							var valueCode = reader.ReadUInt16();
							var uomCode = reader.ReadUInt16();
							if (valueCode != 0xffff) {
								string paramName = paramUsage[paramIndex].Parameter.Name;
								var uom = EpsgUnit.Get(uomCode);
								paramValues.Add(CreateParameter(valueCode, paramName, uom, readerTxt, numberLookUp));
							}
						}
						lookUpDictionary.Add(opCode, new OpParamValueInfo(opCode, paramValues.ToArray()));
					}
				}
				return new EpsgFixedLookUpBase<ushort, OpParamValueInfo>(lookUpDictionary);
			}

			private readonly ushort _coordinateOperationCode;
			private readonly ParamUsage[] _paramUsage;
			private readonly EpsgLookUpBase<ushort, OpParamValueInfo> _valueLookUp;
			private readonly string _paramDatFileName;

			public EpsgCoordinateOperationMethodParamInfoLookUp(ushort coordinateOperationCode) {
				_coordinateOperationCode = coordinateOperationCode;
				_paramDatFileName = "param" + _coordinateOperationCode + ".dat";
				using (var reader = EpsgDataResource.CreateBinaryReader(_paramDatFileName)) {
					_paramUsage = new ParamUsage[reader.ReadByte()];
					for (int i = 0; i < _paramUsage.Length; i++) {
						var paramInfo = EpsgParameterInfo.Get(reader.ReadUInt16());
						var signRev = 0x01 == reader.ReadByte();
						_paramUsage[i] = new ParamUsage(paramInfo, signRev);
					}
					var opCount = reader.ReadUInt16();
					if (opCount <= FixedLookUpThreshold) {
						_valueLookUp = CreateFullLookUp(reader, opCount, _paramUsage);
					}
					else {
						var opCodes = new ushort[opCount];
						var opSkip = _paramUsage.Length * (sizeof(ushort) * 2);
						for (int i = 0; i < opCodes.Length; i++) {
							opCodes[i] = reader.ReadUInt16();
							reader.BaseStream.Seek(opSkip, SeekOrigin.Current);
						}
						_valueLookUp = new OpParamValueDynamicLookUp(this, opCodes);
					}
				}
			}

			public ReadOnlyCollection<ParamUsage> ParameterUsage { get { return Array.AsReadOnly(_paramUsage); } }

			public OpParamValueInfo GetParameterValueInfo(int operationCode) {
				return operationCode < 0 || operationCode > UInt16.MaxValue
					? null
					: _valueLookUp.Get((ushort) operationCode);
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
			private readonly ReadOnlyCollection<INamedParameter> _values;

			internal OpParamValueInfo(ushort opCode, [NotNull] INamedParameter[] values) {
				_opCode = opCode;
				_values = Array.AsReadOnly(values);
			}

			public int OpCode { get { return _opCode; }}
			public ReadOnlyCollection<INamedParameter> Values { [NotNull] get { return _values; } }
		}

		internal static readonly EpsgCoordinateOperationMethodInfoLookUp LookUp = new EpsgCoordinateOperationMethodInfoLookUp();

		public static EpsgCoordinateOperationMethodInfo Get(int code) {
			return code >= 0 && code <= UInt16.MaxValue
				? LookUp.Get(unchecked((ushort)code))
				: null;
		}

		public static IEnumerable<EpsgCoordinateOperationMethodInfo> Values { get { return LookUp.Values; } }

		private readonly ushort _code;
		private readonly string _name;
		private readonly bool _canReverse;
		private readonly Lazy<EpsgCoordinateOperationMethodParamInfoLookUp> _paramData;

		private EpsgCoordinateOperationMethodInfo(ushort code, string name, bool canReverse)
		{
			_code = code;
			_name = name;
			_canReverse = canReverse;
			_paramData = new Lazy<EpsgCoordinateOperationMethodParamInfoLookUp>(
				CreateParamInfoLookUp, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private EpsgCoordinateOperationMethodParamInfoLookUp CreateParamInfoLookUp() {
			return new EpsgCoordinateOperationMethodParamInfoLookUp(_code);
		}

		public int Code { get { return _code; } }
		public string Name { get { return _name; } }
		public bool CanReverse { get { return _canReverse; } }

		public ReadOnlyCollection<ParamUsage> ParameterUsage { get { return _paramData.Value.ParameterUsage; } }

		public ReadOnlyCollection<INamedParameter> GetOperationParameters(int operationCode) {
			var info = _paramData.Value.GetParameterValueInfo(operationCode);
			return null == info ? null : info.Values;
		}

		public IAuthorityTag Authority { get { return new EpsgAuthorityTag(_code); } }
	}
}
