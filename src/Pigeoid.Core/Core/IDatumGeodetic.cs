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

        public IPrimeMeridianInfo PrimeMeridian {
            get {
                Contract.Ensures(Contract.Result<IPrimeMeridianInfo>() != null);
                throw new NotImplementedException();
            }
        }

        public Helmert7Transformation BasicWgs84Transformation {
            get {
                Contract.Ensures(!(Contract.Result<Helmert7Transformation>() == null && IsTransformableToWgs84));
                throw new NotImplementedException();
            }
        }

        public bool IsTransformableToWgs84 {
            get {
                Contract.Ensures(!(Contract.Result<bool>() == true && BasicWgs84Transformation == null));
                throw new NotImplementedException();
            }
        }

        public abstract string Name { get; }

        public abstract string Type { get; }

        public abstract IAuthorityTag Authority { get; }
    }

}
