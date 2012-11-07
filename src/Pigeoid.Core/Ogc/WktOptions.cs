using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	public class WktOptions : IAuthorityResolver
	{

		/// <summary>
		/// Options for WKT serialization and deserialization.
		/// </summary>
		public WktOptions()
		{
			Pretty = false;
			ThrowOnError = false;
			ResolveAuthorities = true;
			CorrectNames = true;
			SuppressProjectionAuthority = true;
			CultureInfo = null;
			AuthorityResolvers = new List<IAuthorityResolver>();
		}

		public List<IAuthorityResolver> AuthorityResolvers { get; private set; } 

		public bool Pretty { get; set; }

		public bool ThrowOnError { get; set; }

		public bool ResolveAuthorities { get; set; }

		public bool CorrectNames { get; set; }

		public bool SuppressProjectionAuthority { get; set; }

		public CultureInfo CultureInfo { get; set; }

		public CultureInfo GetCultureInfoOrDefault()
		{
			return CultureInfo ?? CultureInfo.CurrentCulture;
		}

		public virtual string ToStringRepresentation(OgcOrientationType orientationType)
		{
			return orientationType.ToString().ToUpperInvariant();
		}

		public virtual string ToStringRepresentation(WktKeyword keyword)
		{
			switch (keyword)
			{
				case WktKeyword.ParamMt:
					return "PARAM_MT";
				case WktKeyword.InverseMt:
					return "INVERSE_MT";
				case WktKeyword.PrimeMeridian:
					return "PRIMEM";
				case WktKeyword.VerticalDatum:
					return "VERT_DATUM";
				case WktKeyword.LocalDatum:
					return "LOCAL_DATUM";
				case WktKeyword.ConcatMt:
					return "CONCAT_MT";
				case WktKeyword.GeographicCs:
					return "GEOGCS";
				case WktKeyword.ProjectedCs:
					return "PROJCS";
				case WktKeyword.GeocentricCs:
					return "GEOCCS";
				case WktKeyword.VerticalCs:
					return "VERT_CS";
				case WktKeyword.LocalCs:
					return "LOCAL_CS";
				case WktKeyword.FittedCs:
					return "FITTED_CS";
				case WktKeyword.CompoundCs:
					return "COMPD_CS";
				case WktKeyword.PassThroughMt:
					return "PASSTHROUGH_MT";
				default:
					return keyword.ToString().ToUpperInvariant();
			}
		}

		public virtual bool IsWhiteSpace(char c) {
			return Char.IsWhiteSpace(c);
		}

		public virtual bool IsDigit(char c) {
			return Char.IsDigit(c);
		}

		public virtual bool IsLetter(char c) {
			return Char.IsLetter(c);
		}

		public virtual bool IsComma(char c) {
			return c == ',';
		}

		public virtual bool IsValidForDoubleValue(char c) {
			if(IsWhiteSpace(c) || IsComma(c))
				return false;
			
			if(IsDigit(c) || '+' == c || '-' == c || 'e' == c || 'E' == c || '.' == c || ',' == c)
				return true;

			var numberInfo = GetCultureInfoOrDefault().NumberFormat;
			return (
				numberInfo.PositiveSign.IndexOf(c) >= 0
				|| numberInfo.NegativeSign.IndexOf(c) >= 0
				|| numberInfo.NumberDecimalSeparator.IndexOf(c) >= 0
				|| numberInfo.NumberGroupSeparator.IndexOf(c) >= 0
			);
		}

		public virtual IAuthorityTag GetAuthorityTag([CanBeNull] object entity) {
			return GetAuthorityTag(entity as IAuthorityBoundEntity);
		}

		public virtual IAuthorityTag GetAuthorityTag([CanBeNull] IAuthorityBoundEntity entity) {
			return null == entity ? null : entity.Authority;
		}

		public virtual bool IsVerticalDatum(OgcDatumType type) {
			return type == OgcDatumType.VerticalOther
				|| type == OgcDatumType.VerticalOrthometric
				|| type == OgcDatumType.VerticalEllipsoidal
				|| type == OgcDatumType.VerticalAltitudeBarometric
				|| type == OgcDatumType.VerticalNormal
				|| type == OgcDatumType.VerticalGeoidModelDerived
				|| type == OgcDatumType.VerticalDepth;
		}

		public virtual OgcDatumType ToDatumType(string typeName) {
			if (!String.IsNullOrEmpty(typeName)){

				// TODO: can these values be cached?
				var allNames = Enum.GetNames(typeof (OgcDatumType));
				for (int i = 0; i < allNames.Length; i++)
					if (String.Equals(typeName, allNames[i], StringComparison.OrdinalIgnoreCase))
						return ((OgcDatumType[]) Enum.GetValues(typeof (OgcDatumType)))[i];

				if (typeName.Equals("GEODETIC", StringComparison.OrdinalIgnoreCase) || typeName.Equals("GEOCENTRIC", StringComparison.OrdinalIgnoreCase))
					return OgcDatumType.HorizontalGeocentric;

				if (typeName.StartsWith("HORIZONTAL", StringComparison.OrdinalIgnoreCase))
					return OgcDatumType.HorizontalOther;

				if (typeName.StartsWith("VERTICAL", StringComparison.OrdinalIgnoreCase))
					return OgcDatumType.VerticalOther;

				if (typeName.Equals("LOCAL", StringComparison.OrdinalIgnoreCase) || typeName.Equals("ENGINEERING", StringComparison.OrdinalIgnoreCase))
					return OgcDatumType.LocalOther;
			}
			return OgcDatumType.None;
		}

		private T GetAuthorityBoundObject<T>(Func<IAuthorityResolver,T> func) where T:class {
			return AuthorityResolvers.Select(func).FirstOrDefault(x => null != x);
		}

		public IAuthorityTag GetAuthorityTag(string authorityName, string code) {
			return GetAuthorityBoundObject(r => r.GetAuthorityTag(authorityName, code));
		}

		public ICrs GetCrs(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetCrs(tag));
		}

		public ISpheroidInfo GetSpheroid(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetSpheroid(tag));
		}

		public IPrimeMeridianInfo GetPrimeMeridian(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetPrimeMeridian(tag));
		}

		public IDatum GetDatum(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetDatum(tag));
		}

		public IUnit GetUom(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetUom(tag));
		}

		public ICoordinateOperationMethodInfo GetCoordinateOperationMethod(IAuthorityTag tag) {
			return GetAuthorityBoundObject(r => r.GetCoordinateOperationMethod(tag));
		}
	}
}
