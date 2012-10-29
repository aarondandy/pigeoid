using System;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class CoreCoordinateOperationToTransformationConverter : ICoordinateOperationToTransformationConverter
	{

		public ITransformation Convert([NotNull] ICoordinateOperationInfo operation) {
			if(null == operation)
				throw new ArgumentNullException("operation");
			if (operation is IConcatenatedCoordinateOperationInfo)
				return null;

			throw new NotImplementedException();
		}
	}
}
