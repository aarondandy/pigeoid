using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vertesaur;

namespace Pigeoid.Proj4ComparisonTests
{
    public class CoordinateComparisonStatistics
    {

        public double MinDistance { get; set; }
        public double MaxDistance { get; set; }
        public double AvgDistance { get; set; }

        public double MinErrorRatio { get; set; }
        public double MaxErrorRatio { get; set; }
        public double AvgErrorRatio { get; set; }

        public void Process(object[] setA, object[] setB) {
            if (setA == null) throw new ArgumentNullException("setA");
            if (setB == null) throw new ArgumentNullException("setB");
            if (setA.Length != setB.Length) throw new ArgumentException("Sizes don't match");
            Contract.EndContractBlock();

            double distanceSum = 0;
            MinDistance = Double.NaN;
            MaxDistance = Double.NaN;
            double errorRatioSum = 0;
            MinErrorRatio = Double.NaN;
            MaxErrorRatio = Double.NaN;

            for (int i = 0; i < setA.Length; i++) {
                var sampleA = setA[i];
                var sampleB = setB[i];

                if (sampleA is Point2 && sampleB is Point2) {
                    var ptA = (Point2)sampleA;
                    var ptB = (Point2)sampleB;
                    var distance = ptA.Distance(ptB);
                    distanceSum += distance;
                    if (Double.IsNaN(MinDistance) || MinDistance > distance)
                        MinDistance = distance;
                    if (Double.IsNaN(MaxDistance) || MaxDistance < distance)
                        MaxDistance = distance;

                    var magnitude = ((Vector2)ptA).GetMagnitude();
                    var errorRatio = distance / magnitude;
                    errorRatioSum += errorRatio;
                    if (Double.IsNaN(MinErrorRatio) || MinErrorRatio > errorRatio)
                        MinErrorRatio = errorRatio;
                    if (Double.IsNaN(MaxErrorRatio) || MaxErrorRatio < errorRatio)
                        MaxErrorRatio = errorRatio;
                }
                else {
                    throw new NotImplementedException();
                }
            }

            AvgDistance = distanceSum / setA.Length;
            AvgErrorRatio = errorRatioSum / setA.Length;
        }

    }
}
