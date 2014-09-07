using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{

    public class EpsgParameterInfo
    {

        private readonly ushort _code;

        internal EpsgParameterInfo(ushort code, string name) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

    }

}
