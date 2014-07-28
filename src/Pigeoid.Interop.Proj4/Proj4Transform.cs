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
            return new Proj4Transform(Target, Source);
        }

        public bool HasInverse {
            get { return true; ; }
        }

        public object TransformValue(object value) {
            if (value == null)
                return null;
            var valueType = value.GetType();
            if (valueType == typeof(Point2)) {
                var p2 = (Point2)value;
                var xy = new double[] { p2.X, p2.Y };
                var z = new double[] { 0 }; // TODO: 0?
                Reproject.ReprojectPoints(
                    xy,
                    z,
                    SourceProj4,
                    TargetProj4,
                    0,
                    1
                );
                if (TargetProj4.IsLatLon)
                    return new GeographicCoordinate(xy[1], xy[0]);
                else if (TargetProj4.IsGeocentric)
                    return new Point3(xy[0], xy[1], z[0]);
                return new Point2(xy[0], xy[1]);
            }
            else if (valueType == typeof(GeographicCoordinate)) {
                var g2 = (GeographicCoordinate)value;
                var xy = new double[] { g2.Longitude, g2.Latitude};
                var z = new double[] { 0 }; // TODO: 0?
                Reproject.ReprojectPoints(
                    xy,
                    z,
                    SourceProj4,
                    TargetProj4,
                    0,
                    1
                );
                if (TargetProj4.IsLatLon)
                    return new GeographicCoordinate(xy[1], xy[0]);
                else if (TargetProj4.IsGeocentric)
                    return new Point3(xy[0], xy[1], z[0]);
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


}
