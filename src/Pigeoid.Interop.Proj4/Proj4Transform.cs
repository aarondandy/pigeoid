using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using DotSpatial.Projections;
using Vertesaur.Transformation;

namespace Pigeoid.Interop.Proj4
{
    public abstract class Proj4Transform : ITransformation
    {

        public static Proj4Transform Create(ICrs from, ICrs to) {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            Contract.Ensures(Contract.Result<Proj4Transform>() != null);

            var fromProjected = from as ICrsProjected;

            if (fromProjected != null) {
                if (to is ICrsProjected)
                    return Proj4ProjectionTransform.CreateProjected(fromProjected, (ICrsProjected)to);

                throw new InvalidOperationException();
            }

            throw new InvalidOperationException();
        }

        protected Proj4Transform() { }

        public abstract Type[] GetInputTypes();

        public abstract ITransformation GetInverse();

        public abstract Type[] GetOutputTypes(Type inputType);

        public abstract bool HasInverse { get; }

        public abstract object TransformValue(object value);

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            if(values == null) throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }
    }

    public class Proj4ProjectionTransform : Proj4Transform
    {
        
        public static Proj4ProjectionTransform CreateProjected(ICrsProjected from, ICrsProjected to) {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            Contract.Ensures(Contract.Result<Proj4ProjectionTransform>() != null);
            return new Proj4ProjectionTransform(
                Proj4CrsProjected.CreateProjection(from),
                Proj4CrsProjected.CreateProjection(to));
        }

        public Proj4ProjectionTransform(ProjectionInfo piFrom, ProjectionInfo piTo) {
            if (piFrom == null) throw new ArgumentNullException("piFrom");
            if (piTo == null) throw new ArgumentNullException("piTo");
            Contract.Ensures(PiFrom == piFrom);
            Contract.Ensures(PiTo == piTo);
            PiFrom = piFrom;
            PiTo = piTo;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(PiFrom != null);
            Contract.Invariant(PiTo != null);
        }

        protected ProjectionInfo PiFrom { get; private set; }

        protected ProjectionInfo PiTo { get; private set; }

        public override object TransformValue(object value) {
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

    }


}
