// TODO: source header

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
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

			private const string ParamTextValueFileName = "params.txt";

			private readonly ushort _coordOpCode;
			private readonly ReadOnlyCollection<ParamUsage> _paramUsage;

			public EpsgCoordOpMethodParamInfoLookup(ushort coordOpCode) {
				_coordOpCode = coordOpCode;
				using (var reader = EpsgDataResource.CreateBinaryReader(ParamDataFileName)) {
					var paramUsage = new ParamUsage[reader.ReadByte()];
					for (int i = 0; i < paramUsage.Length; i++) {
						var paramInfo = EpsgParameterInfo.Get(reader.ReadUInt16());
						var signRev = 0x01 == reader.ReadByte();
						paramUsage[i] = new ParamUsage(paramInfo, signRev);
					}
					_paramUsage = Array.AsReadOnly(paramUsage);
				}
			}

			private string ParamDataFileName { get { return "param" + _coordOpCode + ".dat"; } }

			public ReadOnlyCollection<ParamUsage> ParameterUsage { get { return _paramUsage; } }

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

	}
}
