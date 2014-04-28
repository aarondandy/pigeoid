using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;

namespace Pigeoid
{

    /// <summary>
    /// A geodetic datum.
    /// </summary>
    [ContractClass(typeof(IDatumGeodeticCodeContracts))]
    public interface IDatumGeodetic : IDatum
    {
        /// <summary>
        /// The spheroid.
        /// </summary>
        ISpheroidInfo Spheroid { get; }
        /// <summary>
        /// The prime meridian.
        /// </summary>
        IPrimeMeridianInfo PrimeMeridian { get; }

        Helmert7Transformation BasicWgs84Transformation { get; }

        bool IsTransformableToWgs84 { get; }
    }

    [ContractClassFor(typeof(IDatumGeodetic))]
    internal abstract class IDatumGeodeticCodeContracts : IDatumGeodetic
    {

        public ISpheroidInfo Spheroid {
            get {
                Contract.Ensures(Contract.Result<ISpheroidInfo>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract IPrimeMeridianInfo PrimeMeridian { get; }

        public Helmert7Transformation BasicWgs84Transformation {
            get {
                Contract.Ensures(!IsTransformableToWgs84 || Contract.Result<Helmert7Transformation>() != null);
                throw new NotImplementedException();
            }
        }

        public abstract bool IsTransformableToWgs84 { get; }

        public abstract string Name { get; }

        public abstract string Type { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
