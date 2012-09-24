using Vertesaur.Contracts;

namespace Pigeoid.Contracts
{
	public interface IUomConversion<TValue> : ITransformation<TValue>
	{
		IUom From { get; }
		IUom To { get; }
		new IUomConversion<TValue> GetInverse();
	}

	public interface IUomScalarConversion<TValue> : IUomConversion<TValue>
	{
		TValue Factor { get; }
		new IUomScalarConversion<TValue> GetInverse();
	}

}
