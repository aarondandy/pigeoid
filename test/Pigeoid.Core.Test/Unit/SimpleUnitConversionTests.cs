using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Unit;

namespace Pigeoid.Core.Test.Unit
{
	[TestFixture]
	public class SimpleUnitConversionTests
	{

		private class SimpleUnit : IUnit
		{

			public SimpleUnit(string name, string type = null) {
				Name = name;
				Type = type ?? "a-unit";
			}

			public string Name { get; set; }

			public string Type { get; set; }

			public IUnitConversion<double> ForwardOperation { get; set; }

			public IUnitConversionMap<double> ConversionMap {
				get {
					if (null == ForwardOperation)
						return null;
					return new BinaryUnitConversionMap(ForwardOperation);
				}
			}
		}

		private SimpleUnit Waffle;
		private SimpleUnit Egg;
		private SimpleUnit Chicken;

		[SetUp]
		public void SetUp() {
			Waffle = new SimpleUnit("waffle");
			Egg = new SimpleUnit("egg");
			Egg.ForwardOperation = new UnitRatioConversion(Egg, Waffle, 1, 2);
			Chicken = new SimpleUnit("chicken");
			Chicken.ForwardOperation = new UnitScalarConversion(Chicken, Egg, 6);
		}

		[Test]
		public void SimpleForwardConversion() {
			var conversion = SimpleUnitConversionGenerator.FindConversion(Chicken, Egg);
			Assert.IsNotNull(conversion);
			var eggsFromAChicken = conversion.TransformValue(1);
			Assert.AreEqual(6, eggsFromAChicken);
		}

		[Test]
		public void SimpleReverseConversion() {
			var conversion = SimpleUnitConversionGenerator.FindConversion(Egg, Chicken);
			Assert.IsNotNull(conversion);
			var chickensForAnEgg = conversion.TransformValue(1);
			Assert.AreEqual(1.0 / 6.0, chickensForAnEgg);
		}

		[Test]
		public void BridgedForwardConversion() {
			var conversion = SimpleUnitConversionGenerator.FindConversion(Chicken, Waffle);
			Assert.IsNotNull(conversion);
			var wafflesFromAChicken = conversion.TransformValue(1);
			Assert.AreEqual(3, wafflesFromAChicken);
		}

		[Test]
		public void BridgedReverseConversion() {
			var conversion = SimpleUnitConversionGenerator.FindConversion(Waffle, Chicken);
			Assert.IsNotNull(conversion);
			var chickensToMakeAWaffle = conversion.TransformValue(1);
			Assert.AreEqual(1.0 / 3.0, chickensToMakeAWaffle);
		}

	}
}
