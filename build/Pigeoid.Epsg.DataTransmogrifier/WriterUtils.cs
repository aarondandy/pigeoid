using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class WriterUtils
	{

		public static void WriteWordLookup(EpsgData data, BinaryWriter writer) {
			int wordCount = data.WordLookupList.Count;
			writer.Write((ushort)wordCount);

			int headerSize = sizeof(ushort) // count
				+ (sizeof(ushort) * wordCount); // lookup keys

			int wordBlockAddress = 0;

			foreach(var word in data.WordLookupList) {
				var currentStreamAddress = headerSize + wordBlockAddress;
				writer.Write(checked((ushort)currentStreamAddress));
				wordBlockAddress += Encoding.UTF8.GetByteCount(word);
			}

			foreach(var word in data.WordLookupList) {
				writer.Write(Encoding.UTF8.GetBytes(word));
			}
		}

		public static void WriteNumberLookup(EpsgData data, BinaryWriter writer) {
			int numberCount = data.NumberLookupList.Count;
			writer.Write((ushort)numberCount);

			foreach (var number in data.NumberLookupList) {
				writer.Write(number);
			}
		}

		private static Dictionary<string, ushort> WriteTextDictionary(EpsgData data, BinaryWriter textWriter, IEnumerable<string> allStrings) {
			var stringLookup = new Dictionary<string, ushort>();
			int textOffset = 0;

			foreach (var textItem in allStrings) {
				var textBytes = data.GenerateWordIndexBytes(textItem);
				textWriter.Write((byte)textBytes.Length);
				textWriter.Write(textBytes);
				stringLookup.Add(textItem, (ushort)textOffset);
				textOffset += textBytes.Length;
			}
			stringLookup.Add(String.Empty, ushort.MaxValue);
			return stringLookup;
		}

		public static void WriteAreas(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Areas.Select(x => x.Name)
				//.Concat(data.Areas.Select(x => StringUtils.TrimTrailingPeriod(x.AreaOfUse)))
				.Concat(data.Areas.Select(x => x.Iso2))
				//.Concat(data.Areas.Select(x => x.Iso3))
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			dataWriter.Write((ushort)data.Areas.Count);
			foreach (var area in data.Areas.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)area.Code);
				/*dataWriter.Write((short)((area.WestBound ?? 0) * 100));
				dataWriter.Write((short)((area.EastBound ?? 0) * 100));
				dataWriter.Write((short)((area.SouthBound ?? 0) * 100));
				dataWriter.Write((short)((area.NorthBound ?? 0) * 100));*/
				dataWriter.Write((ushort)(data.NumberLookupList.IndexOf(area.WestBound ?? 0)));
				dataWriter.Write((ushort)(data.NumberLookupList.IndexOf(area.EastBound ?? 0)));
				dataWriter.Write((ushort)(data.NumberLookupList.IndexOf(area.SouthBound ?? 0)));
				dataWriter.Write((ushort)(data.NumberLookupList.IndexOf(area.NorthBound ?? 0)));
				dataWriter.Write((ushort)stringLookup[area.Name ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[StringUtils.TrimTrailingPeriod(area.AreaOfUse) ?? String.Empty]);
				dataWriter.Write((ushort)stringLookup[area.Iso2 ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[area.Iso3 ?? String.Empty]);
			}

		}

		public static void WriteAxes(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Axes.Select(x => x.Name)
				.Concat(data.Axes.Select(x => x.Orientation))
				.Concat(data.Axes.Select(x => x.Abbreviation))
				.Distinct()
			);

			var axesData = data.Axes.OrderBy(x => x.OrderValue).ToLookup(x => x.CoordinateSystem.Code);
			int c = axesData.Count();
			dataWriter.Write((ushort)c);
			foreach (var axisSet in axesData) {
				dataWriter.Write((ushort)axisSet.Key);
				dataWriter.Write((byte)axisSet.Count());
				foreach (var axis in axisSet) {
					dataWriter.Write((ushort)axis.Uom.Code);
					dataWriter.Write((ushort)stringLookup[axis.Name]);
					dataWriter.Write((ushort)stringLookup[axis.Orientation]);
					dataWriter.Write((ushort)stringLookup[axis.Abbreviation]);
				}
			}
		}

		public static void WriteEllipsoids(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Ellipsoids.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Ellipsoids.Count;
			dataWriter.Write((ushort)c);
			foreach (var ellipsoid in data.Ellipsoids.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)ellipsoid.Code);
				dataWriter.Write(data.NumberLookupList.IndexOf(ellipsoid.SemiMajorAxis));
				dataWriter.Write(data.NumberLookupList.IndexOf(ellipsoid.InverseFlattening ?? ellipsoid.SemiMinorAxis ?? 0));
				dataWriter.Write((byte)(ellipsoid.Uom.Code - 9000));
				dataWriter.Write((ushort)stringLookup[ellipsoid.Name]);
			}
		}

		public static void WriteMeridians(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.PrimeMeridians.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.PrimeMeridians.Count;
			dataWriter.Write((ushort)c);
			foreach (var meridian in data.PrimeMeridians.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)meridian.Code);
				dataWriter.Write((ushort)meridian.Uom.Code);
				dataWriter.Write(data.NumberLookupList.IndexOf(meridian.GreenwichLon));
				dataWriter.Write(stringLookup[meridian.Name]);
			}
		}

		public static void WriteDatums(EpsgData data, BinaryWriter egrWriter, BinaryWriter verWriter, BinaryWriter geoWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Datums.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var datum in data.Datums.OrderBy(x => x.Code)) {
				switch(datum.Type.ToUpper())
				{
					case "ENGINEERING": {
						egrWriter.Write((ushort)datum.Code);
						egrWriter.Write((ushort)(stringLookup[datum.Name]));
						break;
					}
					case "VERTICAL": {
						verWriter.Write((ushort)datum.Code);
						verWriter.Write((ushort)(stringLookup[datum.Name]));
						break;
					}
					case "GEODETIC": {
						geoWriter.Write((ushort)datum.Code);
						geoWriter.Write((ushort)(stringLookup[datum.Name]));
						geoWriter.Write((ushort)datum.Ellipsoid.Code);
						geoWriter.Write((ushort)datum.PrimeMeridian.Code);
						break;
					}
					default: throw new InvalidDataException("Invalid datum type: " + datum.Type);
				}
			}

		}

		public static void WriteUoms(EpsgData data, BinaryWriter lenWriter, BinaryWriter angWriter, BinaryWriter sclWriter, BinaryWriter textWriter) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Uoms.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var uom in data.Uoms.OrderBy(x => x.Code)) {
				BinaryWriter writer = null;
				switch (uom.Type.ToUpper()) {
					case "LENGTH": {
						writer = lenWriter;
						break;
					}
					case "ANGLE": {
						writer = angWriter;
						break;
					}
					case "SCALE": {
						writer = sclWriter;
						break;
					}
					default: throw new InvalidDataException("Invalid uom type: " + uom.Type);
				}
				writer.Write((ushort)uom.Code);
				writer.Write((ushort)stringLookup[uom.Name]);
				writer.Write((ushort)data.NumberLookupList.IndexOf(uom.FactorB ?? 0));
				writer.Write((ushort)data.NumberLookupList.IndexOf(uom.FactorC ?? 0));
			}

		}

	}
}
