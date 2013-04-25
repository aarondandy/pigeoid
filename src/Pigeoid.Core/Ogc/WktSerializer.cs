using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Pigeoid.Ogc
{
    /// <summary>
    /// Used to serialize and deserialize WKT data. 
    /// </summary>
    public class WktSerializer
    {

        public WktSerializer() : this(null) { }

        public WktSerializer(WktOptions options) {
            Options = options ?? new WktOptions();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Options != null);
        }

        [Obsolete("This should have a better name.")]
        public WktOptions Options { get; private set; }

        /// <summary>
        /// Parse WKT entities from the <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A WKT entity.</returns>
        public object Parse(TextReader reader) {
            if(reader == null) throw new ArgumentNullException("reader");
            Contract.EndContractBlock();
            var wktReader = new WktReader(reader, Options);
            return wktReader.MoveNext()
                ? wktReader.ReadEntity()
                : null;
        }

        /// <summary>
        /// Parse WKT entities from the given text string.
        /// </summary>
        /// <param name="wkText">The well known text to parse.</param>
        /// <returns>A WKT entity.</returns>
        public object Parse(string wkText) {
            if(wkText == null) throw new ArgumentNullException("wkText");
            Contract.EndContractBlock();
            using (var sr = new StringReader(wkText)) {
                return Parse(sr);
            }
        }

        public void Serialize(object entity, TextWriter writer) {
            if(writer == null) throw new ArgumentNullException("writer");
            Contract.EndContractBlock();
            (new WktWriter(writer, Options)).WriteEntity(entity);
        }

        public string Serialize(object entity) {
            Contract.Ensures(Contract.Result<string>() != null);
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder)) {
                Serialize(entity, writer);
            }
            return builder.ToString();
        }

    }
}
