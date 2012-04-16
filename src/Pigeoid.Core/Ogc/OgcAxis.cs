using System;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// An axis of a coordinate system.
	/// </summary>
	public class OgcAxis : IAxis
	{

		private readonly string _name;
		private readonly OgcOrientationType _ogcOrientationType;

		/// <summary>
		/// Constructs a new axis.
		/// </summary>
		/// <param name="name">The name of the axis.</param>
		/// <param name="ogcOrientationType">The direction of the axis.</param>
		public OgcAxis(string name, OgcOrientationType ogcOrientationType) {
			_name = name ?? String.Empty;
			_ogcOrientationType = ogcOrientationType;
		}

		public string Name {
			get { return _name; }
		}

		public OgcOrientationType OgcOrientationType {
			get { return _ogcOrientationType; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string IAxis.Orientation {
			get { return _ogcOrientationType.ToString(); }
		}
	}
}
