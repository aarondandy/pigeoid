using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    /// <summary>
    /// An abridged Molodensky transformation.
    /// </summary>
    public class AbridgedMolodenskyTransformation :
        ITransformation<GeographicHeightCoordinate>,
        ITransformation<GeographicCoordinate>
    {

        private static readonly double SinOne = Math.Sin(1);

        public readonly Vector3 D;
        protected readonly double Da;
        protected readonly double SeSq;
        protected readonly double SaDfSfDa;
        protected readonly double OneMinusESqsaSinOne;
        protected readonly double SaSinOne;

        public ISpheroid<double> SourceSpheroid { get; private set; }
        public ISpheroid<double> TargetSpheroid { get; private set; }

        /// <summary>
        /// Constructs an abridged molodensky transformation.
        /// </summary>
        /// <param name="translation">The amount to translate.</param>
        /// <param name="sourceSpheroid">The source CRS spheroid.</param>
        /// <param name="targetSpheroid">The destination CRS spheroid.</param>
        public AbridgedMolodenskyTransformation(
            Vector3 translation,
            ISpheroid<double> sourceSpheroid,
            ISpheroid<double> targetSpheroid
        ) {
            if(sourceSpheroid == null) throw new ArgumentNullException("sourceSpheroid");
            if(targetSpheroid == null) throw new ArgumentNullException("targetSpheroid");
            Contract.EndContractBlock();
            SourceSpheroid = sourceSpheroid;
            TargetSpheroid = targetSpheroid;
            D = translation;
            var sf = sourceSpheroid.F;
            var tf = targetSpheroid.F;
            var df = tf - sf;
            var sa = sourceSpheroid.A;
            var ta = targetSpheroid.A;
            Da = ta - sa;
            SeSq = sourceSpheroid.ESquared;
            SaDfSfDa = (sa * df) + (sf * Da);
            SaSinOne = sa * SinOne;
            OneMinusESqsaSinOne = SaSinOne * (1.0 - SeSq);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (0 == OneMinusESqsaSinOne || Double.IsNaN(OneMinusESqsaSinOne) || 0 == SaSinOne || Double.IsNaN(SaSinOne))
                throw new ArgumentException("Invalid spheroid.", "sourceSpheroid");
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(SourceSpheroid != null);
            Contract.Invariant(TargetSpheroid != null);
        }

        public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate coordinate) {
            var sinLatitude = Math.Sin(coordinate.Latitude);
            var sinLatitudeSquared = sinLatitude * sinLatitude;
            var cosLatitude = Math.Cos(coordinate.Latitude);
            var sinLongitude = Math.Sin(coordinate.Longitude);
            var cosLongitude = Math.Cos(coordinate.Longitude);
            var c = 1.0 - (SeSq * sinLatitudeSquared);
            var cSq = Math.Sqrt(c);
            var dxdy = (D.X * cosLongitude) + (D.Y * sinLongitude);
            return new GeographicHeightCoordinate(
                coordinate.Latitude + (
                    (
                        (
                            (D.Z * cosLatitude)
                            + (SaDfSfDa * Math.Sin(2.0 * coordinate.Latitude))
                            - (sinLatitude * dxdy)
                        )
                        * c * cSq
                    )
                    / OneMinusESqsaSinOne
                ),
                coordinate.Longitude + (
                    (
                        ((D.Y * cosLongitude) - (D.X * sinLongitude))
                        * cSq
                    )
                    / (cosLatitude * SaSinOne)
                ),
                coordinate.Height + (
                    +(cosLatitude * dxdy)
                    + (D.Z * sinLatitude)
                    + (SaDfSfDa * sinLatitudeSquared)
                    - Da
                )
            );
        }

        private void TransformValue(ref GeographicHeightCoordinate value) {
            value = TransformValue(value);
        }

        public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public void TransformValues(GeographicHeightCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                TransformValue(ref values[i]);
        }

        public GeographicCoordinate TransformValue(GeographicCoordinate coordinate) {
            return (GeographicCoordinate)(TransformValue((GeographicHeightCoordinate)coordinate));
        }

        private void TransformValue(ref GeographicCoordinate value) {
            value = TransformValue(value);
        }

        public IEnumerable<GeographicCoordinate> TransformValues(IEnumerable<GeographicCoordinate> values) {
            Contract.Ensures(Contract.Result<IEnumerable<GeographicHeightCoordinate>>() != null);
            return values.Select(TransformValue);
        }

        public void TransformValues(GeographicCoordinate[] values) {
            for (int i = 0; i < values.Length; i++)
                TransformValue(ref values[i]);
        }

        public object TransformValue(object value) {
            if (value is GeographicHeightCoordinate)
                return TransformValue((GeographicHeightCoordinate) value);
            if (value is GeographicCoordinate)
                return TransformValue((GeographicCoordinate) value);
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }


        public AbridgedMolodenskyTransformation GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<AbridgedMolodenskyTransformation>() != null);
            return new AbridgedMolodenskyTransformation(D.GetNegative(), TargetSpheroid, SourceSpheroid);
        }

        ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicCoordinate, GeographicCoordinate> ITransformation<GeographicCoordinate, GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation<GeographicCoordinate> ITransformation<GeographicCoordinate>.GetInverse() {
            return GetInverse();
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get {
                return 0 != TargetSpheroid.A && !Double.IsNaN(TargetSpheroid.A)
                    && 0 != (1.0 - TargetSpheroid.ESquared) && !Double.IsNaN(TargetSpheroid.ESquared);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public Type[] GetInputTypes() {
            return new[] {typeof (GeographicHeightCoordinate), typeof (GeographicCoordinate)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            if (inputType == typeof (GeographicHeightCoordinate) || inputType == typeof(GeographicCoordinate))
                return new[] { inputType };
            return ArrayUtil<Type>.Empty;
        }

    }
}
