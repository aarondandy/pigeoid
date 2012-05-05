using System;

namespace Pigeoid.Epsg.DataTransmogrifier
{
	public class EpsgCoordinateOperation
	{

		public virtual int Code { get; set; }

		public virtual string Name { get; set; }

		public virtual string TypeName { get; set; }

		public virtual EpsgCrs SourceCrs { get; set; }

		public virtual EpsgCrs TargetCrs { get; set; }

		public virtual string TransformVersion { get; set; }

		public virtual int? Variant { get; set; }

		public virtual EpsgArea Area { get; set; }

		public virtual string Scope { get; set; }

		public virtual double? Accuracy { get; set; }

		public virtual int MethodCode { get; set; }

		public virtual int? SourceUomCode { get; set; }

		public virtual int? TargetUomCode { get; set; }

		public virtual string Remarks { get; set; }

		public virtual string InformationSource { get; set; }

		public virtual string DataSource { get; set; }

		public virtual DateTime RevisionDate { get; set; }

		public virtual string ChangeId { get; set; }

		public virtual bool ShowOperation { get; set; }

		public virtual bool Deprecated { get; set; }

		public override string ToString() {
			return Name + " (" + Code + ')';
		}

	}
}
