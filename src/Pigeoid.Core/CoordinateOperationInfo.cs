using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid
{
	public class CoordinateOperationInfo : IParameterizedCoordinateOperationInfo, IAuthorityBoundEntity
	{

		private static readonly ReadOnlyCollection<INamedParameter> EmptyParameterList = Array.AsReadOnly(new INamedParameter[0]);

		public CoordinateOperationInfo(
			string name,
			IEnumerable<INamedParameter> parameters = null,
			ICoordinateOperationMethodInfo method = null,
			IAuthorityTag authority = null,
			bool hasInverse = true
		) {
			Name = name;
			Parameters = null == parameters
				? EmptyParameterList
				: Array.AsReadOnly(parameters.ToArray());
			HasInverse = hasInverse;
			Method = method;
			Authority = authority;
		}

		public string Name { get; protected set; }

		public ReadOnlyCollection<INamedParameter> Parameters { get; private set; }

		IEnumerable<INamedParameter> IParameterizedCoordinateOperationInfo.Parameters { get { return Parameters; } }

		public bool HasInverse { get; protected set; }

		public ICoordinateOperationInfo GetInverse() {
			if (!HasInverse)
				throw new InvalidOperationException("Operation does not have an inverse.");

			return new CoordinateOperationInfoInverse(this);
		}

		public bool IsInverseOfDefinition { [ContractAnnotation("=>false")] get { return false; } }

		public ICoordinateOperationMethodInfo Method { get; private set; }

		public IAuthorityTag Authority { get; private set; }
	}

}
