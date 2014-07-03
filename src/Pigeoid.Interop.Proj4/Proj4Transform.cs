using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DotSpatial.Projections;
using Vertesaur.Transformation;
using Vertesaur;

namespace Pigeoid.Interop.Proj4
{

    public class Proj4Transform : ITransformation
    {

        public Proj4Transform(ICrs source, ICrs target) {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            Contract.EndContractBlock();
            Source = source;
            Target = target;
            SourceProj4 = Proj4Crs.CreateProjection(source);
            TargetProj4 = Proj4Crs.CreateProjection(target);
        }

        public ICrs Source { get; private set; }

        public ICrs Target { get; private set; }

        public ProjectionInfo SourceProj4 { get; private set; }

        public ProjectionInfo TargetProj4 { get; private set; }


        public Type[] GetInputTypes() {
            // TODO: redo
            return new[] {
                typeof(double[])
            };
        }

        public Type[] GetOutputTypes(Type inputType) {
            if (inputType == typeof(double[]))
                return new[] { inputType };
            return new Type[0];
        }

        public ITransformation GetInverse() {
            throw new NotImplementedException();
        }

        public bool HasInverse {
            get { throw new NotImplementedException(); }
        }

        public object TransformValue(object value) {
            if (value is Point2) {
                var p2 = (Point2)value;
                var xy = new double[] { p2.X, p2.Y };
                var z = new double[] { 1 };
                Reproject.ReprojectPoints(
                    xy,
                    z,
                    SourceProj4,
                    TargetProj4,
                    0,
                    1
                );
                return new Point2(xy[0], xy[1]);
            }
            throw new NotImplementedException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            if (values == null) throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }
    }

    /*

    public class Proj4ProjectionTransform : Proj4Transform
    {

        public Proj4ProjectionTransform(ProjectionInfo piFrom, ProjectionInfo piTo) {
            if (piFrom == null) throw new ArgumentNullException("piFrom");
            if (piTo == null) throw new ArgumentNullException("piTo");
            Contract.Ensures(From == From);
            Contract.Ensures(To == To);
            From = From;
            To = To;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(PiFrom != null);
            Contract.Invariant(PiTo != null);
        }

        protected ProjectionInfo From { get; private set; }

        protected ProjectionInfo To { get; private set; }

        public override object TransformValue(object value) {
            if (value is Point2) {
                var p2 = (Point2)value;
                var xy = new double[] { 0, 0};
                var z = new double[] { 1 };
                Reproject.ReprojectPoints(
                    xy,
                    z,
                    KnownCoordinateSystems.Projected.NorthAmerica.USAContiguousLambertConformalConic,
                    KnownCoordinateSystems.Geographic.World.WGS1984,
                    0,
                    1);
                return new Point2(xy[0], xy[1]);
            }
            throw new NotImplementedException();
        }

        public override Type[] GetInputTypes() {
            return new[] {
                typeof(double[])
            };
        }

        public override Type[] GetOutputTypes(Type inputType) {
            if (inputType == typeof (double[]))
                return new[] {inputType};
            return new Type[0];
        }

        public override bool HasInverse {
            get { throw new NotImplementedException(); }
        }

        public override ITransformation GetInverse() {
            throw new NotImplementedException();
        }

    }*/


}
