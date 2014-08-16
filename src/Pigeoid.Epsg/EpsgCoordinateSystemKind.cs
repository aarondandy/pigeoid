using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Epsg
{
    public enum EpsgCoordinateSystemKind : byte
    {
        None = 0,
        Cartesian = 1,
        Ellipsoidal = 2,
        Spherical = 3,
        Vertical = 4
    }
}
