using NUnit.Framework;
using Pigeoid.CoordinateOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vertesaur;

namespace Pigeoid.Epsg.Transform.Test
{
    [TestFixture]
    public class EpsgCoordOperationRegressionTests
    {

        [Test]
        public void epsg3079_to_epsg3575() {
            var from = EpsgCrs.Get(3079);
            var to = EpsgCrs.Get(3575);
            var wgs = EpsgCrs.Get(4326);

            var somePlaceInMichigan = new GeographicCoordinate(40.4, -91.8);
            var expected3079 = new Point2(6992.885640195105, -644.956855237484);
            var expected3575 = new Point2(-5244224.354585549, 1095575.5476152631);

            var gen = new EpsgCrsCoordinateOperationPathGenerator();
            var fromPath = gen.Generate(from, wgs).First();
            var toPath = gen.Generate(to, wgs).First();

            var compiler = new StaticCoordinateOperationCompiler();

            var fromTx = compiler.Compile(fromPath);
            var toTx = compiler.Compile(toPath);

            var a = (GeographicCoordinate)fromTx.TransformValue(expected3079);
            Assert.AreEqual(somePlaceInMichigan.Latitude, a.Latitude, 0.01);
            Assert.AreEqual(somePlaceInMichigan.Longitude, a.Longitude, 0.01);
            var b = (Point2)fromTx.GetInverse().TransformValue(somePlaceInMichigan);
            Assert.AreEqual(expected3079.X, b.X, 0.01);
            Assert.AreEqual(expected3079.Y, b.Y, 0.01);
            var d = (Point2)toTx.GetInverse().TransformValue(somePlaceInMichigan);
            Assert.AreEqual(expected3575.X, d.X, 0.01);
            Assert.AreEqual(expected3575.Y, d.Y, 0.01);
            var c = (GeographicCoordinate)toTx.TransformValue(expected3575);
            Assert.AreEqual(somePlaceInMichigan.Latitude, c.Latitude, 0.01);
            Assert.AreEqual(somePlaceInMichigan.Longitude, c.Longitude, 0.01);

            var allTotalPaths = gen.Generate(from, to).ToList();
            var totalPath = allTotalPaths.First();
            var totalTx = compiler.Compile(totalPath);
            var actual3575 = (Point2)totalTx.TransformValue(expected3079);

            Assert.AreEqual(expected3575.X, actual3575.X, 1);
            Assert.AreEqual(expected3575.Y, actual3575.Y, 1);
        }

    }
}
