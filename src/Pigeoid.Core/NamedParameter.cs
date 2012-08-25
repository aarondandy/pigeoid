// TODO: source header

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Pigeoid.Contracts;

namespace Pigeoid
{
	/// <summary>
	/// A named parameter.
	/// </summary>
	/// <typeparam name="TValue">The value type of the parameter.</typeparam>
	public class NamedParameter<TValue> : INamedParameter
	{

		/// <summary>
		/// The parameter name.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _name;

		/// <summary>
		/// The parameter value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly TValue _value;

		/// <summary>
		/// The optional unit of measure for the parameter value.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IUom _unit;

		public NamedParameter(string name, TValue value)
			: this(name, value, null) { }

		public NamedParameter(string name, TValue value, IUom unit) {
			_name = name;
			_value = value;
			_unit = unit;
		}

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name { get { return _name; } }

		/// <summary>
		/// Parameter value.
		/// </summary>
		public TValue Value { get { return _value; } }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object INamedParameter.Value { get { return _value; } }

		public IUom Unit { get { return _unit; } }

	}

	/// <summary>
	/// Named parameter utility class.
	/// </summary>
	public static class NamedParameter
	{

		private static readonly ReadOnlyCollection<string> GenericNamesCoreList;

		// TODO: I don't know if exposed properties for the names is the best idea.

		/// <summary>
		/// Angle from Rectified to Skew Grid.
		/// </summary>
		public static readonly string NameAngleFromRectifiedToSkewGrid = "Angle from Rectified to Skew Grid";
		/// <summary>
		/// Azimuth of initial line.
		/// </summary>
		public static readonly string NameAzimuthOfInitialLine = "Azimuth of initial line";
		/// <summary>
		/// Azimuth of the center line.
		/// </summary>
		public static readonly string NameAzimuthOfCenterLine = "Azimuth of the center line";
		/// <summary>
		/// Central Meridian.
		/// </summary>
		public static readonly string NameCentralMeridian = "Central Meridian";
		/// <summary>
		/// False Easting.
		/// </summary>
		public static readonly string NameFalseEasting = "False Easting";
		/// <summary>
		/// False Northing.
		/// </summary>
		public static readonly string NameFalseNorthing = "False Northing";
		/// <summary>
		/// Easting at projection center.
		/// </summary>
		public static readonly string NameEastingAtProjectionCenter = "Easting at projection center";
		/// <summary>
		/// Easting of false origin.
		/// </summary>
		public static readonly string NameEastingOfFalseOrigin = "Easting of false origin";
		/// <summary>
		/// Latitude of false origin.
		/// </summary>
		public static readonly string NameLatitudeOfFalseOrigin = "Latitude of false origin";
		/// <summary>
		/// Latitude of first standard parallel.
		/// </summary>
		public static readonly string NameLatitudeOfFirstStandardParallel = "Latitude of first standard parallel";
		/// <summary>
		/// Latitude of natural origin.
		/// </summary>
		public static readonly string NameLatitudeOfNaturalOrigin = "Latitude of natural origin";
		/// <summary>
		/// Latitude of origin.
		/// </summary>
		public static readonly string NameLatitudeOfOrigin = "Latitude of origin";
		/// <summary>
		/// Latitude of projection center.
		/// </summary>
		public static readonly string NameLatitudeOfProjectionCenter = "Latitude of projection center";
		/// <summary>
		/// Latitude of Pseudo Standard Parallel.
		/// </summary>
		public static readonly string NameLatitudeOfPseudoStandardParallel = "Latitude of Pseudo Standard Parallel";
		/// <summary>
		/// Latitude of second standard parallel.
		/// </summary>
		public static readonly string NameLatitudeOfSecondStandardParallel = "Latitude of second standard parallel";
		/// <summary>
		/// Latitude of true scale.
		/// </summary>
		public static readonly string NameLatitudeOfTrueScale = "Latitude of true scale";
		/// <summary>
		/// Longitude of false origin.
		/// </summary>
		public static readonly string NameLongitudeOfFalseOrigin = "Longitude of false origin";
		/// <summary>
		/// Longitude of natural origin.
		/// </summary>
		public static readonly string NameLongitudeOfNaturalOrigin = "Longitude of natural origin";
		/// <summary>
		/// Longitude of projection center.
		/// </summary>
		public static readonly string NameLongitudeOfProjectionCenter = "Longitude of projection center";
		/// <summary>
		/// Northing at projection center.
		/// </summary>
		public static readonly string NameNorthingAtProjectionCenter = "Northing at projection center";
		/// <summary>
		/// Northing of false origin.
		/// </summary>
		public static readonly string NameNorthingOfFalseOrigin = "Northing of false origin";
		/// <summary>
		/// Rectified grid angle.
		/// </summary>
		public static readonly string NameRectifiedGridAngle = "Rectified grid angle";
		/// <summary>
		/// Satellite Height.
		/// </summary>
		public static readonly string NameSatelliteHeight = "Satellite Height";
		/// <summary>
		/// Scale factor at natural origin.
		/// </summary>
		public static readonly string NameScaleFactorAtNaturalOrigin = "Scale factor at natural origin";
		/// <summary>
		/// Scale factor on initial line.
		/// </summary>
		public static readonly string NameScaleFactorOnInitialLine = "Scale factor on initial line";
		/// <summary>
		/// Scale factor on the pseudo standard line.
		/// </summary>
		public static readonly string NameScaleFactorOnPseudoStandardLine = "Scale factor on the pseudo standard line";
		/// <summary>
		/// Standard Parallel.
		/// </summary>
		public static readonly string NameStandardParallel = "Standard Parallel";

		static NamedParameter() {
			GenericNamesCoreList = Array.AsReadOnly(new[] {
				NameAngleFromRectifiedToSkewGrid,
				NameAzimuthOfInitialLine,
				NameAzimuthOfCenterLine,
				NameCentralMeridian,
				NameEastingAtProjectionCenter,
				NameEastingOfFalseOrigin,
				NameFalseEasting,
				NameFalseNorthing,
				NameLatitudeOfFalseOrigin,
				NameLatitudeOfFirstStandardParallel,
				NameLatitudeOfNaturalOrigin,
				NameLatitudeOfOrigin,
				NameLatitudeOfProjectionCenter,
				NameLatitudeOfPseudoStandardParallel,
				NameLatitudeOfSecondStandardParallel,
				NameLatitudeOfTrueScale,
				NameLongitudeOfFalseOrigin,
				NameLongitudeOfNaturalOrigin,
				NameLongitudeOfProjectionCenter,
				NameNorthingAtProjectionCenter,
				NameNorthingOfFalseOrigin,
				NameRectifiedGridAngle,
				NameSatelliteHeight,
				NameScaleFactorAtNaturalOrigin,
				NameScaleFactorOnInitialLine,
				NameScaleFactorOnPseudoStandardLine,
				NameStandardParallel
			});
		}

		/// <summary>
		/// All generic names.
		/// </summary>
		public static IEnumerable<string> GenericNames {
			get { return GenericNamesCoreList; }
		}

	}
}
