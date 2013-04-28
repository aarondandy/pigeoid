using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation;

namespace Pigeoid.Epsg
{
    public class EpsgCoordinateOperationInverse : CoordinateOperationInfoInverse
    {

        internal EpsgCoordinateOperationInverse(EpsgCoordinateOperationInfoBase core)
            : base(core) { Contract.Requires(core != null);}

        public new EpsgCoordinateOperationInfoBase GetInverse() {
            Contract.Ensures(Contract.Result<EpsgCoordinateOperationInfoBase>() != null);
            return (EpsgCoordinateOperationInfoBase)(base.GetInverse());
        }

    }
}
