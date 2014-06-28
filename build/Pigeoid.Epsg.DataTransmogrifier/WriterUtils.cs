using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public static class WriterUtils
	{

		public static void WriteWordLookUp(EpsgData data, BinaryWriter textWriter, BinaryWriter indexWriter) {

			var roots = new List<TextNode>();
			foreach(var text in data.WordLookUpList) {
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

			var offsetLookUp = new Dictionary<string, int>();
			int rootOffset = 0;
			foreach(var root in roots) {
				var rootText = root.Text;
				var rootBytes = Encoding.UTF8.GetBytes(rootText);
				textWriter.Write(rootBytes);
				foreach(var text in root.GetAllString()) {
					int startIndex = rootText.IndexOf(text, StringComparison.Ordinal);
					var localOffset = Encoding.UTF8.GetByteCount(rootText.Substring(0, startIndex));
					offsetLookUp.Add(text, rootOffset + localOffset);
				}
				rootOffset += rootBytes.Length;
			}

			foreach(var word in data.WordLookUpList) {
				indexWriter.Write((ushort)offsetLookUp[word]);
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

		public static void WriteNumberLookUps(EpsgData data, BinaryWriter writerDouble, BinaryWriter writerInt, BinaryWriter writerShort) {
			foreach (var number in data.NumberLookUpDouble) {
				writerDouble.Write(number);
			}
			foreach (var number in data.NumberLookUpInt) {
				writerInt.Write((int)number);
			}
			foreach (var number in data.NumberLookUpShort) {
				writerShort.Write((short)number);
			}
		}

		private static Dictionary<string, ushort> WriteTextDictionary(EpsgData data, BinaryWriter textWriter, IEnumerable<string> allStrings) {
			var stringLookUp = new Dictionary<string, ushort>();
			int textOffset = 0;
			foreach (var textItem in allStrings) {
				var textBytes = data.GenerateWordIndexBytes(textItem);
				textWriter.Write((byte)textBytes.Length);
				textWriter.Write(textBytes);
				stringLookUp.Add(textItem, (ushort)textOffset);
				textOffset += textBytes.Length + sizeof(byte);
			}
			stringLookUp.Add(String.Empty, ushort.MaxValue);
			return stringLookUp;
		}

		private static short EncodeDegreeValueToShort(double? v) {
			return EncodeDegreeValueToShort(v ?? 0);
		}

		private static short EncodeDegreeValueToShort(double v) {
			// push in at least twice
			v *= 100.0;
			// push up into an integer value
			while(Math.Abs(Math.Round(v) - v) > 0.000001) {
				v *= 10.0;
			}
			return (short)Math.Round(v);
		}

		private static double DecodeDegreeValueFromShort(short encoded) {
			double v = encoded / 100.0;
			while(v < -180 || v > 180) {
				v /= 10.0;
			}
			return v;
		}

		public static void WriteAreas(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter, BinaryWriter iso2Writer, BinaryWriter iso3Writer) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.Areas.Select(x => x.Name)
				//.Concat(data.Areas.Select(x => StringUtils.TrimTrailingPeriod(x.AreaOfUse)))
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			dataWriter.Write((ushort)data.Repository.Areas.Count);
			foreach (var area in data.Repository.Areas.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)area.Code);

				var encodedWestBound = EncodeDegreeValueToShort(area.WestBound);
				var encodedEastBound = EncodeDegreeValueToShort(area.EastBound);
				var encodedSouthBound =EncodeDegreeValueToShort(area.SouthBound);
				var encodedNorthBound =EncodeDegreeValueToShort(area.NorthBound);

// ReSharper disable CompareOfFloatsByEqualityOperator
				if (DecodeDegreeValueFromShort(encodedWestBound) != (area.WestBound ?? 0))
					throw new InvalidOperationException();
				if (DecodeDegreeValueFromShort(encodedEastBound) != (area.EastBound ?? 0))
					throw new InvalidOperationException();
				if (DecodeDegreeValueFromShort(encodedNorthBound) != (area.NorthBound ?? 0))
					throw new InvalidOperationException();
				if (DecodeDegreeValueFromShort(encodedSouthBound) != (area.SouthBound ?? 0))
					throw new InvalidOperationException();
// ReSharper restore CompareOfFloatsByEqualityOperator

				dataWriter.Write(encodedWestBound);
				dataWriter.Write(encodedEastBound);
				dataWriter.Write(encodedSouthBound);
				dataWriter.Write(encodedNorthBound);
				dataWriter.Write((ushort)stringLookUp[area.Name ?? String.Empty]);
				//dataWriter.Write((ushort)stringLookup[StringUtils.TrimTrailingPeriod(area.AreaOfUse) ?? String.Empty]);
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
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.Axes.Select(x => x.Name)
				.Concat(data.Repository.Axes.Select(x => x.Orientation))
				.Concat(data.Repository.Axes.Select(x => x.Abbreviation))
				.Distinct()
			);

			var axesData = data.Repository.Axes.ToLookup(x => x.CoordinateSystem.Code);
			int c = axesData.Count();
			dataWriter.Write((ushort)c);
			foreach (var axisSet in axesData.OrderBy(x => x.Key)) {
				dataWriter.Write((ushort)axisSet.Key);
				dataWriter.Write((byte)axisSet.Count());
				foreach (var axis in axisSet.OrderBy(x => x.OrderValue)) {
					dataWriter.Write((ushort)axis.Uom.Code);
					dataWriter.Write((ushort)stringLookUp[axis.Name]);
					dataWriter.Write((ushort)stringLookUp[axis.Orientation]);
					dataWriter.Write((ushort)stringLookUp[axis.Abbreviation]);
				}
			}
		}

		public static void WriteEllipsoids(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.Ellipsoids.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Repository.Ellipsoids.Count;
			dataWriter.Write((ushort)c);
			foreach (var ellipsoid in data.Repository.Ellipsoids.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)ellipsoid.Code);
				dataWriter.Write((ushort)data.GetNumberIndex(ellipsoid.SemiMajorAxis));
				dataWriter.Write((ushort)data.GetNumberIndex(ellipsoid.InverseFlattening ?? ellipsoid.SemiMinorAxis ?? 0));
				dataWriter.Write((ushort)stringLookUp[ellipsoid.Name]);
				dataWriter.Write((byte)(ellipsoid.Uom.Code - 9000));
			}
		}

		public static void WriteMeridians(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.PrimeMeridians.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Repository.PrimeMeridians.Count;
			dataWriter.Write((ushort)c);
			foreach (var meridian in data.Repository.PrimeMeridians.OrderBy(x => x.Code)) {
				dataWriter.Write((ushort)meridian.Code);
				dataWriter.Write((ushort)meridian.Uom.Code);
				dataWriter.Write((ushort)data.GetNumberIndex(meridian.GreenwichLon));
				dataWriter.Write((byte)stringLookUp[meridian.Name]);
			}
		}

		public static void WriteDatums(EpsgData data, BinaryWriter engineeringWriter, BinaryWriter verticalWriter, BinaryWriter geodeticWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.Datums.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var datum in data.Repository.Datums.OrderBy(x => x.Code)) {
				switch(datum.Type.ToUpper())
				{
					case "ENGINEERING": {
						engineeringWriter.Write((ushort)datum.Code);
						engineeringWriter.Write((ushort)(stringLookUp[datum.Name]));
						engineeringWriter.Write((ushort)datum.AreaOfUse.Code);
						break;
					}
					case "VERTICAL": {
						verticalWriter.Write((ushort)datum.Code);
						verticalWriter.Write((ushort)(stringLookUp[datum.Name]));
						verticalWriter.Write((ushort)datum.AreaOfUse.Code);
						break;
					}
					case "GEODETIC": {
						geodeticWriter.Write((ushort)datum.Code);
						geodeticWriter.Write((ushort)(stringLookUp[datum.Name]));
						geodeticWriter.Write((ushort)datum.AreaOfUse.Code);
						geodeticWriter.Write((ushort)datum.Ellipsoid.Code);
						geodeticWriter.Write((ushort)datum.PrimeMeridian.Code);
						break;
					}
					default: throw new InvalidDataException("Invalid datum type: " + datum.Type);
				}
			}

		}

		public static void WriteUnitOfMeasures(EpsgData data, BinaryWriter lengthWriter, BinaryWriter angleWriter, BinaryWriter scaleWriter, BinaryWriter timeWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
				data.Repository.Uoms.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var unit in data.Repository.Uoms.OrderBy(x => x.Code)) {
				BinaryWriter writer;
				switch (unit.Type.ToUpper()) {
					case "LENGTH": {
						writer = lengthWriter;
						break;
					}
					case "ANGLE": {
						writer = angleWriter;
						break;
					}
					case "SCALE": {
						writer = scaleWriter;
						break;
					}
                    case "TIME": {
                        writer = timeWriter;
                        break;
                    }
					default: throw new InvalidDataException("Invalid uom type: " + unit.Type);
				}
				writer.Write((ushort)unit.Code);
				writer.Write((ushort)stringLookUp[unit.Name]);
				writer.Write((ushort)data.GetNumberIndex(unit.FactorB ?? 0));
				writer.Write((ushort)data.GetNumberIndex(unit.FactorC ?? 0));
			}

		}


		public static void WriteParameters(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.Parameters.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Repository.Parameters.Count;
			writerData.Write((ushort)c);
			foreach (var parameter in data.Repository.Parameters.OrderBy(x => x.Code)) {
				writerData.Write((ushort)parameter.Code);
				writerData.Write((ushort)stringLookUp[parameter.Name]);
			}
		}

		public static void WriteOpMethod(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.CoordinateOperationMethods.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Repository.CoordinateOperationMethods.Count;
			writerData.Write((ushort)c);
			foreach (var opMethod in data.Repository.CoordinateOperationMethods.OrderBy(x => x.Code)) {
				writerData.Write((ushort)opMethod.Code);
				writerData.Write((byte)(opMethod.Reverse ? 'B' : 'U'));
				writerData.Write((ushort)stringLookUp[opMethod.Name]);
			}
		}


		public static void WriteCoordinateSystems(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.CoordinateSystems.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			int c = data.Repository.CoordinateSystems.Count;
			writerData.Write((ushort)c);
			foreach (var cs in data.Repository.CoordinateSystems.OrderBy(x => x.Code)) {

				byte typeByte;
				switch (cs.TypeName.ToLower()) {
					case "cartesian": typeByte = 16; break;
					case "ellipsoidal": typeByte = 32; break;
					case "spherical": typeByte = 48; break;
					case "vertical": typeByte = 64; break;
					default: throw new InvalidDataException();
				}
				var dimByte = (byte)cs.Dimension;
				var typeDimVal = checked((byte)(typeByte | dimByte));
				if (cs.Deprecated)
					typeDimVal |= 128;

				writerData.Write((ushort)cs.Code);
				writerData.Write((byte)typeDimVal);
				writerData.Write((ushort)stringLookUp[cs.Name]);
			}
		}

		public static void WriteParamData(EpsgData data, BinaryWriter writerText, Func<ushort,BinaryWriter> paramFileGenerator) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.ParamValues.Select(x => x.TextValue)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var coordinateOpMethod in data.Repository.CoordinateOperationMethods) {
				var usedBy = coordinateOpMethod.UsedBy.ToList();

				using(var writerData = paramFileGenerator((ushort)coordinateOpMethod.Code)) {
					var paramUses = coordinateOpMethod.ParamUse.OrderBy(x => x.SortOrder).ToList();
					writerData.Write((byte)paramUses.Count);
					foreach (var paramUse in paramUses) {
						writerData.Write((ushort)paramUse.Parameter.Code);
						writerData.Write((byte)(paramUse.SignReversal.GetValueOrDefault() ? 0x01 : 0x02));
					}
					writerData.Write((ushort)usedBy.Count);
					foreach (var coordinateOperation in usedBy.OrderBy(x => x.Code)) {
						writerData.Write((ushort)coordinateOperation.Code);
						var paramValues = coordinateOperation.ParameterValues.ToList();
						foreach (var paramUse in paramUses) {
							var paramCode = paramUse.Parameter.Code;
							var paramValue = paramValues.FirstOrDefault(x => x.Parameter.Code == paramCode);

							// the value
							if(null != paramValue && paramValue.NumericValue.HasValue)
								writerData.Write((ushort)data.GetNumberIndex(paramValue.NumericValue.Value));
							else if(null != paramValue && !String.IsNullOrWhiteSpace(paramValue.TextValue))
								writerData.Write((ushort)(stringLookUp[paramValue.TextValue] | 0x8000));
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

		public static void WriteCoordinateReferenceSystem(EpsgData data, BinaryWriter writerText, BinaryWriter writerProjection, BinaryWriter writerGeo, BinaryWriter writerComposite) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.Crs.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			foreach (var coordinateReferenceSystem in data.Repository.Crs.OrderBy(x => x.Code)) {
				byte kindByte;
				switch (coordinateReferenceSystem.Kind.ToLower()) {
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

				if(kindByte != (byte)'P' && coordinateReferenceSystem.Projection != null) {
					kindByte = (byte)'P';
				}

				if(kindByte == (byte)'P') {
					writerProjection.Write((uint)coordinateReferenceSystem.Code);

					writerProjection.Write((ushort)coordinateReferenceSystem.SourceGeographicCrs.Code);
					writerProjection.Write((ushort)coordinateReferenceSystem.Projection.Code);

					writerProjection.Write((ushort)coordinateReferenceSystem.CoordinateSystem.Code);
					writerProjection.Write((ushort)coordinateReferenceSystem.Area.Code);
					writerProjection.Write((ushort)stringLookUp[coordinateReferenceSystem.Name]);
					writerProjection.Write((byte)(coordinateReferenceSystem.Deprecated ? 0xff : 0));
				}
				else if (kindByte == (byte)'C') {
					writerComposite.Write((ushort)coordinateReferenceSystem.Code);
					
					writerComposite.Write((ushort)coordinateReferenceSystem.CompoundHorizontalCrs.Code);
					writerComposite.Write((ushort)coordinateReferenceSystem.CompoundVerticalCrs.Code);

					writerComposite.Write((ushort)coordinateReferenceSystem.Area.Code);
					writerComposite.Write((ushort)stringLookUp[coordinateReferenceSystem.Name]);
					writerComposite.Write((byte)(coordinateReferenceSystem.Deprecated ? 0xff : 0));
				}
				else {
					writerGeo.Write((uint) coordinateReferenceSystem.Code);

					writerGeo.Write((ushort)(null == coordinateReferenceSystem.Datum ? 0 : coordinateReferenceSystem.Datum.Code));

					writerGeo.Write((ushort)coordinateReferenceSystem.CoordinateSystem.Code);
					writerGeo.Write((ushort)coordinateReferenceSystem.Area.Code);
					writerGeo.Write((ushort)stringLookUp[coordinateReferenceSystem.Name]);
					writerGeo.Write((byte)(coordinateReferenceSystem.Deprecated ? 0xff : 0));
					writerGeo.Write((byte)kindByte);
				}

			}

		}



		public static void WriteCoordinateOperations(EpsgData data, BinaryWriter writerText, BinaryWriter writerDataConversion, BinaryWriter writerDataTransformation, BinaryWriter writerDataConcatenated, BinaryWriter writerDataPath) {
			var stringLookUp = WriteTextDictionary(data, writerText,
				data.Repository.CoordinateOperations.Select(x => x.Name)
				.Where(x => !String.IsNullOrEmpty(x))
				.Distinct()
			);

			var pathOffset = 0;

			foreach (var op in data.Repository.CoordinateOperations.OrderBy(x => x.Code)) {
				var opCode = op.Code;
				switch(op.TypeName.ToLower()) {
					case "transformation": {
						writerDataTransformation.Write((ushort)opCode);
						writerDataTransformation.Write((ushort)op.SourceCrs.Code);
						writerDataTransformation.Write((ushort)op.TargetCrs.Code);
						writerDataTransformation.Write((ushort)op.Method.Code);
						writerDataTransformation.Write((ushort)data.GetNumberIndex(op.Accuracy ?? 0));
						writerDataTransformation.Write((ushort)op.Area.Code);
						writerDataTransformation.Write((byte)(op.Deprecated ? 0xff : 0));
						writerDataTransformation.Write((ushort)stringLookUp[op.Name]);
						break;
					}
					case "conversion": {
						writerDataConversion.Write((ushort)opCode);
						writerDataConversion.Write((ushort)op.Method.Code);
						writerDataConversion.Write((ushort)op.Area.Code);
						writerDataConversion.Write((byte)(op.Deprecated ? 0xff : 0));
						writerDataConversion.Write((ushort)stringLookUp[op.Name]);
						break;
					}
					case "concatenated operation": {
						writerDataConcatenated.Write((ushort)opCode);
						writerDataConcatenated.Write((ushort)op.SourceCrs.Code);
						writerDataConcatenated.Write((ushort)op.TargetCrs.Code);
						writerDataConcatenated.Write((ushort)op.Area.Code);
						writerDataConcatenated.Write((byte)(op.Deprecated ? 0xff : 0));
						writerDataConcatenated.Write((ushort)stringLookUp[op.Name]);
						var catOps = data.Repository.CoordOpPathItems
							.Where(x => x.CatCode == opCode)
							.OrderBy(x => x.Step)
							.ToList();
						writerDataConcatenated.Write((byte)(catOps.Count));
						writerDataConcatenated.Write((ushort)pathOffset);
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
