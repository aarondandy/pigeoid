namespace Pigeoid.Epsg
{
	public class EpsgCoordinateOperationInverse : CoordinateOperationInfoInverse
	{

		internal EpsgCoordinateOperationInverse(EpsgCoordinateOperationInfoBase core)
			: base(core) { }

		public new EpsgCoordinateOperationInfoBase GetInverse() {
			return base.GetInverse() as EpsgCoordinateOperationInfoBase;
		}

	}
}
