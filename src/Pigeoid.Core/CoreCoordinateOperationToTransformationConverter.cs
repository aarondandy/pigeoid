using System;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class CoreCoordinateOperationToTransformationConverter : ICoordinateOperationToTransformationConverter
	{

		public CoreCoordinateOperationToTransformationConverter() { }

		public ITransformation Convert(ICoordinateOperationInfo operation) {
			if(null == operation)
				throw new ArgumentNullException("operation");
			if (operation is IConcatenatedCoordinateOperationInfo)
				return null;

			throw new NotImplementedException();
		}
	}
}
