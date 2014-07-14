using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public enum EpsgCrsKind
    {
        Unknown = 0,
        Projected = (byte)'P',
        Geographic2D = (byte)'2',
        Geographic3D = (byte)'3',
        Geocentric = (byte)'G',
        Vertical = (byte)'V',
        Compound = (byte)'C',
        Engineering = (byte)'E'
    }
}
