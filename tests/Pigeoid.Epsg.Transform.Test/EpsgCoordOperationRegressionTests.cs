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
            var from = EpsgMicroDatabase.Default.GetCrs(3079);
            var to = EpsgMicroDatabase.Default.GetCrs(3575);
            var wgs = EpsgMicroDatabase.Default.GetCrs(4326);

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

        [Test]
        public void epsg3140_to_wgs() {
            var crs = EpsgMicroDatabase.Default.GetCrs(3140);
            var wgs = EpsgMicroDatabase.Default.GetCrs(4326);

            // source for coordinates is http://epsg.io/3140/map
            var ptWgs = new GeographicCoordinate(-17.785, 177.97);
            var pt3140 = new Point2(530138.52663372, 821498.68898981); // units in links

            var gen = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = gen.Generate(wgs, crs);
            var compiler = new StaticCoordinateOperationCompiler();
            var txs = paths
                .Select(p => compiler.Compile(p))
                .Where(p => p != null);

            var forward = txs.Single();
            var actualForward = (Point2)forward.TransformValue(ptWgs);
            Assert.AreEqual(pt3140.X, actualForward.X, 30);
            Assert.AreEqual(pt3140.Y, actualForward.Y, 30);

            var reverse = forward.GetInverse();
            var actualReverse = (GeographicCoordinate)reverse.TransformValue(pt3140);
            Assert.AreEqual(ptWgs.Longitude, actualReverse.Longitude, 0.01);
            Assert.AreEqual(ptWgs.Latitude, actualReverse.Latitude, 0.01);
        }

        [Test]
        public void epsg4087_to_wgs() {
            var crs = EpsgMicroDatabase.Default.GetCrs(4087);
            var wgs = EpsgMicroDatabase.Default.GetCrs(4326);

            var ptWgs = new GeographicCoordinate(39, -104);
            var pt4087 = new Point2(-11577227,4341460);

            var gen = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = gen.Generate(wgs, crs);
            var compiler = new StaticCoordinateOperationCompiler();
            var txs = paths.Select(p => compiler.Compile(p)).Where(p => p != null);

            var forward = txs.Single();

            var actualZeroMeters = (Point2)forward.TransformValue(GeographicCoordinate.Zero);
            Assert.AreEqual(0, actualZeroMeters.X);
            Assert.AreEqual(0, actualZeroMeters.Y);

            var actualForward = (Point2)forward.TransformValue(ptWgs);
            Assert.AreEqual(pt4087.X, actualForward.X, 30);
            Assert.AreEqual(pt4087.Y, actualForward.Y, 30);

            var reverse = forward.GetInverse();

            var actualZeroDegrees = (GeographicCoordinate)reverse.TransformValue(Point2.Zero);
            Assert.AreEqual(0, actualZeroDegrees.Longitude);
            Assert.AreEqual(0, actualZeroDegrees.Latitude);

            var actualReverse = (GeographicCoordinate)reverse.TransformValue(pt4087);
            Assert.AreEqual(ptWgs.Longitude, actualReverse.Longitude, 0.01);
            Assert.AreEqual(ptWgs.Latitude, actualReverse.Latitude, 0.01);
        }

        [Test]
        public void epsg3857_to_wgs() {
            var crs = EpsgMicroDatabase.Default.GetCrs(3857);
            var wgs = EpsgMicroDatabase.Default.GetCrs(4326);

            var ptWgs = new GeographicCoordinate(45,10);
            var pt3857 = new Point2(1113194, 5621521);

            var gen = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = gen.Generate(wgs, crs);
            var compiler = new StaticCoordinateOperationCompiler();
            var txs = paths.Select(p => compiler.Compile(p)).Where(p => p != null);

            var forward = txs.Single();

            var actualZeroMeters = (Point2)forward.TransformValue(GeographicCoordinate.Zero);
            Assert.AreEqual(0, actualZeroMeters.X);
            Assert.AreEqual(0, actualZeroMeters.Y, 0.0000001);

            var actualForward = (Point2)forward.TransformValue(ptWgs);
            Assert.AreEqual(pt3857.X, actualForward.X, 30);
            Assert.AreEqual(pt3857.Y, actualForward.Y, 30);

            var reverse = forward.GetInverse();

            var actualZeroDegrees = (GeographicCoordinate)reverse.TransformValue(Point2.Zero);
            Assert.AreEqual(0, actualZeroDegrees.Longitude);
            Assert.AreEqual(0, actualZeroDegrees.Latitude);

            var actualReverse = (GeographicCoordinate)reverse.TransformValue(pt3857);
            Assert.AreEqual(ptWgs.Longitude, actualReverse.Longitude, 0.0001);
            Assert.AreEqual(ptWgs.Latitude, actualReverse.Latitude, 0.0001);
        }

        [Test]
        public void epsg4326_to_5072() {
            var wgs = EpsgMicroDatabase.Default.GetCrs(4326);
            var nad83Albers = EpsgMicroDatabase.Default.GetCrs(5072);
            var gen = new EpsgCrsCoordinateOperationPathGenerator();
            var paths = gen.Generate(wgs, nad83Albers);
            var compiler = new StaticCoordinateOperationCompiler();
            var txs = paths.Select(p => compiler.Compile(p)).Where(p => p != null);
            var forward = txs.Single();

            var transformed = (Point2)forward.TransformValue(new GeographicCoordinate(39, -105));
            Assert.AreEqual(-771063, transformed.X, 1);
            Assert.AreEqual(1811448, transformed.Y, 1);

        }

    }
}
