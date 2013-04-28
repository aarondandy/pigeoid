using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Ogc;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Transformation
{

    public class GeocentricGeographicTransformation :
        ITransformation<Point3, GeographicCoordinate>,
        ITransformation<Point3, GeographicHeightCoordinate>,
        IParameterizedCoordinateOperationInfo
    {

        //protected readonly GeographicGeocentricTransformation Core;
        protected readonly double MajorAxis;
        protected readonly double MinorAxis;
        protected readonly double ESq;

        /// <summary>
        /// The spheroid.
        /// </summary>
        public readonly ISpheroid<double> Spheroid;
        private readonly double _eSqMajAxis;
        private readonly double _eSecSqMinAxis;

        internal GeocentricGeographicTransformation(ISpheroid<double> spheroid, bool isInverse)
            : this(spheroid) {
            Contract.Requires(spheroid != null);
            IsInverseOfDefinition = isInverse;
        }

        public GeocentricGeographicTransformation(ISpheroid<double> spheroid) {
            if (null == spheroid) throw new ArgumentNullException("spheroid");
            Contract.EndContractBlock();

            MajorAxis = spheroid.A;
            MinorAxis = spheroid.B;
            ESq = spheroid.ESquared;
            Spheroid = spheroid;

            _eSqMajAxis = ESq * MajorAxis;
            _eSecSqMinAxis = spheroid.ESecondSquared * MinorAxis;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
        }

        GeographicCoordinate ITransformation<Point3, GeographicCoordinate>.TransformValue(Point3 geocentric) {
            return TransformValue2D(geocentric);
        }

        public GeographicCoordinate TransformValue2D(Point3 geocentric) {
            double p = Math.Sqrt((geocentric.X * geocentric.X) + (geocentric.Y * geocentric.Y));
            double cosQ = Math.Atan((geocentric.Z * MajorAxis) / (p * MinorAxis));
            double sinQ = Math.Sin(cosQ);
            cosQ = Math.Cos(cosQ);
            return new GeographicCoordinate(
                Math.Atan(
                    (geocentric.Z + (_eSecSqMinAxis * sinQ * sinQ * sinQ))
                    /
                    (p - (_eSqMajAxis * cosQ * cosQ * cosQ))
                ),
                Math.Atan2(geocentric.Y, geocentric.X)
            );
        }

        public GeographicHeightCoordinate TransformValue(Point3 geocentric) {
            var p = Math.Sqrt((geocentric.X * geocentric.X) + (geocentric.Y * geocentric.Y));

            double cosQ = Math.Atan((geocentric.Z * MajorAxis) / (p * MinorAxis));
            double sinQ = Math.Sin(cosQ);
            cosQ = Math.Cos(cosQ);
            double lat = Math.Atan(
                (geocentric.Z + (_eSecSqMinAxis * sinQ * sinQ * sinQ))
                /
                (p - (_eSqMajAxis * cosQ * cosQ * cosQ))
            );
            sinQ = Math.Sin(lat);
            return new GeographicHeightCoordinate(
                lat,
                Math.Atan2(geocentric.Y, geocentric.X),
                (p / Math.Cos(lat))
                - (
                    MajorAxis / Math.Sqrt(
                        1.0 - (ESq * sinQ * sinQ)
                    )
                )
            );
        }

        IEnumerable<GeographicCoordinate> ITransformation<Point3, GeographicCoordinate>.TransformValues(IEnumerable<Point3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicCoordinate>>() != null);
            return values.Select(((ITransformation<Point3, GeographicCoordinate>)this).TransformValue);
        }

        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<Point3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool HasInverse {
            [Pure] get { return 0 != MajorAxis && !Double.IsNaN(MajorAxis); }
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        public GeographicGeocentricTransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<GeographicGeocentricTransformation>() != null);
            return new GeographicGeocentricTransformation(Spheroid, !IsInverseOfDefinition);
        }

        ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicCoordinate, Point3> ITransformation<Point3, GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate, Point3> ITransformation<Point3, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        public string Name {
            get { return "Geocentric To Ellipsoid";}
        }

        public bool IsInverseOfDefinition { get; private set; }

        [Obsolete]
        public IEnumerable<INamedParameter> Parameters {
            get {
                return new[] {
                    new NamedParameter<double>("Semi Major", MajorAxis),
                    new NamedParameter<double>("Semi Minor", MinorAxis)
                };
            }
        }

        [Obsolete]
        public ICoordinateOperationMethodInfo Method {
            get { return new OgcCoordinateOperationMethodInfo(Name); }
        }

        public override string ToString() {
            return Name + ' ' + Spheroid;
        }

    }

    public class GeographicGeocentricTransformation :
        ITransformation<GeographicCoordinate, Point3>,
        ITransformation<GeographicHeightCoordinate, Point3>,
        IParameterizedCoordinateOperationInfo
    {

        protected readonly double MajorAxis;
        protected readonly double MinorAxis;
        protected readonly double ESq;
        protected readonly double OneMinusESq;
        /// <summary>
        /// The spheroid.
        /// </summary>
        public ISpheroid<double> Spheroid { get; private set; }

        internal GeographicGeocentricTransformation(ISpheroid<double> spheroid, bool isInverse)
            : this(spheroid) {
            Contract.Requires(spheroid != null);
            IsInverseOfDefinition = isInverse;
        }

        public GeographicGeocentricTransformation(ISpheroid<double> spheroid) {
            if (null == spheroid) throw new ArgumentNullException("spheroid");
            Contract.EndContractBlock();

            MajorAxis = spheroid.A;
            MinorAxis = spheroid.B;
            ESq = spheroid.ESquared;
            OneMinusESq = 1.0 - ESq;
            Spheroid = spheroid;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants(){
            Contract.Invariant(Spheroid != null);
        }

        public Point3 TransformValue(GeographicCoordinate geographic) {
            var sinLatitude = Math.Sin(geographic.Latitude);
            var v = MajorAxis / Math.Sqrt(
                1.0 - (ESq * sinLatitude * sinLatitude)
            );
            var vCosLatitude = v * Math.Cos(geographic.Latitude);
            return new Point3(
                vCosLatitude * Math.Cos(geographic.Longitude),
                vCosLatitude * Math.Sin(geographic.Longitude),
                OneMinusESq * v * sinLatitude
            );
        }

        public Point3 TransformValue(GeographicHeightCoordinate geographic) {
            var sinLatitude = Math.Sin(geographic.Latitude);
            var v = MajorAxis / Math.Sqrt(
                1.0 - (ESq * sinLatitude * sinLatitude)
            );
            var vHeightCostLatitude = (v + geographic.Height) * Math.Cos(geographic.Latitude);
            return new Point3(
                vHeightCostLatitude * Math.Cos(geographic.Longitude),
                vHeightCostLatitude * Math.Sin(geographic.Longitude),
                ((OneMinusESq * v) + geographic.Height) * sinLatitude
            );
        }

        public IEnumerable<Point3> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != null);
            return values.Select(TransformValue);
        }


        public IEnumerable<Point3> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != null);
            return values.Select(TransformValue);
        }

        public bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return 0 != MinorAxis && !Double.IsNaN(MajorAxis); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Gets the inverse transformation if one exists.
        /// </summary>
        /// <returns>A transformation.</returns>
        public GeocentricGeographicTransformation GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<GeocentricGeographicTransformation>() != null);
            return new GeocentricGeographicTransformation(Spheroid, !IsInverseOfDefinition);
        }

        ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point3, GeographicCoordinate> ITransformation<GeographicCoordinate, Point3>.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point3, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, Point3>.GetInverse() {
            return GetInverse();
        }

        public string Name {
            get { return "Ellipsoid To Geocentric"; }
        }

        public bool IsInverseOfDefinition { get; private set; }

        [Obsolete]
        public IEnumerable<INamedParameter> Parameters {
            get {
                return new[] {
                    new NamedParameter<double>("Semi Major", MajorAxis),
                    new NamedParameter<double>("Semi Minor", MinorAxis)
                };
            }
        }

        [Obsolete]
        public ICoordinateOperationMethodInfo Method {
            get { return new OgcCoordinateOperationMethodInfo(Name); }
        }

        public override string ToString() {
            return Name + ' ' + Spheroid;
        }
    }
}
