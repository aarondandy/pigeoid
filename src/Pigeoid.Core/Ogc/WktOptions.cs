using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	public class WktOptions
	{

		/// <summary>
		/// Options for WKT serialization and deserialization.
		/// </summary>
		public WktOptions()
		{
			Pretty = false;
			ThrowOnError = false;
			CultureInfo = null;
			SpheroidAuthorityResolvers = new List<Func<IAuthorityTag, ISpheroidInfo>>();
			UomAuthorityResolvers = new List<Func<IAuthorityTag, IUom>>();
			PrimeMeridianAuthorityResolvers = new List<Func<IAuthorityTag, IPrimeMeridianInfo>>();
		}

		public List<Func<IAuthorityTag, ISpheroidInfo>> SpheroidAuthorityResolvers { get; private set; }

		public List<Func<IAuthorityTag, IUom>> UomAuthorityResolvers { get; private set; }

		public List<Func<IAuthorityTag, IPrimeMeridianInfo>> PrimeMeridianAuthorityResolvers { get; private set; } 

		public bool Pretty { get; set; }

		public bool ThrowOnError { get; set; }

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

		public virtual string GetEntityName(object entity) {
			if (entity is ICoordinateOperationInfo)
				return (entity as ICoordinateOperationInfo).Name;
			if (entity is ISpheroidInfo)
				return (entity as ISpheroidInfo).Name;
			if (entity is IPrimeMeridianInfo)
				return (entity as IPrimeMeridianInfo).Name;

			throw new NotSupportedException();
		}

		public virtual IAuthorityTag GetAuthorityTag(object entity) {
			if (entity is IAuthorityBoundEntity)
				return (entity as IAuthorityBoundEntity).Authority;

			throw new NotSupportedException();
		}

		public virtual bool IsLocalDatum(OgcDatumType type) {
			return type == OgcDatumType.LocalOther;
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

			// TODO: can these values be cached?
			var allNames = Enum.GetNames(typeof(OgcDatumType));
			for (int i = 0; i < allNames.Length; i++)
				if (String.Equals(typeName, allNames[i], StringComparison.OrdinalIgnoreCase))
					return ((OgcDatumType[])Enum.GetValues(typeof(OgcDatumType)))[i];
			
			if(typeName.Equals("GEODETIC", StringComparison.OrdinalIgnoreCase) || typeName.Equals("GEOCENTRIC", StringComparison.OrdinalIgnoreCase))
				return OgcDatumType.HorizontalGeocentric;

			if(typeName.StartsWith("HORIZONTAL", StringComparison.OrdinalIgnoreCase))
				return OgcDatumType.HorizontalOther;

			if(typeName.StartsWith("VERTICAL", StringComparison.OrdinalIgnoreCase))
				return OgcDatumType.VerticalOther;

			if (typeName.Equals("LOCAL", StringComparison.OrdinalIgnoreCase) || typeName.Equals("ENGINEERING", StringComparison.OrdinalIgnoreCase))
				return OgcDatumType.LocalOther;

			return OgcDatumType.None;
		}

		public virtual IAuthorityTag CreateAuthority(string name, string code) {
			return new AuthorityTag(name, code);
		}

		public virtual ISpheroidInfo CreateSpheroidFromAuthority(IAuthorityTag authority) {
			if (null == authority || null == SpheroidAuthorityResolvers || 0 == SpheroidAuthorityResolvers.Count)
				return null;

			return SpheroidAuthorityResolvers
				.Select(resolver => resolver(authority))
				.FirstOrDefault(result => null != result);
		}

		public virtual IUom CreateUomFromAuthority(IAuthorityTag authorityTag) {
			if (null == authorityTag || null == UomAuthorityResolvers || 0 == UomAuthorityResolvers.Count)
				return null;

			return UomAuthorityResolvers
				.Select(resolver => resolver(authorityTag))
				.FirstOrDefault(result => null != result);
		}

		public OgcAngularUnit GetAngularUnit(string name) {
			return null;
		}

		public IPrimeMeridianInfo CreatePrimeMeridianFromAuthority(IAuthorityTag authorityTag) {
			if (null == authorityTag || null == PrimeMeridianAuthorityResolvers || 0 == PrimeMeridianAuthorityResolvers.Count)
				return null;

			return PrimeMeridianAuthorityResolvers
				.Select(resolver => resolver(authorityTag))
				.FirstOrDefault(result => null != result);
		}
	}
}
