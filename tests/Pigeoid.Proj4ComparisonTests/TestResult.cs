using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeoid.Proj4ComparisonTests
{
    public class TestResult
    {

        public ICrs Source { get; set; }
        public ICrs Target { get; set; }

        public CoordinateComparisonStatistics StatsData { get; set; }

    }
}
