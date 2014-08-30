using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public struct EpsgParameterUsage
    {

        internal EpsgParameterUsage(EpsgParameterInfo parameterInfo, bool signReversal) {
            Contract.Requires(parameterInfo != null);
            _parameterInfo = parameterInfo;
            _signReversal = signReversal;
        }

        private readonly EpsgParameterInfo _parameterInfo;

        private readonly bool _signReversal;

        public EpsgParameterInfo ParameterInfo { get { return _parameterInfo; } }

        public bool SignReversal { get { return _signReversal; } }

    }
}
