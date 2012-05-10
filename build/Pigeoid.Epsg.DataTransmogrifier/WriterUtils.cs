using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class WriterUtils
	{

		public static void WriteWordLookup(EpsgData data, BinaryWriter textWriter, BinaryWriter indexWriter) {
			/*int wordCount = data.WordLookupList.Count;
			textWriter.Write((ushort)wordCount);

			int headerSize = sizeof(ushort) // count
				+ (sizeof(ushort) * wordCount); // lookup keys

			int wordBlockAddress = 0;

			foreach(var word in data.WordLookupList) {
				var currentStreamAddress = headerSize + wordBlockAddress;
				textWriter.Write(checked((ushort)currentStreamAddress));
				wordBlockAddress += Encoding.UTF8.GetByteCount(word);
			}

			foreach(var word in data.WordLookupList) {
				textWriter.Write(Encoding.UTF8.GetBytes(word));
			}*/

			var roots = new List<TextNode>();
			foreach(var text in data.WordLookupList) {
				var containerRoot = TextNode.FindContainingRoot(roots, text);
				if(null == containerRoot) {
					containerRoot = new TextNode(text);
					var containedRoots = roots.Where(r => containerRoot.Contains(r.Text)).ToList();
					foreach(var containedRoot in containedRoots) {
						roots.Remove(containedRoot);
						if(!containerRoot.Add(containedRoot)) {
							throw new InvalidOperationException();
						}
					}
					roots.Add(containerRoot);
				}else {
					if(!containerRoot.Add(text)) {
						throw new InvalidOperationException();
					}
				}
			}

			for (int quality = Math.Min(6,roots.Select(x => x.Text.Length).Max()/2); quality >= 0; quality--) {
				for (int i = 0; i < roots.Count; i++) {
					for (int j = i + 1; j < roots.Count; j++) {
						int overlapAt = StringUtils.OverlapIndex(roots[i].Text, roots[j].Text);
						if (overlapAt >= 0 && (roots[i].Text.Length - overlapAt) >= quality) {
							var newText = roots[i].Text.Substring(0, overlapAt) + roots[j].Text;
							var newNode = new TextNode(newText, new[]{roots[i], roots[j]});
							roots.RemoveAt(j);
							roots[i] = newNode;
							i--;
							break;
						}
						overlapAt = StringUtils.OverlapIndex(roots[j].Text, roots[i].Text);
						if (overlapAt >= 0 && (roots[j].Text.Length - overlapAt) >= quality) {
							var newText = roots[j].Text.Substring(0, overlapAt) + roots[i].Text;
							var newNode = new TextNode(newText, new[]{roots[j], roots[i]});
							roots.RemoveAt(j);
							roots[i] = newNode;
							i--;
							break;
						}
					}
				}
			}

			var offsetLookup = new Dictionary<string, int>();
			int rootOffset = 0;
			foreach(var root in roots) {
				var rootText = root.Text;
				var rootBytes = Encoding.UTF8.GetBytes(rootText);
				textWriter.Write(rootBytes);
				foreach(var text in root.GetAllString()) {
					int startIndex = rootText.IndexOf(text);
					var localOffset = Encoding.UTF8.GetByteCount(rootText.Substring(0, startIndex));
					offsetLookup.Add(text, rootOffset + localOffset);
				}
				rootOffset += rootBytes.Length;
			}

			foreach(var word in data.WordLookupList) {
				indexWriter.Write((ushort)offsetLookup[word]);
				indexWriter.Write((byte)(Encoding.UTF8.GetByteCount(word)));
			}
		}

		private class TextNode
		{

			public static TextNode FindContainingRoot(IEnumerable<TextNode> nodes, string text) {
				foreach(var node in nodes) {
					if(node.Contains(text)) {
						return FindContainingRoot(node.Children, text) ?? node;
					}
				}
				return null;
			}

			public bool Contains(string text) {
				return Text.Contains(text);
			}

			public readonly string Text;

			public readonly List<TextNode> Children;

			public TextNode(string text, IEnumerable<TextNode> children = null) {
				Text = text;
				Children = null == children ? new List<TextNode>() : children.ToList();
			}

			public bool Add(string text) {
				return Add(new TextNode(text));
			}

			public bool Add(TextNode textNode) {
				if(Contains(textNode.Text)) {
					foreach (var child in Children) {
						if (child.Add(textNode)) {
							return true;
						}
					}
					Children.Add(textNode);
					return true;
				}
				return false;
			}

			public IEnumerable<string> GetAllString() {
				yield return Text;
				foreach(var item in Children.SelectMany(c => c.GetAllString())) {
					yield return item;
				}
			}

			public override string ToString() {
				return Text + " (" + Children.Count + ')';
			}
		}

		public static void WriteNumberLookups(EpsgData data, BinaryWriter writerDouble, BinaryWriter writerInt, BinaryWriter writerShort) {
			foreach (var number in data.NumberLookupDouble) {
				writerDouble.Write(number);
			}
			foreach (var number in data.NumberLookupInt) {
				writerInt.Write((int)number);
			}
			foreach (var number in data.NumberLookupShort) {
				writerShort.Write((short)number);
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

		public static void WriteAreas(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter, BinaryWriter iso2Writer, BinaryWriter iso3Writer) {
			var stringLookup = WriteTextDictionary(data, textWriter,
				data.Areas.Select(x => x.Name)
				//.Concat(data.Areas.Select(x => StringUtils.TrimTrailingPeriod(x.AreaOfUse)))
				//.Concat(data.Areas.Select(x => x.Iso2))
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
				dataWriter.Write((short)((area.WestBound ?? 0) * 100));
				dataWriter.Write((short)((area.EastBound ?? 0) * 100));
				dataWriter.Write((short)((area.SouthBound ?? 0) * 100));
				dataWriter.Write((short)((area.NorthBound ?? 0) * 100));
				dataWriter.Write((ushort)stringLookup[area.Name ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[StringUtils.TrimTrailingPeriod(area.AreaOfUse) ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[area.Iso2 ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[area.Iso3 ?? String.Empty]);
				if(!String.IsNullOrWhiteSpace(area.Iso2)) {
					iso2Writer.Write((ushort)area.Code);
					iso2Writer.Write((byte)area.Iso2[0]);
					iso2Writer.Write((byte)area.Iso2[1]);
				}

				if (!String.IsNullOrWhiteSpace(area.Iso3)) {
					iso3Writer.Write((ushort)area.Code);
					iso3Writer.Write((byte)area.Iso3[0]);
					iso3Writer.Write((byte)area.Iso3[1]);
					iso3Writer.Write((byte)area.Iso3[2]);
				}
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
				dataWriter.Write((ushort)data.GetNumberIndex(ellipsoid.SemiMajorAxis));
				dataWriter.Write((ushort)data.GetNumberIndex(ellipsoid.InverseFlattening ?? ellipsoid.SemiMinorAxis ?? 0));
				dataWriter.Write((ushort)stringLookup[ellipsoid.Name]);
				dataWriter.Write((byte)(ellipsoid.Uom.Code - 9000));
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
				dataWriter.Write((ushort)data.GetNumberIndex(meridian.GreenwichLon));
				dataWriter.Write((byte)stringLookup[meridian.Name]);
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
				BinaryWriter writer;
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
				writer.Write((ushort)data.GetNumberIndex(uom.FactorB ?? 0));
				writer.Write((ushort)data.GetNumberIndex(uom.FactorC ?? 0));
			}

		}


		public static void WriteParameters(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.Parameters.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.PrimeMeridians.Count;
			writerData.Write((ushort)c);
			foreach (var parameter in data.Parameters.OrderBy(x => x.Code)) {
				writerData.Write((ushort)parameter.Code);
				writerData.Write((ushort)stringLookup[parameter.Name]);
			}
		}

		public static void WriteOpMethod(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.CoordinateOperationMethods.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.PrimeMeridians.Count;
			writerData.Write((ushort)c);
			foreach (var opMethod in data.CoordinateOperationMethods.OrderBy(x => x.Code)) {
				writerData.Write((ushort)opMethod.Code);
				writerData.Write((byte)(opMethod.Reverse ? 'B' : 'U'));
				writerData.Write((ushort)stringLookup[opMethod.Name]);
			}
		}


		public static void WriteCoordSystems(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.CoordinateSystems.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.PrimeMeridians.Count;
			writerData.Write((ushort)c);
			foreach (var cs in data.CoordinateSystems.OrderBy(x => x.Code)) {

				byte typeByte = 0;
				switch (cs.TypeName.ToLower()) {
					case "cartesian": typeByte = 16; break;
					case "ellipsoidal": typeByte = 32; break;
					case "spherical": typeByte = 48; break;
					case "vertical": typeByte = 64; break;
					default: throw new InvalidDataException();
				}
				byte dimByte = (byte)cs.Dimension;
				byte typeDimVal = checked((byte)(typeByte | dimByte));
				
				writerData.Write((ushort)cs.Code);
				writerData.Write((byte)typeDimVal);
				writerData.Write((ushort)stringLookup[cs.Name]);
			}
		}

		public static void WriteParamData(EpsgData data, BinaryWriter writerText, Func<ushort,BinaryWriter> paramFileGenerator) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.ParamValues.Select(x => x.TextValue)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach(var coordOpMethod in data.CoordinateOperationMethods) {
				if(coordOpMethod.UsedBy.Count <= 0)
					continue;

				using(var writerData = paramFileGenerator((ushort)coordOpMethod.Code)) {
					writerData.Write((byte)coordOpMethod.ParamUse.Count);
					var paramUses = coordOpMethod.ParamUse.OrderBy(x => x.SortOrder).ToList();
					foreach (var paramUse in paramUses) {
						writerData.Write((ushort)paramUse.Parameter.Code);
						writerData.Write((byte)(paramUse.SignReversal.GetValueOrDefault() ? 0x01 : 0x02));
					}
					writerData.Write((ushort)coordOpMethod.UsedBy.Count);
					foreach(var coordOp in coordOpMethod.UsedBy.OrderBy(x => x.Code)) {
						writerData.Write((ushort)coordOp.Code);
						var paramValues = coordOp.ParameterValues.ToList();
						foreach (var paramUse in paramUses) {
							var paramCode = paramUse.Parameter.Code;
							var paramValue = paramValues.FirstOrDefault(x => x.Parameter.Code == paramCode);

							// the value
							if(null != paramValue && paramValue.NumericValue.HasValue)
								writerData.Write((ushort)data.GetNumberIndex(paramValue.NumericValue.Value));
							else if(null != paramValue && !String.IsNullOrWhiteSpace(paramValue.TextValue))
								writerData.Write((ushort)(stringLookup[paramValue.TextValue] | 0x8000));
							else
								writerData.Write((ushort)0xffff);

							// the uom of the value
							if (null != paramValue && null != paramValue.Uom)
								writerData.Write((ushort)paramValue.Uom.Code);
							else
								writerData.Write((ushort)0xffff);
						}
					}

					writerData.Flush();
				}

			}
		}

		public static void WriteCrs(EpsgData data, BinaryWriter writerText, BinaryWriter writerProj, BinaryWriter writerGeo, BinaryWriter writerCmp) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.Crs.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach(var crs in data.Crs.OrderBy(x => x.Code)) {
				byte kindByte;
				switch (crs.Kind.ToLower()) {
					case "compound":
						kindByte = (byte)'C';
						break;
					case "engineering":
						kindByte = (byte)'E';
						break;
					case "geocentric":
						kindByte = (byte)'G';
						break;
					case "geographic 2d":
						kindByte = (byte)'2';
						break;
					case "geographic 3d":
						kindByte = (byte)'3';
						break;
					case "projected":
						kindByte = (byte)'P';
						break;
					case "vertical":
						kindByte = (byte)'V';
						break;
					default: throw new NotSupportedException();
				}

				if(kindByte == (byte)'P') {
					if(crs.Code > 0xffffff)
						throw new InvalidDataException();
					writerProj.Write((byte)(crs.Code & 0xff));
					writerProj.Write((byte)((crs.Code >> 8) & 0xff));
					writerProj.Write((byte)((crs.Code >> 16) & 0xff));

					writerProj.Write((ushort)crs.SourceGeographicCrs.Code);
					writerProj.Write((ushort)crs.Projection.Code);

					writerProj.Write((ushort)crs.CoordinateSystem.Code);
					writerProj.Write((ushort)crs.Area.Code);
					writerProj.Write((ushort)stringLookup[crs.Name]);
					writerProj.Write((byte)(crs.Deprecated ? 0xff : 0));
				}
				else if (kindByte == (byte)'C') {
					writerCmp.Write((ushort)crs.Code);
					
					writerCmp.Write((ushort)crs.CompoundHorizontalCrs.Code);
					writerCmp.Write((ushort)crs.CompoundVerticalCrs.Code);

					writerCmp.Write((ushort)crs.Area.Code);
					writerCmp.Write((ushort)stringLookup[crs.Name]);
					writerCmp.Write((byte)(crs.Deprecated ? 0xff : 0));
				}
				else {
					writerGeo.Write((uint) crs.Code);

					writerGeo.Write((ushort)(null == crs.Datum ? 0 : crs.Datum.Code));

					writerGeo.Write((ushort)crs.CoordinateSystem.Code);
					writerGeo.Write((ushort)crs.Area.Code);
					writerGeo.Write((ushort)stringLookup[crs.Name]);
					writerGeo.Write((byte)(crs.Deprecated ? 0xff : 0));
					writerGeo.Write((byte)kindByte);
				}

			}

		}



		public static void WriteCoordOps(EpsgData data, BinaryWriter writerText, BinaryWriter writerDataConv, BinaryWriter writerDataTran, BinaryWriter writerDataCat, BinaryWriter writerDataPath) {
			var stringLookup = WriteTextDictionary(data, writerText,
				data.CoordinateOperations.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			var pathOffset = 0;

			foreach(var op in data.CoordinateOperations.OrderBy(x => x.Code)) {
				switch(op.TypeName.ToLower()) {
					case "transformation": {
						writerDataTran.Write((ushort)op.Code);
						writerDataTran.Write((ushort)op.SourceCrs.Code);
						writerDataTran.Write((ushort)op.TargetCrs.Code);
						writerDataTran.Write((ushort)op.Method.Code);
						writerDataTran.Write((ushort)data.GetNumberIndex(op.Accuracy ?? 0));
						writerDataTran.Write((ushort)op.Area.Code);
						writerDataTran.Write((byte)(op.Deprecated ? 0xff : 0));
						break;
					}
					case "conversion": {
						writerDataConv.Write((ushort)op.Code);
						writerDataConv.Write((ushort)op.Method.Code);
						writerDataConv.Write((ushort)op.Area.Code);
						writerDataConv.Write((byte)(op.Deprecated ? 0xff : 0));

						break;
					}
					case "concatenated operation": {
						writerDataCat.Write((ushort)op.Code);
						writerDataCat.Write((ushort)op.SourceCrs.Code);
						writerDataCat.Write((ushort)op.TargetCrs.Code);
						writerDataCat.Write((ushort)op.Area.Code);
						writerDataCat.Write((byte)(op.Deprecated ? 0xff : 0));
						var catOps = data.CoordOpPathItems
							.Where(x => x.CatCode == op.Code)
							.OrderBy(x => x.Step)
							.ToList();
						writerDataCat.Write((byte)(catOps.Count));
						writerDataCat.Write((ushort)pathOffset);
						foreach(var catOp in catOps) {
							writerDataPath.Write((ushort)catOp.Operation.Code);
						}
						pathOffset += catOps.Count*sizeof (ushort);
						break;
					}
					default: throw new InvalidDataException();
				}
			}

		}
	}
}
