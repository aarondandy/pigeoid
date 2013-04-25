namespace Pigeoid.Contracts
{
    public interface IAuthorityResolver
    {
        IAuthorityTag GetAuthorityTag(string authorityName, string code);

        ICrs GetCrs(IAuthorityTag tag);

        ISpheroidInfo GetSpheroid(IAuthorityTag tag);

        IPrimeMeridianInfo GetPrimeMeridian(IAuthorityTag tag);

        IDatum GetDatum(IAuthorityTag tag);

        IUnit GetUom(IAuthorityTag tag);

        ICoordinateOperationMethodInfo GetCoordinateOperationMethod(IAuthorityTag tag);
    }
}
