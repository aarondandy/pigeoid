using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur.Contracts;

namespace Pigeoid.Ogc
{
	public class WktWriter
	{

		private int _indent;
		private readonly TextWriter _writer;

		public WktWriter(TextWriter writer, WktOptions options = null)
			: this(writer, options, 0) { }

		internal WktWriter(TextWriter writer, WktOptions options, int initialIndent) {
			if(null == writer)
				throw new ArgumentNullException("writer");

			_writer = writer;
			_indent = initialIndent;
			Options = options ?? new WktOptions();
		}

		public WktOptions Options { get; private set; }

		public void Write(WktKeyword keyword) {
			_writer.Write(Options.ToStringRepresentation(keyword));
		}

		public void Write(OgcOrientationType orientation) {
			_writer.Write(Options.ToStringRepresentation(orientation));
		}

		public void WriteOpenParenthesis() {
			_writer.Write('[');
		}

		public void WriteCloseParenthesis() {
			_writer.Write(']');
		}

		public void WriteQuote() {
			_writer.Write('\"');
		}

		public void WriteQuoted(string text) {
			WriteQuote();
			if (null != text)
			{
				// TODO: some way to escape quotes within here?
				_writer.Write(text);
			}
			WriteQuote();
		}

		public void WriteComma() {
			_writer.Write(',');
		}

		public void WriteIndentation() {
			for (int i = 0; i < _indent; i++)
				_writer.Write('\t');
		}

		public void WriteNewline() {
			_writer.WriteLine();
		}

		public void WriteValue(object value) {
			value = value ?? String.Empty;

			var isValueType = value.GetType().IsValueType;
			var isNumber = isValueType && !(
				value is bool
				|| value is char
				|| value.GetType().IsEnum
			);
			
			string textOut;
			if (isValueType) {
				if (value is double || value is float) {
					textOut = String.Format(CultureInfo.InvariantCulture, "{0:r}", value);
				}
				else {
					textOut = String.Format(CultureInfo.InvariantCulture, "{0}", value);
				}
			}
			else {
				textOut = value.ToString();
			}

			if (isNumber) {
				_writer.Write(textOut);
			}
			else {
				WriteQuoted(textOut);
			}
		}

		public void Write(IAuthorityTag entity) {
			Write(WktKeyword.Authority);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			WriteComma();
			WriteQuoted(entity.Code);
			WriteCloseParenthesis();
		}

		public void Write(INamedParameter entity) {
			Write(WktKeyword.Parameter);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name.Replace(' ', '_').ToLowerInvariant());
			WriteComma();
			WriteValue(entity.Value);
			WriteCloseParenthesis();
		}

		public void Write(ICoordinateOperationInfo entity) {
			Write(WktKeyword.ParamMt);
			WriteOpenParenthesis();
			WriteQuoted((entity.Name ?? String.Empty).Replace(' ', '_'));
			Indent();
			Write(entity.Parameters);
			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(ISpheroid<double> entity) {
			Write(WktKeyword.Spheroid);
			WriteOpenParenthesis();
			Indent();
			WriteNewline();
			WriteQuoted(Options.GetEntityName(entity));
			WriteComma();
			WriteValue(entity.A);
			WriteComma();
			WriteValue(entity.InvF);
			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				WriteNewline();
				Write(authorityTag);
			}
			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(IPrimeMeridian entity) {
			Write(WktKeyword.PrimeMeridian);
			WriteOpenParenthesis();
			Indent();
			WriteNewline();
			WriteQuoted(Options.GetEntityName(entity));
			WriteComma();
			WriteValue(entity.Longitude);
			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				WriteNewline();
				Write(authorityTag);
			}
			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(Helmert7Transformation helmert) {
			Write(WktKeyword.ToWgs84);
			WriteOpenParenthesis();
			WriteValue(helmert.D.X);
			WriteComma();
			WriteValue(helmert.D.Y);
			WriteComma();
			WriteValue(helmert.D.Z);
			WriteComma();
			WriteValue(helmert.R.X);
			WriteComma();
			WriteValue(helmert.R.Y);
			WriteComma();
			WriteValue(helmert.R.Z);
			WriteComma();
			WriteValue(helmert.Mppm);
			WriteCloseParenthesis();
		}

		public void Write(IDatum entity) {
			var keyword = WktKeyword.Datum;
			ISpheroid<double> spheroid = null;
			Helmert7Transformation toWgs84 = null;
			var ogcDatumType = Options.ToDatumType(entity.Type);

			if (Options.IsLocalDatum(ogcDatumType)) {
				keyword = WktKeyword.LocalDatum;
			}
			else if (Options.IsVerticalDatum(ogcDatumType)) {
				keyword = WktKeyword.VerticalDatum;
			}
			else {
				if (entity is IDatumGeodetic) {
					spheroid = (entity as IDatumGeodetic).Spheroid;
				}

				throw new NotImplementedException();
				/*if (entity is ITransformableToWgs84) {
					
					toWgs84 = (entity as ITransformableToWgs84).PrimaryTransformation;
				}*/
			}
			var authorityTag = Options.GetAuthorityTag(entity);

			Write(keyword);
			WriteOpenParenthesis();
			Indent();
			WriteNewline();
			WriteQuoted(Options.GetEntityName(entity));
			if (keyword != WktKeyword.Datum) {
				WriteComma();
				WriteValue((int)ogcDatumType);
			}

			if (null != spheroid) {
				WriteComma();
				WriteNewline();
				Write(spheroid);
			}
			if (null != toWgs84) {
				WriteComma();
				WriteNewline();
				Write(toWgs84);
			}
			if (null != authorityTag) {
				WriteComma();
				WriteNewline();
				Write(authorityTag);
			}
			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(IAxis entity) {
			Write(WktKeyword.Axis);
			WriteOpenParenthesis();
			WriteQuoted(Options.GetEntityName(entity));
			WriteComma();
			throw new NotImplementedException(); // Write(entity.Orientation); // TODO: How should this be written? Look at old code.
			WriteCloseParenthesis();
		}

		/*public void Write(ICoordinateOperationConcatenated entities) {
			Write(WktKeyword.ConcatMt);
			WriteOpenParenthesis();
			throw new NotImplementedException(); // var items = (entities as IEnumerable<ICoordinateOperation>).ToList();
			if (items.Count > 0) {
				Indent();
				WriteEntityCollection(items, Write);
				UnIndent();
			}
			WriteNewline();
			WriteCloseParenthesis();
		}*/

		public void Write(ITransformation entity) {
			var info = entity as ICoordinateOperationInfo;
			if (null == info && entity.HasInverse) {
				info = entity.GetInverse() as ICoordinateOperationInfo;
				if (null != info) {
					Write(WktKeyword.InverseMt);
					WriteOpenParenthesis();
					Indent();
					WriteNewline();
					Write(info);
					UnIndent();
					WriteNewline();
					WriteCloseParenthesis();
					return;
				}
			}
			if (null != info) {
				Write(info);
				return;
			}
			var name = Options.GetEntityName(entity) ?? entity.GetType().Name;
			throw new NotImplementedException(); // Write(new OgcCoordinateOperationInfo(name, null) as ICoordinateOperationInfo);
		}

		public void Write(ICrs entity) {
			var keyword = WktKeyword.Invalid;
			IDatum datum = null;
			IPrimeMeridian primeMeridian = null;
			IUom unit = null;
			IEnumerable<IAxis> axes = null;
			IEnumerable<ICrs> coordinateReferenceSystems = null;
			ICoordinateOperationInfo projection = null;
			ITransformation coordinateOperation = null;
			if (entity is ICrsGeographic) {
				var geographic = entity as ICrsGeographic;
				keyword = WktKeyword.GeographicCs;
				datum = geographic.Datum;
				primeMeridian = geographic.Datum.PrimeMeridian;
				unit = geographic.Unit;
				axes = geographic.Axes;
			}
			else if (entity is ICrsGeocentric) {
				var geocentric = entity as ICrsGeocentric;
				keyword = WktKeyword.GeocentricCs;
				datum = geocentric.Datum;
				primeMeridian = geocentric.Datum.PrimeMeridian;
				unit = geocentric.Unit;
				axes = geocentric.Axes;
			}
			else if (entity is ICrsProjected) {
				var projected = entity as ICrsProjected;
				keyword = WktKeyword.ProjectedCs;
				unit = projected.Unit;
				axes = projected.Axes;
				coordinateReferenceSystems = new ICrs[] { projected.BaseCrs };
				projection = projected.Projection;
			}
			else if (entity is ICrsVertical) {
				var vertical = entity as ICrsVertical;
				keyword = WktKeyword.VerticalCs;
				datum = vertical.Datum;
				unit = vertical.Unit;
				axes = new[] { vertical.Axis };
			}
			else if (entity is ICrsLocal) {
				var local = entity as ICrsLocal;
				keyword = WktKeyword.LocalCs;
				datum = local.Datum;
				unit = local.Unit;
				axes = local.Axes;
			}
			else if (entity is ICrsFitted) {
				var fitted = entity as ICrsFitted;
				keyword = WktKeyword.FittedCs;
				coordinateOperation = fitted.ToBaseOperation;
				coordinateReferenceSystems = new[] { fitted.BaseCrs };
			}
			else if (entity is ICrsCompound) {
				var compound = entity as ICrsCompound;
				keyword = WktKeyword.CompoundCs;
				coordinateReferenceSystems = compound.CrsComponents;
			}
			if (WktKeyword.Invalid == keyword) {
				return; // TODO: throw?
			}

			Write(keyword);
			WriteOpenParenthesis();
			Indent();
			WriteNewline();
			WriteQuoted(Options.GetEntityName(entity));

			if (null != coordinateOperation) {
				WriteComma();
				WriteNewline();
				Write(coordinateOperation);
			}

			if (null != coordinateReferenceSystems) {
				foreach (var crs in coordinateReferenceSystems) {
					WriteComma();
					WriteNewline();
					Write(crs);
				}
			}

			if (null != projection) {
				WriteComma();
				WriteNewline();
				Write(WktKeyword.Projection);
				WriteOpenParenthesis();
				WriteQuoted((Options.GetEntityName(projection) ?? String.Empty).Replace(' ', '_'));
				var projectionAuthorityTag = Options.GetAuthorityTag(projection);
				if (null != projectionAuthorityTag) {
					Indent();
					WriteComma();
					WriteNewline();
					Write(projectionAuthorityTag);
					WriteNewline();
					UnIndent();
				}
				WriteCloseParenthesis();
				IEnumerable<INamedParameter> namedParams;
				if (projection is ICoordinateOperationInfo) {
					namedParams = (projection as ICoordinateOperationInfo).Parameters;
				}
				else if (projection is IEnumerable<INamedParameter>) {
					namedParams = projection as IEnumerable<INamedParameter>;
				}
				else {
					namedParams = null;
				}
				if (null != namedParams) {
					foreach (var namedParam in namedParams) {
						WriteComma();
						WriteNewline();
						Write(namedParam);
					}
				}
			}
			if (null != datum) {
				WriteComma();
				WriteNewline();
				Write(datum);
				if (null != primeMeridian) {
					WriteComma();
					WriteNewline();
					Write(primeMeridian);
				}
			}
			if (null != unit) {
				WriteComma();
				WriteNewline();
				Write(unit);
			}

			if (null != axes) {
				foreach (var axis in axes) {
					WriteComma();
					WriteNewline();
					Write(axis);
				}
			}

			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				WriteNewline();
				Write(authorityTag);
			}

			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(IUom entity) {
			Write(WktKeyword.Unit);
			WriteOpenParenthesis();
			Indent();
			WriteNewline();

			WriteQuoted(entity.Name);
			WriteComma();

			throw new NotImplementedException(); // WriteValue(GetReferenceFactor(entity));
			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				WriteNewline();
				Write(authorityTag);
			}
			UnIndent();
			WriteNewline();
			WriteCloseParenthesis();
		}

		public void Write(IEnumerable<INamedParameter> entities) {
			WriteEntityCollection(entities, Write);
		}

		private void WriteEntityCollection<T>(IEnumerable<T> entities, Action<T> write) {
			using (var enumerator = entities.GetEnumerator()) {
				if (!enumerator.MoveNext())
					return;
				write(enumerator.Current);
				while(enumerator.MoveNext()) {
					WriteComma();
					WriteNewline();
					write(enumerator.Current);
				}
			}
		}

		public void Indent() {
			_indent++;
		}

		public void UnIndent() {
			_indent--;
		}

	}
}
