using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Pigeoid.Contracts;
using Pigeoid.Transformation;

namespace Pigeoid.Ogc
{
	public class WktWriter
	{

		private int _indent;
		private string _indentationText;
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

		private string FixName(string name) {
			if (name == null)
				return String.Empty;
			if (Options.CorrectNames)
				return name.Replace(' ', '_');
			return name;
		}

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

		public void WriteRaw(string text) {
			if (null != text) {
				_writer.Write(text);
			}
		}

		public void WriteQuoted(string text) {
			WriteQuote();
			if (null != text){
				// TODO: some way to escape quotes within here?
				_writer.Write(text);
			}
			WriteQuote();
		}

		public void WriteComma() {
			_writer.Write(',');
		}

		private void WriteIndentedNewLineIfPretty() {
			if (Options.Pretty) {
				WriteNewline();
				WriteIndentation();
			}
		}

		private void StartNextLineParameter() {
			WriteComma();
			WriteIndentedNewLineIfPretty();
		}

		private static string GenerateTabs(int n) {
			var text = new StringBuilder(n);
			for (int i = n; i > 0; i--)
				text.Append('\t');
			return text.ToString();
		}

		public void WriteIndentation() {
			_writer.Write(_indentationText);
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
				textOut = value is double || value is float
					? String.Format(CultureInfo.InvariantCulture, "{0:r}", value)
					: String.Format(CultureInfo.InvariantCulture, "{0}", value);
			}
			else {
				textOut = value.ToString();
			}

			if (isNumber)
				WriteRaw(textOut);
			else
				WriteQuoted(textOut);
		}

		public void Write(IAuthorityTag entity) {
			Write(WktKeyword.Authority);
			WriteOpenParenthesis();
			WriteQuoted(FixName(entity.Name));
			WriteComma();
			WriteQuoted(entity.Code);
			WriteCloseParenthesis();
		}

		public void Write(INamedParameter entity) {
			Write(WktKeyword.Parameter);
			WriteOpenParenthesis();
			WriteQuoted(FixName(entity.Name).ToLowerInvariant());
			WriteComma();
			WriteValue(entity.Value);
			WriteCloseParenthesis();
		}

		public void Write(ICoordinateOperationInfo entity) {

			if(entity is IPassThroughCoordinateOperationInfo) {
				Write(entity as IPassThroughCoordinateOperationInfo);
			}
			else if (entity is IConcatenatedCoordinateOperationInfo) {
				Write(entity as IConcatenatedCoordinateOperationInfo);
			}
			else {
				if (entity.IsInverseOfDefinition && entity.HasInverse){
					Write(WktKeyword.InverseMt);
					WriteOpenParenthesis();
					Indent();
					WriteIndentedNewLineIfPretty();
					Write(entity.GetInverse());
				}
				else{
					Write(WktKeyword.ParamMt);
					WriteOpenParenthesis();
					var parameterizedOperation = entity as IParameterizedCoordinateOperationInfo;
					if(null != parameterizedOperation){
						var method = parameterizedOperation.Method;
						WriteQuoted(FixName(null == method ? entity.Name : method.Name));
						Indent();
						foreach (var parameter in parameterizedOperation.Parameters) {
							StartNextLineParameter();
							Write(parameter);
						}
					}
					else {
						WriteQuoted(FixName(entity.Name));
					}
				}

				UnIndent();
				WriteCloseParenthesis();
			}
		}

		public void Write(IPassThroughCoordinateOperationInfo entity) {
			Write(WktKeyword.PassThroughMt);
			WriteOpenParenthesis();
			WriteValue(entity.FirstAffectedOrdinate);
			WriteComma();
			Write(entity.Steps.First());
			WriteCloseParenthesis();
		}

		public void Write(IConcatenatedCoordinateOperationInfo entity){
			Write(WktKeyword.ConcatMt);
			WriteOpenParenthesis();
			Indent();
			WriteIndentedNewLineIfPretty();
			WriteEntityCollection(entity.Steps, Write);
			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(ISpheroidInfo entity) {
			Write(WktKeyword.Spheroid);
			WriteOpenParenthesis();
			Indent();
			WriteQuoted(entity.Name);
			WriteComma();
			WriteValue(entity.A);
			WriteComma();
			WriteValue(entity.InvF);
			if (null != entity.Authority) {
				WriteComma();
				Write(entity.Authority);
			}
			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(IPrimeMeridianInfo entity) {
			Write(WktKeyword.PrimeMeridian);
			WriteOpenParenthesis();
			Indent();
			WriteQuoted(entity.Name);
			WriteComma();
			WriteValue(entity.Longitude);
			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				Write(authorityTag);
			}
			UnIndent();
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
			var ogcDatumType = Options.ToDatumType(entity.Type);
			if(Options.IsLocalDatum(ogcDatumType)) {
				WriteBasicDatum(entity, WktKeyword.LocalDatum, ogcDatumType);
			}
			else if(Options.IsVerticalDatum(ogcDatumType)) {
				WriteBasicDatum(entity, WktKeyword.VerticalDatum, ogcDatumType);
			}
			else {
				WriteGeoDatum(entity);
			}
		}

		private void WriteGeoDatum(IDatum entity) {
			var geodeticDatum = entity as IDatumGeodetic;
			Write(WktKeyword.Datum);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			if (null != geodeticDatum) {
				StartNextLineParameter();
				Write(geodeticDatum.Spheroid);

				if(geodeticDatum.IsTransformableToWgs84) {
					StartNextLineParameter();
					Write(geodeticDatum.BasicWgs84Transformation);
				}
			}

			if(null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		private void WriteBasicDatum(IDatum entity, WktKeyword keyword, OgcDatumType ogcDatumType) {
			Write(keyword);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			WriteComma();
			WriteValue((int)ogcDatumType);
			if(null != entity.Authority) {
				WriteComma();
				Write(entity.Authority);
			}
			WriteCloseParenthesis();
		}

		public void Write(IAxis entity) {
			Write(WktKeyword.Axis);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			WriteComma();
			WriteRaw(entity.Orientation.ToUpperInvariant());
			WriteCloseParenthesis();
		}

		public void Write(ICrs entity) {
			if (entity is ICrsCompound)
				Write(entity as ICrsCompound);
			else if (entity is ICrsProjected)
				Write(entity as ICrsProjected);
			else if(entity is ICrsGeocentric)
				WriteCrs(entity as ICrsGeocentric, WktKeyword.GeocentricCs);
			else if(entity is ICrsGeographic)
				WriteCrs(entity as ICrsGeographic, WktKeyword.GeographicCs);
			else if(entity is ICrsVertical)
				Write(entity as ICrsVertical);
			else if(entity is ICrsLocal)
				Write(entity as ICrsLocal);
			else if(entity is ICrsFitted)
				Write(entity as ICrsFitted);
			else
				throw new NotSupportedException();
		}

		public void WriteCrs(ICrsGeodetic entity, WktKeyword keyword) {
			Write(keyword);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.Datum);
			StartNextLineParameter();
			Write(entity.Datum.PrimeMeridian);
			StartNextLineParameter();
			Write(entity.Unit);

			foreach (var axis in entity.Axes) {
				StartNextLineParameter();
				Write(axis);
			}

			if (null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(ICrsProjected entity) {
			Write(WktKeyword.ProjectedCs);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.BaseCrs);

			var parameterizedOperation = entity.Projection as IParameterizedCoordinateOperationInfo;
			if (null != parameterizedOperation) {
				StartNextLineParameter();
				WriteProjection(parameterizedOperation.Method);

				foreach (var parameter in parameterizedOperation.Parameters) {
					StartNextLineParameter();
					Write(parameter);
				}
			}

			if(null != entity.Unit) {
				StartNextLineParameter();
				Write(entity.Unit);
			}

			foreach(var axis in entity.Axes) {
				StartNextLineParameter();
				Write(axis);
			}

			if(null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		private void WriteProjection(ICoordinateOperationMethodInfo method) {
			Write(WktKeyword.Projection);
			WriteOpenParenthesis();
			WriteQuoted(FixName(method.Name));
			if (!Options.SuppressProjectionAuthority && null != method.Authority) {
				WriteComma();
				Write(method.Authority);
			}
			WriteCloseParenthesis();
		}

		public void Write(ICrsVertical entity) {
			Write(WktKeyword.VerticalCs);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.Datum);
			StartNextLineParameter();
			Write(entity.Unit);

			if(null != entity.Axis) {
				StartNextLineParameter();
				Write(entity.Axis);
			}

			if (null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(ICrsLocal entity) {
			Write(WktKeyword.LocalCs);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.Datum);

			StartNextLineParameter();
			Write(entity.Unit);

			foreach(var axis in entity.Axes) {
				StartNextLineParameter();
				Write(axis);
			}

			if(null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(ICrsFitted entity) {
			Write(WktKeyword.FittedCs);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.ToBaseOperation);

			StartNextLineParameter();
			Write(entity.BaseCrs);

			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(ICrsCompound entity) {
			Write(WktKeyword.CompoundCs);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			StartNextLineParameter();
			Write(entity.Head);
			StartNextLineParameter();
			Write(entity.Tail);

			if (null != entity.Authority) {
				StartNextLineParameter();
				Write(entity.Authority);
			}

			UnIndent();
			WriteCloseParenthesis();
		}

		public void Write(IUom entity) {
			Write(WktKeyword.Unit);
			WriteOpenParenthesis();
			WriteQuoted(entity.Name);
			Indent();

			WriteComma();
			WriteValue(GetReferenceFactor(entity));

			var authorityTag = Options.GetAuthorityTag(entity);
			if (null != authorityTag) {
				WriteComma();
				Write(authorityTag);
			}
			
			UnIndent();
			WriteCloseParenthesis();
		}

		private double GetReferenceFactor(IUom entity) {
			IUom convertTo;
			if (StringComparer.OrdinalIgnoreCase.Equals("Length", entity.Type))
				convertTo = OgcLinearUnit.DefaultMeter;
			else if (StringComparer.OrdinalIgnoreCase.Equals("Angle", entity.Type))
				convertTo = OgcAngularUnit.DefaultRadians;
			else
				return Double.NaN;

			var conversion = entity.GetConversionTo(convertTo) as IUomScalarConversion<double>;
			if (null != conversion)
				return conversion.Factor;

			return Double.NaN;
		}

		private void WriteEntityCollection<T>(IEnumerable<T> entities, Action<T> write) {
			using (var enumerator = entities.GetEnumerator()) {
				if (!enumerator.MoveNext())
					return;
				write(enumerator.Current);
				while(enumerator.MoveNext()) {
					StartNextLineParameter();
					write(enumerator.Current);
				}
			}
		}

		public void WriteEntity(object entity) {
			if (null == entity)
				WriteValue(null);
			else if(entity is IAuthorityTag)
				Write(entity as IAuthorityTag);
			else if(entity is INamedParameter)
				Write(entity as INamedParameter);
			else if(entity is ICoordinateOperationInfo)
				Write(entity as ICoordinateOperationInfo);
			else if(entity is ICrs)
				Write(entity as ICrs);
			else if(entity is ISpheroidInfo)
				Write(entity as ISpheroidInfo);
			else if(entity is IPrimeMeridianInfo)
				Write(entity as IPrimeMeridianInfo);
			else if(entity is IUom)
				Write(entity as IUom);
			else if(entity is IDatum)
				Write(entity as IDatum);
			else if(entity is IAxis)
				Write(entity as IAxis);
			else if(entity is Helmert7Transformation)
				Write(entity as Helmert7Transformation);
			else
				throw new NotSupportedException("Entity type not supported.");
		}

		public void Indent() {
			_indentationText = GenerateTabs(Interlocked.Increment(ref _indent));
		}

		public void UnIndent() {
			_indentationText = GenerateTabs(Interlocked.Decrement(ref _indent));
		}


	}
}
