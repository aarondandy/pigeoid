using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pigeoid.Proj4ComparisonTests
{
    public class ConversionResult
    {
        public Exception Exception { get; set; }
        public bool HasError { get { return Exception != null; } }
        public object[] ResultData { get; set; }

    }
}
