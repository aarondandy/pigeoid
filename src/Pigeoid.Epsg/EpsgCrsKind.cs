using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    [Flags]
    public enum EpsgCrsKind
    {
        Unknown = 0,

        DatumBased = 0x01,
        Geodetic = 0x02 | DatumBased,

        Projected = 0x04,
        Geographic = 0x08 | Geodetic,
        Geocentric = 0x10 | Geodetic,

        Vertical = 0x20 | DatumBased,
        Engineering = 0x40 | DatumBased,
        Compound = 0x80
    }
}
