using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class BasicCoordinateOperationToTransformationGenerator : ICoordinateOperationToTransformationGenerator
	{

		private class CrsData
		{

			private static Type ChooseGeometryType(ICrs crs) {
				if (crs is ICrsGeodetic)
					return ChooseGeometryType(crs as ICrsGeodetic);
				if (crs is ICrsVertical)
					return typeof(double);
				return null;
			}

			private static Type ChooseGeometryType(ICrsGeodetic crs) {
				if (crs.Axes.Count == 2) {
					// 2D or geog
					if (crs is ICrsProjected) {
						return typeof(Point2);
					}
					return typeof(GeographicCoordinate);
				}
				if (crs.Axes.Count == 3) {
					// 3D or geog-H
					if (crs is ICrsGeocentric)
						return typeof(Point3);
					return typeof(GeographicHeightCoordinate);
				}
				return null;
			}

			public CrsData(ICrs crs) {
				Crs = crs;
				CoordianteType = ChooseGeometryType(crs);

				var crsGeodetic = crs as ICrsGeodetic;
				if (crsGeodetic != null) {
					CrsUnit = crsGeodetic.Unit;
				}
			}

			public readonly ICrs Crs;
			public readonly Type CoordianteType;
			public readonly IUnit CrsUnit;
		}

		private class TransformationGenerationParams
		{

			public static IEnumerable<ICoordinateOperationInfo> LinearizeOperations(ICoordinateOperationInfo operation) {
				if (null == operation)
					return Enumerable.Empty<ICoordinateOperationInfo>();
				var concatOperations = operation as IConcatenatedCoordinateOperationInfo;
				if (null == concatOperations)
					return new[] {operation};
				return concatOperations.Steps.SelectMany(LinearizeOperations);
			}

			public TransformationGenerationParams(ICoordinateOperationInfo opInfo, CrsData inputData, CrsData outputData) {
				CoordinateOperationInfo = opInfo;
				InputData = inputData;
				OutputData = outputData;
			}

			public readonly CrsData InputData;
			public readonly CrsData OutputData;
			public readonly ICoordinateOperationInfo CoordinateOperationInfo;

			public IUnit DesiredInputUnit;
			public IUnit DesiredOutputUnit;

		}

		private class TransformationGenerationResult
		{
			public ITransformation Transformation;
			public IUnit InputUnit;
			public IUnit OutputUnit;
		}

		private class TransformationStep : TransformationGenerationResult
		{
			public TransformationGenerationParams GenerationParams;
		} 

		public ITransformation Create(ICoordinateOperationCrsPathInfo operationPath) {
			if(null == operationPath)
				throw new ArgumentNullException("operationPath");

			var allOps = operationPath.CoordinateOperations.ToList();
			var allCrs = operationPath.CoordinateReferenceSystems
				.Select(x => new CrsData(x)).ToList();
			if(allCrs.Count == 0)
				throw new ArgumentException("operationPath contains no CRSs", "operationPath");

			var firstCrs = allCrs[0];
			var lastCrs = allCrs[allCrs.Count - 1];

			var steps = new TransformationStep[allOps.Count];
			var previousStep = new TransformationStep{OutputUnit = firstCrs.CrsUnit};
			for (int i = 0; i < steps.Length; i++) {
				var generationParams = new TransformationGenerationParams(
					allOps[i],
					allCrs[i],
					allCrs[i + 1]
				) {
					DesiredInputUnit = previousStep.OutputUnit
				};
				var step = Create(generationParams);
				if (null == step)
					return null; // not supported

				steps[i] = step;
				previousStep = step;
			}

			throw new NotImplementedException("TODO: Add a final conversion for the output unit if needed.");
			throw new NotImplementedException("TODO: Need to build the final transformation.");
		}

		private TransformationStep Create(TransformationGenerationParams generationParams) {
			var operations = TransformationGenerationParams
				.LinearizeOperations(generationParams.CoordinateOperationInfo)
				.ToArray();
			

			throw new NotImplementedException("Need to combine the steps.");
		}

	}
}
