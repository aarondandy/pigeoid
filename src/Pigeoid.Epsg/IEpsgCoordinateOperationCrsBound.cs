using Pigeoid.CoordinateOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public interface IEpsgCoordinateOperationCrsBound : ICoordinateOperationInfo
    {

        int SourceCrsCode { get; }
        int TargetCrsCode { get; }
        EpsgCrs SourceCrs { get; }
        EpsgCrs TargetCrs { get; }

    }
}
