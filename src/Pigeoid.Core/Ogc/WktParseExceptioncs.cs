using System;

namespace Pigeoid.Ogc
{
    public class WktParseExceptioncs : Exception
    {
        public WktParseExceptioncs() { }

        public WktParseExceptioncs(string message) : base(message) { }

        public WktParseExceptioncs(string message, Exception inner) : base(message, inner) { }

        public WktParseExceptioncs(string entityType, string reason)
            : this(String.Format("Failed to parse {0}: {1}", entityType, reason)) { }

    }
}
