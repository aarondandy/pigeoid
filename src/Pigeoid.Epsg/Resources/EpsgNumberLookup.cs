using System;
using System.IO;

namespace Pigeoid.Epsg.Resources
{
	internal class EpsgNumberLookUp : IDisposable
	{

		public static double ReadNumber(ushort index, BinaryReader reader) {
			reader.BaseStream.Seek(index * sizeof(double), SeekOrigin.Begin);
			return reader.ReadDouble();
		}

		private readonly BinaryReader _doubleReader;
		private readonly BinaryReader _intReader;
		private readonly BinaryReader _shortReader;

		public EpsgNumberLookUp() {
			_doubleReader = EpsgDataResource.CreateBinaryReader("numbersd.dat");
			_intReader = EpsgDataResource.CreateBinaryReader("numbersi.dat");
			_shortReader = EpsgDataResource.CreateBinaryReader("numberss.dat");
		}

		public double Get(ushort code) {
			var index = (ushort) (code & 0x3fff);
			if ((code & 0xc000) == 0xc000)
				return GetFromInt(index);
			if ((code & 0x4000) == 0x4000)
				return GetFromShort(index);
			return GetFromDouble(index);
		}

		public double GetFromDouble(ushort code) {
			_doubleReader.BaseStream.Seek(code * sizeof(double), SeekOrigin.Begin);
			return _doubleReader.ReadDouble();
		}

		public double GetFromInt(ushort code) {
			_intReader.BaseStream.Seek(code * sizeof(int), SeekOrigin.Begin);
			return _intReader.ReadInt32();
		}

		public double GetFromShort(ushort code) {
			_shortReader.BaseStream.Seek(code * sizeof(short), SeekOrigin.Begin);
			return _shortReader.ReadInt16();
		}

		public void Dispose() {
			_doubleReader.Dispose();
			_intReader.Dispose();
			_shortReader.Dispose();
		}

	}
}
