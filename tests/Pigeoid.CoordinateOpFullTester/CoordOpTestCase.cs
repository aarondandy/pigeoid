using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Core;
using Pigeoid.Interop.Proj4;
using Vertesaur;
using Vertesaur.Periodic;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOpFullTester
{
    public class CoordOpTestCase
    {

        public ICrs From { get; set; }
        public ICrs To { get; set; }

        public IGeographicMbr Area { get; set; }
        public ICoordinateOperationCrsPathInfo Path { get; set; }
        public List<GeographicCoordinate> InputWgs84Coordinates { get; set; }
        public List<object> InputCoordinates { get; set; }

        public void Execute() {
            try {
                var staticCompiler = new StaticCoordinateOperationCompiler();
                var staticTransformation = staticCompiler.Compile(Path);
                if (staticTransformation == null) {
                    Console.WriteLine("TODO: Log static compile failure");
                    return;
                }


                try {
                    var proj4Transform = new Proj4Transform(From, To);
                    ;
                }
                catch (Exception ex) {
                    ;
                }

                var outputPoints = staticTransformation.TransformValues(InputCoordinates).ToList();
            }
            catch (Exception ex) {
                Console.WriteLine("TODO: Log exception")
                ;
            }
        }

        public override string ToString() {
            return String.Format("{0} to {1}", From, To);
        }

    }
}
