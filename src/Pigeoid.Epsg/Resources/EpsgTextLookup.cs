using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pigeoid.Epsg.Resources
{
	internal static class EpsgTextLookup
	{

		public static string GetString(ushort stringOffset, string wordPointerFile) {
			using (var reader = EpsgDataResource.CreateBinaryReader(wordPointerFile)) {
				return GetString(stringOffset, reader);
			}
		}

		public static string GetString(ushort stringOffset, BinaryReader reader) {
			reader.BaseStream.Seek(stringOffset, SeekOrigin.Begin);
			var wordIndices = Read7BitArray(reader);
			return BuildWordString(wordIndices);
		}

		private static string BuildWordString(List<int> wordIndices) {
			using(var datReader = EpsgDataResource.CreateBinaryReader("words.dat"))
			using(var txtReader = EpsgDataResource.CreateBinaryReader("words.txt")){
				if (wordIndices.Count == 1) {
					return BuildWordString(wordIndices[0], datReader, txtReader);
				}
				var builder = new StringBuilder();
				foreach (var wordIndex in wordIndices) {
					var word = BuildWordString(wordIndex, datReader, txtReader);
					builder.Append(word);
				}
				return builder.ToString();
			}
		}

		private static string BuildWordString(int wordIndex, BinaryReader datReader, BinaryReader txtReader) {
			datReader.BaseStream.Seek(wordIndex * (sizeof(ushort) + sizeof(byte)), SeekOrigin.Begin);
			var address = datReader.ReadUInt16();
			var stringBytes = new byte[datReader.ReadByte()];
			txtReader.BaseStream.Seek(address, SeekOrigin.Begin);
			txtReader.Read(stringBytes, 0, stringBytes.Length);
			var word = Encoding.UTF8.GetString(stringBytes);
			return word;
		}

		private static List<int> Read7BitArray(BinaryReader reader) {
			var dataCount = reader.ReadByte();
			return Read7BitArray(dataCount, reader);
		}

		private static List<int> Read7BitArray(int length, BinaryReader reader) {
			var result = new List<int>();
			int bytesRead = 0;
			while(bytesRead < length) {
				int wordIndex = 0;
				unchecked {
					int shift = 0;
					byte b;
					do {
						b = reader.ReadByte();
						bytesRead++;
						wordIndex |= (b & 0x7F) << shift;
						shift += 7;
					} while ((b & 0x80) != 0);
				}
				result.Add(wordIndex);
			}
			return result;
		}

		/// <summary>
		/// Performs a binary search on the ISO files for an ISO name code.
		/// </summary>
		/// <param name="searchCode">The object key to look for.</param>
		/// <param name="textPathFile">The resource file to search within.</param>
		/// <param name="textSize">The fixed length of the ISO code to read.</param>
		/// <returns>An ISO name code or <see langword="null"/> if not found.</returns>
		public static string LookupIsoString(ushort searchCode, string textPathFile, int textSize) {
			using (var reader = EpsgDataResource.CreateBinaryReader(textPathFile)) {
				var recordSize = sizeof(ushort) + (textSize * sizeof(byte));
				var byteLength = reader.BaseStream.Length;
				var itemCount = (int)byteLength / recordSize;
				int minIndex = 0;
				int maxIndex = itemCount - 1;
				while(minIndex <= maxIndex) {
					var midIndex = (minIndex + maxIndex) / 2;
					reader.BaseStream.Seek(midIndex*recordSize, SeekOrigin.Begin);
					var code = reader.ReadUInt16();
					if(code < searchCode) {
						minIndex = midIndex + 1;
					}
					else if(code > searchCode) {
						maxIndex = midIndex - 1;
					}
					else {
						var data = new byte[textSize];
						reader.Read(data, 0, data.Length);
						return Encoding.UTF8.GetString(data);
					}
				}
			}
			return null;
		}


	}
}
