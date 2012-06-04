using System;
using System.Collections.Generic;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
	public class EpsgCoordinateOperationInfoRepository
	{

		internal class EpsgCoordinateConversionInfoLookup : EpsgDynamicLookupBase<ushort, EpsgCoordinateOperationInfo>
		{
			private const string DatFileName = "opconv.dat";
			private const string TxtFileName = "op.txt";
			private const int RecordDataSize = (sizeof(ushort) * 3) + sizeof(byte);
			private const int CodeSize = sizeof(ushort);
			private const int RecordSize = CodeSize + RecordDataSize;

			private static ushort[] GetKeys() {
				var keys = new List<ushort>();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add(reader.ReadUInt16());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
				}
				return keys.ToArray();
			}

			public EpsgCoordinateConversionInfoLookup() : base(GetKeys()) { }

			protected override EpsgCoordinateOperationInfo Create(ushort code, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var opMethodCode = reader.ReadUInt16();
					var areaCode = reader.ReadUInt16();
					var deprecated = reader.ReadByte() != 0;
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
					return new EpsgCoordinateOperationInfo(code, opMethodCode, areaCode, deprecated, name);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordinateOperationInfo value) {
				return (ushort)value.Code;
			}

		}

		internal class EpsgCoordinateTransformInfoLookup : EpsgDynamicLookupBase<ushort, EpsgCoordinateTransformInfo>
		{

			private const string DatFileName = "optran.dat";
			private const string TxtFileName = "op.txt";
			private const int RecordDataSize = (sizeof(ushort) * 6) + sizeof(byte);
			private const int CodeSize = sizeof(ushort);
			private const int RecordSize = CodeSize + RecordDataSize;

			private static ushort[] GetKeys() {
				var keys = new List<ushort>();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add(reader.ReadUInt16());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
				}
				return keys.ToArray();
			}

			public EpsgCoordinateTransformInfoLookup() : base(GetKeys()) { }

			protected override EpsgCoordinateTransformInfo Create(ushort code, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName))
				using (var numberLookup = new EpsgNumberLookup()) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var sourceCrsCode = reader.ReadUInt16();
					var targetCrsCode = reader.ReadUInt16();
					var opMethodCode = reader.ReadUInt16();
					var accuracy = numberLookup.Get(reader.ReadUInt16());
					var areaCode = reader.ReadUInt16();
					var deprecated = reader.ReadByte() != 0;
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
					return new EpsgCoordinateTransformInfo(
						code, sourceCrsCode, targetCrsCode, opMethodCode,
						accuracy, areaCode, deprecated, name);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordinateTransformInfo value) {
				return (ushort)value.Code;
			}

		}

		internal class EpsgCoordinateOperationConcatenatedInfoLookup : EpsgDynamicLookupBase<ushort, EpsgCoordinateOperationConcatenatedInfo>
		{
			private const string DatFileName = "opcat.dat";
			private const string PathFileName = "oppath.dat";
			private const string TxtFileName = "op.txt";
			private const int RecordDataSize = (sizeof(ushort) * 5) + (sizeof(byte) * 2);
			private const int CodeSize = sizeof(ushort);
			private const int RecordSize = CodeSize + RecordDataSize;

			private static ushort[] GetKeys() {
				var keys = new List<ushort>();
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					while (reader.BaseStream.Position < reader.BaseStream.Length) {
						keys.Add(reader.ReadUInt16());
						reader.BaseStream.Seek(RecordDataSize, SeekOrigin.Current);
					}
				}
				return keys.ToArray();
			}

			public EpsgCoordinateOperationConcatenatedInfoLookup() : base(GetKeys()) { }

			protected override EpsgCoordinateOperationConcatenatedInfo Create(ushort code, int index) {
				using (var reader = EpsgDataResource.CreateBinaryReader(DatFileName)) {
					reader.BaseStream.Seek((index * RecordSize) + CodeSize, SeekOrigin.Begin);
					var sourceCrsCode = reader.ReadUInt16();
					var targetCrsCode = reader.ReadUInt16();
					var areaCode = reader.ReadUInt16();
					var deprecated = reader.ReadByte() != 0;
					var name = EpsgTextLookup.GetString(reader.ReadUInt16(), TxtFileName);
					var stepCodes = new ushort[reader.ReadByte()];
					var stepFileOffset = reader.ReadUInt16();
					using (var readerPath = EpsgDataResource.CreateBinaryReader(PathFileName)) {
						readerPath.BaseStream.Seek(stepFileOffset, SeekOrigin.Begin);
						for (int i = 0; i < stepCodes.Length; i++) {
							stepCodes[i] = readerPath.ReadUInt16();
						}
					}
					return new EpsgCoordinateOperationConcatenatedInfo(
						code, sourceCrsCode, targetCrsCode, areaCode,
						deprecated, name, stepCodes
					);
				}
			}

			protected override ushort GetKeyForItem(EpsgCoordinateOperationConcatenatedInfo value) {
				return (ushort)value.Code;
			}

		}

		internal static readonly EpsgCoordinateTransformInfoLookup TransformLookup = new EpsgCoordinateTransformInfoLookup();
		internal static readonly EpsgCoordinateConversionInfoLookup ConversionLookup = new EpsgCoordinateConversionInfoLookup();
		internal static readonly EpsgCoordinateOperationConcatenatedInfoLookup ConcatenatedLookup = new EpsgCoordinateOperationConcatenatedInfoLookup();

		public static EpsgCoordinateTransformInfo GetTransformInfo(int code) {
			return code >= 0 && code < UInt16.MaxValue ? TransformLookup.Get((ushort)code) : null;
		}

		public static EpsgCoordinateOperationInfo GetConversionInfo(int code) {
			return code >= 0 && code < UInt16.MaxValue ? ConversionLookup.Get((ushort) code) : null;
		}

		public static EpsgCoordinateOperationConcatenatedInfo GetConcatenatedInfo(int code) {
			return code >= 0 && code < UInt16.MaxValue ? ConcatenatedLookup.Get((ushort) code) : null;
		}

		/// <summary>
		/// Finds either a transformation or conversion for the given code.
		/// </summary>
		/// <param name="code">The code to find.</param>
		/// <returns>The operation for the code.</returns>
		internal static EpsgCoordinateOperationInfo GetOperationInfo(int code) {
			if(code < 0 || code >= UInt16.MaxValue)
				return null;
			var codeShort = (ushort) code;
			return TransformLookup.Get(codeShort)
				?? ConversionLookup.Get(codeShort);
		}

		public static IEnumerable<EpsgCoordinateTransformInfo> TransformInfos { get { return TransformLookup.Values; } }
		public static IEnumerable<EpsgCoordinateOperationInfo> ConversionInfos { get { return ConversionLookup.Values; } }
		public static IEnumerable<EpsgCoordinateOperationConcatenatedInfo> ConcatenatedInfos { get { return ConcatenatedLookup.Values; } }

	}
}
