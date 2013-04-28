using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Pigeoid.CoordinateOperation;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Unit;

namespace Pigeoid.Ogc
{
    public class WktWriter
    {

        private int _indent;
        private string _indentationText;
        private readonly TextWriter _writer;

        public WktWriter(TextWriter writer, WktOptions options = null) : this(writer, options, 0) {
            Contract.Requires(writer != null);
        }

        internal WktWriter(TextWriter writer, WktOptions options, int initialIndent) {
            if (null == writer) throw new ArgumentNullException("writer");
            Contract.EndContractBlock();

            _writer = writer;
            _indent = initialIndent;
            Options = options ?? new WktOptions();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Options != null);
            Contract.Invariant(_writer != null);
        }

        public WktOptions Options { get; private set; }

        private string FixName(string name) {
            Contract.Ensures(Contract.Result<string>() != null);
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
            if (!String.IsNullOrEmpty(text)) {
                _writer.Write(text);
            }
        }

        public void WriteQuoted(string text) {
            WriteQuote();
            if (!String.IsNullOrEmpty(text)) {
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
            Contract.Ensures(Contract.Result<string>() != null);
            if (n <= 0)
                return String.Empty;

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
            if(entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.Authority);
            WriteOpenParenthesis();
            WriteQuoted(FixName(entity.Name));
            WriteComma();
            WriteQuoted(entity.Code);
            WriteCloseParenthesis();
        }

        public void Write(INamedParameter entity) {
            if(entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.Parameter);
            WriteOpenParenthesis();
            WriteQuoted(FixName(entity.Name).ToLowerInvariant());
            WriteComma();
            WriteValue(entity.Value);
            WriteCloseParenthesis();
        }

        public void Write(ICoordinateOperationInfo entity) {
            if(entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();

            if (entity is IPassThroughCoordinateOperationInfo) {
                Write(entity as IPassThroughCoordinateOperationInfo);
            }
            else if (entity is IConcatenatedCoordinateOperationInfo) {
                Write(entity as IConcatenatedCoordinateOperationInfo);
            }
            else {
                if (entity.IsInverseOfDefinition && entity.HasInverse) {
                    Write(WktKeyword.InverseMt);
                    WriteOpenParenthesis();
                    Indent();
                    WriteIndentedNewLineIfPretty();
                    if (entity.HasInverse) {
                        Write(entity.GetInverse());
                    }
                    else {
                        if(Options.ThrowOnError)
                            throw new Exception("TODO:");
                        WriteQuoted(String.Empty);
                    }
                }
                else {
                    Write(WktKeyword.ParamMt);
                    WriteOpenParenthesis();
                    var parameterizedOperation = entity as IParameterizedCoordinateOperationInfo;
                    if (null != parameterizedOperation) {
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
            if(entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.PassThroughMt);
            WriteOpenParenthesis();
            WriteValue(entity.FirstAffectedOrdinate);
            WriteComma();
            Write(entity.Steps.First());
            WriteCloseParenthesis();
        }

        public void Write(IConcatenatedCoordinateOperationInfo entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.ConcatMt);
            WriteOpenParenthesis();
            Indent();
            WriteIndentedNewLineIfPretty();
            WriteEntityCollection(entity.Steps, Write);
            UnIndent();
            WriteCloseParenthesis();
        }

        public void Write(ISpheroidInfo entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.Spheroid);
            WriteOpenParenthesis();
            Indent();
            WriteQuoted(entity.Name);
            WriteComma();

            // the axis value must be in meters
            var a = entity.A;
            if (entity.AxisUnit != null) {

                var conversion = SimpleUnitConversionGenerator.FindConversion(entity.AxisUnit, OgcLinearUnit.DefaultMeter);
                if (null != conversion) {
                    a = conversion.TransformValue(a);
                }
            }
            WriteValue(a);
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
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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
            if (helmert == null) throw new ArgumentNullException("helmert");
            Contract.EndContractBlock();
            Write(WktKeyword.ToWgs84);
            WriteOpenParenthesis();
            WriteValue(helmert.Delta.X);
            WriteComma();
            WriteValue(helmert.Delta.Y);
            WriteComma();
            WriteValue(helmert.Delta.Z);
            WriteComma();

            WriteValue(helmert.RotationArcSeconds.X);
            WriteComma();
            WriteValue(helmert.RotationArcSeconds.Y);
            WriteComma();
            WriteValue(helmert.RotationArcSeconds.Z);
            WriteComma();
            WriteValue(helmert.ScaleDeltaPartsPerMillion);
            WriteCloseParenthesis();
        }

        public void Write(IDatum entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            var ogcDatumType = Options.ToDatumType(entity.Type);
            if (ogcDatumType == OgcDatumType.LocalOther) {
                WriteBasicDatum(entity, WktKeyword.LocalDatum, ogcDatumType);
            }
            else if (Options.IsVerticalDatum(ogcDatumType)) {
                WriteBasicDatum(entity, WktKeyword.VerticalDatum, ogcDatumType);
            }
            else {
                WriteGeoDatum(entity);
            }
        }

        private void WriteGeoDatum(IDatum entity) {
            Contract.Requires(entity != null);
            var geodeticDatum = entity as IDatumGeodetic;
            Write(WktKeyword.Datum);
            WriteOpenParenthesis();
            WriteQuoted(entity.Name);
            Indent();

            if (null != geodeticDatum) {
                StartNextLineParameter();
                Write(geodeticDatum.Spheroid);

                if (geodeticDatum.IsTransformableToWgs84) {
                    StartNextLineParameter();
                    Write(geodeticDatum.BasicWgs84Transformation);
                }
            }

            if (null != entity.Authority) {
                StartNextLineParameter();
                Write(entity.Authority);
            }

            UnIndent();
            WriteCloseParenthesis();
        }

        private void WriteBasicDatum(IDatum entity, WktKeyword keyword, OgcDatumType ogcDatumType) {
            Contract.Requires(entity != null);
            Write(keyword);
            WriteOpenParenthesis();
            WriteQuoted(entity.Name);
            WriteComma();
            WriteValue((int)ogcDatumType);
            if (null != entity.Authority) {
                WriteComma();
                Write(entity.Authority);
            }
            WriteCloseParenthesis();
        }

        public void Write(IAxis entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.Axis);
            WriteOpenParenthesis();
            WriteQuoted(entity.Name);
            WriteComma();
            WriteRaw(entity.Orientation.ToUpperInvariant());
            WriteCloseParenthesis();
        }

        public void Write(ICrs entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            if (entity is ICrsCompound)
                Write(entity as ICrsCompound);
            else if (entity is ICrsProjected)
                Write(entity as ICrsProjected);
            else if (entity is ICrsGeocentric)
                WriteCrs(entity as ICrsGeocentric, WktKeyword.GeocentricCs);
            else if (entity is ICrsGeographic)
                WriteCrs(entity as ICrsGeographic, WktKeyword.GeographicCs);
            else if (entity is ICrsVertical)
                Write(entity as ICrsVertical);
            else if (entity is ICrsLocal)
                Write(entity as ICrsLocal);
            else if (entity is ICrsFitted)
                Write(entity as ICrsFitted);
            else
                throw new NotSupportedException();
        }

        public void WriteCrs(ICrsGeodetic entity, WktKeyword keyword) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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

            if (null != entity.Unit) {
                StartNextLineParameter();
                Write(entity.Unit);
            }

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

        private void WriteProjection(ICoordinateOperationMethodInfo method) {
            Contract.Requires(method != null);
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
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.VerticalCs);
            WriteOpenParenthesis();
            WriteQuoted(entity.Name);
            Indent();

            StartNextLineParameter();
            Write(entity.Datum);
            StartNextLineParameter();
            Write(entity.Unit);

            if (null != entity.Axis) {
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
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
            Write(WktKeyword.LocalCs);
            WriteOpenParenthesis();
            WriteQuoted(entity.Name);
            Indent();

            StartNextLineParameter();
            Write(entity.Datum);

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

        public void Write(ICrsFitted entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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

        public void Write(IUnit entity) {
            if (entity == null) throw new ArgumentNullException("entity");
            Contract.EndContractBlock();
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

        private double GetReferenceFactor(IUnit entity) {
            Contract.Requires(entity != null);
            IUnit convertTo;
            if (StringComparer.OrdinalIgnoreCase.Equals("Length", entity.Type))
                convertTo = OgcLinearUnit.DefaultMeter;
            else if (StringComparer.OrdinalIgnoreCase.Equals("Angle", entity.Type))
                convertTo = OgcAngularUnit.DefaultRadians;
            else
                return Double.NaN;

            var conversion = SimpleUnitConversionGenerator.FindConversion(entity, convertTo) as IUnitScalarConversion<double>;
            if (null != conversion)
                return conversion.Factor;

            return Double.NaN;
        }

        private void WriteEntityCollection<T>(IEnumerable<T> entities, Action<T> write) {
            Contract.Requires(entities != null);
            Contract.Requires(write != null);
            using (var enumerator = entities.GetEnumerator()) {
                if (!enumerator.MoveNext())
                    return;
                write(enumerator.Current);
                while (enumerator.MoveNext()) {
                    StartNextLineParameter();
                    write(enumerator.Current);
                }
            }
        }

        /// <exception cref="System.NotSupportedException"><paramref name="entity">Entity</paramref> type not supported.</exception>
        public void WriteEntity(object entity) {
            if (null == entity)
                WriteValue(null);
            else if (entity is Helmert7Transformation)
                Write(entity as Helmert7Transformation);
            else if (entity is IAuthorityTag)
                Write(entity as IAuthorityTag);
            else if (entity is INamedParameter)
                Write(entity as INamedParameter);
            else if (entity is ICoordinateOperationInfo)
                Write(entity as ICoordinateOperationInfo);
            else if (entity is ICrs)
                Write(entity as ICrs);
            else if (entity is ISpheroidInfo)
                Write(entity as ISpheroidInfo);
            else if (entity is IPrimeMeridianInfo)
                Write(entity as IPrimeMeridianInfo);
            else if (entity is IUnit)
                Write(entity as IUnit);
            else if (entity is IDatum)
                Write(entity as IDatum);
            else if (entity is IAxis)
                Write(entity as IAxis);
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
