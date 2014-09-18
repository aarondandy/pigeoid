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
        private static IEnumerable<string> DistinctNonEmptyStrings(this IEnumerable<string> items) {
            return items.Where(x => !String.IsNullOrEmpty(x)).Distinct();
        }

		public static void WriteAreas(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
            var areas = data.Repository.Areas.ToList();

			var stringLookUp = WriteTextDictionary(data, textWriter,
                areas.Select(x => x.Name).DistinctNonEmptyStrings()
			);

            var isos = new List<EpsgArea>();

            dataWriter.Write((ushort)areas.Count);
            foreach (var area in areas) {
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

                if (String.IsNullOrWhiteSpace(area.Iso2)) {
                    dataWriter.Write((ushort)0);
                }
                else {
                    dataWriter.Write((byte)area.Iso2[0]);
                    dataWriter.Write((byte)area.Iso2[1]);
                }

                if (String.IsNullOrWhiteSpace(area.Iso3)) {
                    dataWriter.Write((ushort)0);
                    dataWriter.Write((byte)0);
                }
                else {
                    dataWriter.Write((byte)area.Iso3[0]);
                    dataWriter.Write((byte)area.Iso3[1]);
                    dataWriter.Write((byte)area.Iso3[2]);
                }

			}

		}

        public static void WriteAxes(EpsgData data, BinaryWriter textWriter, BinaryWriter data1Writer, BinaryWriter data2Writer, BinaryWriter data3Writer) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
                data.Repository.Axes.Select(x => x.NameObject.Name).Distinct()
                .Concat(data.Repository.Axes.Select(x => x.Orientation).Distinct())
                .Concat(data.Repository.Axes.Select(x => x.Abbreviation).Distinct())
                .DistinctNonEmptyStrings()
			);

			var axesData = data.Repository.Axes.ToLookup(x => x.CoordinateSystem.Code);
            
            foreach(var axisFileGroup in axesData.GroupBy(x => x.Count())){
                var axisCount = axisFileGroup.Key;
                BinaryWriter dataWriter;
                if (axisCount == 1)
                    dataWriter = data1Writer;
                else if (axisCount == 2)
                    dataWriter = data2Writer;
                else if (axisCount == 3)
                    dataWriter = data3Writer;
                else
                    throw new InvalidDataException();

                dataWriter.Write((ushort)axisFileGroup.Count());
                foreach (var axisSet in axisFileGroup.OrderBy(x => x.Key)) {
                    dataWriter.Write((ushort)axisSet.Key);
                    foreach (var axis in axisSet.OrderBy(x => x.OrderValue)) {
                        dataWriter.Write((ushort)axis.Uom.Code);
                        dataWriter.Write((ushort)stringLookUp[axis.Name]);
                        dataWriter.Write((ushort)stringLookUp[axis.Orientation]);
                        dataWriter.Write((ushort)stringLookUp[axis.Abbreviation]);
                    }
                }

            }
		}

		public static void WriteEllipsoids(EpsgData data, BinaryWriter dataWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
                data.Repository.Ellipsoids.Select(x => x.Name).DistinctNonEmptyStrings()
			);

			int c = data.Repository.Ellipsoids.Count();
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
                data.Repository.PrimeMeridians.Select(x => x.Name).DistinctNonEmptyStrings()
			);

			int c = data.Repository.PrimeMeridians.Count();
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
                data.Repository.Datums.Select(x => x.Name).DistinctNonEmptyStrings()
			);

            foreach (var datumSet in data.Repository.Datums.GroupBy(x => x.Type)) {
                var datums = datumSet.OrderBy(x => x.Code);
                switch (datumSet.Key.ToUpper()) {
                    case "ENGINEERING":
                        engineeringWriter.Write((ushort)datums.Count());
                        foreach (var datum in datums) {
                            engineeringWriter.Write((ushort)datum.Code);
                            engineeringWriter.Write((ushort)(stringLookUp[datum.Name]));
                            engineeringWriter.Write((ushort)datum.AreaOfUse.Code);
                        }
                        break;
                    case "VERTICAL":
                        verticalWriter.Write((ushort)datums.Count());
                        foreach (var datum in datums) {
                            verticalWriter.Write((ushort)datum.Code);
                            verticalWriter.Write((ushort)(stringLookUp[datum.Name]));
                            verticalWriter.Write((ushort)datum.AreaOfUse.Code);
                        }
                        break;
                    case "GEODETIC":
                        geodeticWriter.Write((ushort)datums.Count());
                        foreach (var datum in datums) {
                            geodeticWriter.Write((ushort)datum.Code);
                            geodeticWriter.Write((ushort)(stringLookUp[datum.Name]));
                            geodeticWriter.Write((ushort)datum.AreaOfUse.Code);
                            geodeticWriter.Write((ushort)datum.Ellipsoid.Code);
                            geodeticWriter.Write((ushort)datum.PrimeMeridian.Code);
                        }
                        break;
                    default: throw new InvalidDataException("Invalid datum type: " + datumSet.Key);
                }
            }

		}

		public static void WriteUnitOfMeasures(EpsgData data, BinaryWriter lengthWriter, BinaryWriter angleWriter, BinaryWriter scaleWriter, BinaryWriter timeWriter, BinaryWriter textWriter) {
			var stringLookUp = WriteTextDictionary(data, textWriter,
                data.Repository.Uoms.Select(x => x.Name).DistinctNonEmptyStrings()
			);

            var uomGroups = data.Repository.Uoms.GroupBy(x => x.Type.ToUpper());
            foreach (var uomGroup in uomGroups) {
                BinaryWriter writer;
                if (uomGroup.Key == "LENGTH")
                    writer = lengthWriter;
                else if (uomGroup.Key == "ANGLE")
                    writer = angleWriter;
                else if (uomGroup.Key == "SCALE")
                    writer = scaleWriter;
                else if (uomGroup.Key == "TIME")
                    writer = timeWriter;
                else
                    throw new InvalidDataException("Invalid uom type: " + uomGroup.Key);

                writer.Write((ushort)uomGroup.Count());
                foreach (var uom in uomGroup.OrderBy(x => x.Code)) {
                    writer.Write((ushort)uom.Code);
                    writer.Write((ushort)stringLookUp[uom.Name]);
                    writer.Write((ushort)data.GetNumberIndex(uom.FactorB ?? 0));
                    writer.Write((ushort)data.GetNumberIndex(uom.FactorC ?? 0));
                }
            }

		}


		public static void WriteParameters(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
                data.Repository.Parameters.Select(x => x.Name).DistinctNonEmptyStrings()
			);

			int c = data.Repository.Parameters.Count();
			writerData.Write((ushort)c);
			foreach (var parameter in data.Repository.Parameters.OrderBy(x => x.Code)) {
				writerData.Write((ushort)parameter.Code);
				writerData.Write((ushort)stringLookUp[parameter.Name]);
			}
		}

		public static void WriteOpMethod(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
                data.Repository.CoordinateOperationMethods.Select(x => x.Name).DistinctNonEmptyStrings()
			);

			int c = data.Repository.CoordinateOperationMethods.Count();
			writerData.Write((ushort)c);
			foreach (var opMethod in data.Repository.CoordinateOperationMethods.OrderBy(x => x.Code)) {
				writerData.Write((ushort)opMethod.Code);
				writerData.Write((byte)(opMethod.Reverse ? 'B' : 'U'));
				writerData.Write((ushort)stringLookUp[opMethod.Name]);
			}
		}


		public static void WriteCoordinateSystems(EpsgData data, BinaryWriter writerData, BinaryWriter writerText) {
			var stringLookUp = WriteTextDictionary(data, writerText,
                data.Repository.CoordinateSystems.Select(x => x.Name).DistinctNonEmptyStrings()
			);

			int c = data.Repository.CoordinateSystems.Count();
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
                data.Repository.ParamTextValues.DistinctNonEmptyStrings()
			);

			foreach (var coordinateOpMethod in data.Repository.CoordinateOperationMethods) {
                var usedBy = coordinateOpMethod.UsedBy.OrderBy(x => x.Code).ToList();

				using(var writerData = paramFileGenerator((ushort)coordinateOpMethod.Code)) {
					var paramUses = coordinateOpMethod.ParamUse.OrderBy(x => x.SortOrder).ToList();
					writerData.Write((byte)paramUses.Count);
					foreach (var paramUse in paramUses) {
						writerData.Write((ushort)paramUse.Parameter.Code);
						writerData.Write((byte)(paramUse.SignReversal.GetValueOrDefault() ? 0x01 : 0x02));
					}
					writerData.Write((ushort)usedBy.Count);
					foreach (var coordinateOperation in usedBy) {
						writerData.Write((ushort)coordinateOperation.Code);
						var paramValues = coordinateOperation.ParameterValues.ToList();
						foreach (var paramUse in paramUses) {
							var paramCode = paramUse.Parameter.Code;
							var paramValue = paramValues.SingleOrDefault(x => x.Parameter.Code == paramCode);

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

        private static byte ToCrsKindByte(string crsKind) {
            switch (crsKind.ToLower()) {
                case "compound":
                    return (byte)'C';
                case "engineering":
                    return (byte)'E';
                case "geocentric":
                    return (byte)'G';
                case "geographic 2d":
                    return (byte)'2';
                case "geographic 3d":
                    return (byte)'3';
                case "projected":
                    return (byte)'P';
                case "vertical":
                    return (byte)'V';
                default: throw new NotSupportedException();
            }
        }

        private static EpsgDatum GetFirstDatum(EpsgCrs crs) {
            while (crs != null) {
                if (crs.Datum != null)
                    return crs.Datum;
                crs = crs.SourceGeographicCrs;
            }
            return null;
        }

		public static void WriteCoordinateReferenceSystem(EpsgData data, BinaryWriter writerText, BinaryWriter writerNormal, BinaryWriter writerComposite) {
			var stringLookUp = WriteTextDictionary(data, writerText,
                data.Repository.Crs.Select(x => x.Name).DistinctNonEmptyStrings()
			);

            foreach (var crsGroup in data.Repository.Crs.Select(x => new { KindByte = ToCrsKindByte (x.Kind), Crs = x}).GroupBy(x => x.KindByte == 'C')) {
                var crsItemCount = crsGroup.Count();
                var crsItems = crsGroup.OrderBy(x => x.Crs.Code);
                var groupIsComposite = crsGroup.Key;
                if (groupIsComposite) {
                    writerComposite.Write((ushort)crsItemCount);
                    foreach (var crs in crsItems.Select(x => x.Crs)) {
                        writerComposite.Write((ushort)crs.Code);
                        writerComposite.Write((ushort)crs.CompoundHorizontalCrs.Code);
                        writerComposite.Write((ushort)crs.CompoundVerticalCrs.Code);
                        writerComposite.Write((ushort)crs.Area.Code);
                        writerComposite.Write((ushort)stringLookUp[crs.Name]);
                        writerComposite.Write((byte)(crs.Deprecated ? 0xff : 0));
                    }
                }
                else {
                    writerNormal.Write((ushort)crsItemCount);
                    foreach (var crsData in crsItems) {
                        var crs = crsData.Crs;
                        writerNormal.Write((uint)crs.Code);
                        var datum = GetFirstDatum(crs);
                        writerNormal.Write((ushort)(null == datum ? 0 : datum.Code));
                        writerNormal.Write((ushort)(crs.SourceGeographicCrs == null ? 0 : crs.SourceGeographicCrs.Code));
                        writerNormal.Write((ushort)(crs.Projection == null ? 0 : crs.Projection.Code));
                        writerNormal.Write((ushort)crs.CoordinateSystem.Code);
                        writerNormal.Write((ushort)crs.Area.Code);
                        writerNormal.Write((ushort)stringLookUp[crs.Name]);
                        writerNormal.Write((byte)(crs.Deprecated ? 0xff : 0));
                        writerNormal.Write((byte)crsData.KindByte);
                    }
                }

			}

		}



		public static void WriteCoordinateOperations(EpsgData data, BinaryWriter writerText, BinaryWriter writerDataConversion, BinaryWriter writerDataTransformation, BinaryWriter writerDataConcatenated, BinaryWriter writerDataPath) {
			var stringLookUp = WriteTextDictionary(data, writerText,
                data.Repository.CoordinateOperations.Select(x => x.Name).DistinctNonEmptyStrings()
			);

            var opGroups = data.Repository.CoordinateOperations.GroupBy(x => x.TypeName.ToLower());

            foreach (var opGroup in opGroups) {
                var ops = opGroup.OrderBy(x => x.Code);
                var typeName = opGroup.Key;
                switch (typeName) {
                    case "transformation": {
                        writerDataTransformation.Write((ushort)ops.Count());
                        foreach (var op in ops) {
                            writerDataTransformation.Write((ushort)op.Code);
                            writerDataTransformation.Write((ushort)op.SourceCrs.Code);
                            writerDataTransformation.Write((ushort)op.TargetCrs.Code);
                            writerDataTransformation.Write((ushort)op.Method.Code);
                            writerDataTransformation.Write((ushort)data.GetNumberIndex(op.Accuracy ?? 0));
                            writerDataTransformation.Write((ushort)op.Area.Code);
                            writerDataTransformation.Write((byte)(op.Deprecated ? 0xff : 0));
                            writerDataTransformation.Write((ushort)stringLookUp[op.Name]);
                        }
                        break;
                    }
                    case "conversion": {
                        writerDataConversion.Write((ushort)ops.Count());
                        foreach (var op in ops) {
                            writerDataConversion.Write((ushort)op.Code);
                            writerDataConversion.Write((ushort)op.Method.Code);
                            writerDataConversion.Write((ushort)op.Area.Code);
                            writerDataConversion.Write((byte)(op.Deprecated ? 0xff : 0));
                            writerDataConversion.Write((ushort)stringLookUp[op.Name]);
                        }
                        break;
                    }
                    case "concatenated operation": {
                        var pathOffset = 0;
                        writerDataConcatenated.Write((ushort)ops.Count());

                        foreach (var op in ops) {
                            var catOps = data.Repository.CoordOpPathItems
                                .Where(x => x.CatCode == op.Code)
                                .OrderBy(x => x.Step)
                                .ToList();
                            foreach (var catOp in catOps) {
                                writerDataPath.Write((ushort)catOp.Operation.Code);
                            }

                            writerDataConcatenated.Write((ushort)op.Code);
                            writerDataConcatenated.Write((ushort)op.SourceCrs.Code);
                            writerDataConcatenated.Write((ushort)op.TargetCrs.Code);
                            writerDataConcatenated.Write((ushort)op.Area.Code);
                            writerDataConcatenated.Write((byte)(op.Deprecated ? 0xff : 0));
                            writerDataConcatenated.Write((ushort)stringLookUp[op.Name]);
                            writerDataConcatenated.Write((byte)(catOps.Count));
                            writerDataConcatenated.Write((ushort)pathOffset);

                            pathOffset += catOps.Count * sizeof(ushort);
                        }
                        break;
                    }
                    default: throw new NotSupportedException();
                }
            }

		}

        internal static void WriteOpPaths(EpsgData data, BinaryWriter writerOpForward, BinaryWriter writerOpReverse, BinaryWriter writerConversionFromBase) {
            {
                var crsBoundOps = data.Repository.CrsBoundCoordinateOperations;

                {
                    var forwardOpMappings = crsBoundOps.ToLookup(x => x.SourceCrs.Code, x => checked((ushort)x.Code));
                    writerOpForward.Write((ushort)forwardOpMappings.Count);
                    var pendingCodes = new List<ushort>();
                    foreach (var map in forwardOpMappings.OrderBy(x => x.Key)) {
                        var codes = map.OrderBy(x => x).ToList();
                        writerOpForward.Write(checked((ushort)map.Key));
                        writerOpForward.Write(checked((ushort)codes.Count));
                        if (codes.Count == 1) {
                            writerOpForward.Write(codes[0]);
                        }
                        else {
                            writerOpForward.Write(checked((ushort)(pendingCodes.Count)));
                            pendingCodes.AddRange(codes);
                        }
                    }
                    foreach (var c in pendingCodes) {
                        writerOpForward.Write(c);
                    }
                }

                {
                    var reverseOpMappings = crsBoundOps.ToLookup(x => x.TargetCrs.Code, x => checked((ushort)x.Code));
                    writerOpReverse.Write((ushort)reverseOpMappings.Count);
                    var pendingCodes = new List<ushort>();
                    foreach (var map in reverseOpMappings.OrderBy(x => x.Key)) {
                        var codes = map.OrderBy(x => x).ToList();
                        writerOpReverse.Write(checked((ushort)map.Key));
                        writerOpReverse.Write(checked((ushort)codes.Count));
                        if (codes.Count == 1) {
                            writerOpReverse.Write(codes[0]);
                        }
                        else {
                            writerOpReverse.Write(checked((ushort)(pendingCodes.Count)));
                            pendingCodes.AddRange(codes);
                        }
                    }
                    foreach (var c in pendingCodes) {
                        writerOpReverse.Write(c);
                    }
                }
            }

            {

                var reverseFromBases = data.Repository.CrsProjected
                    .Where(x => x.SourceGeographicCrs != null)
                    .ToLookup(x => checked((ushort)x.SourceGeographicCrs.Code), x => checked((ushort)x.Code))
                    .Select(x => Tuple.Create(x.Key, x.OrderBy(y => y).ToArray()))
                    .OrderBy(x => x.Item1)
                    .ToList();

                var baseCodes = new List<ushort>();
                writerConversionFromBase.Write((ushort)reverseFromBases.Count);
                foreach (var rev in reverseFromBases) {
                    writerConversionFromBase.Write((ushort)rev.Item1);
                    writerConversionFromBase.Write(checked((ushort)(rev.Item2.Length)));
                    if (rev.Item2.Length == 1) {
                        writerConversionFromBase.Write(rev.Item2[0]);
                    }
                    else {
                        writerConversionFromBase.Write(checked((ushort)baseCodes.Count));
                        baseCodes.AddRange(rev.Item2);
                    }
                }
                foreach (var c in baseCodes) {
                    writerConversionFromBase.Write(c);
                }

            }
        }
    }
}
