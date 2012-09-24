using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfo : IParameterizedCoordinateOperationInfo, IAuthorityBoundEntity
	{

		public CoordinateOperationInfo(
			string name,
			IEnumerable<INamedParameter> parameters = null,
			ICoordinateOperationMethodInfo method = null,
			IAuthorityTag authority = null,
			bool hasInverse = true
		) {
			Name = name;
			Parameters = null == parameters ? new List<INamedParameter>() : parameters.ToList();
			HasInverse = hasInverse;
			Method = method;
			Authority = authority;
		}

		public string Name { get; protected set; }

		public List<INamedParameter> Parameters { get; private set; }

		IEnumerable<INamedParameter> IParameterizedCoordinateOperationInfo.Parameters { get { return Parameters; } }

		public bool HasInverse { get; protected set; }

		public ICoordinateOperationInfo GetInverse()
		{
			if (HasInverse)
				return new CoordinateOperationInfoInverse(this);

			throw new InvalidOperationException("Operation does not have an inverse.");
		}

		public bool IsInverseOfDefinition { get { return false; } }

		public ICoordinateOperationMethodInfo Method { get; private set; }

		public IAuthorityTag Authority { get; private set; }
	}

}
