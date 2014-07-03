using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Interop
{
    public class SpheroidNameNormalizedComparer : NameNormalizedComparerBase
    {

        public static readonly SpheroidNameNormalizedComparer Default = new SpheroidNameNormalizedComparer();

        public SpheroidNameNormalizedComparer() : this(null) { }

        public SpheroidNameNormalizedComparer(StringComparer comparer) : base(comparer) { }


    }
}
