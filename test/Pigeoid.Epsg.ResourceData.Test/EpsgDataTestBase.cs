using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using MbUnit.Framework;

namespace Pigeoid.Epsg.ResourceData.Test
{
	public abstract class EpsgDataTestBase<T1,T2>
	{

		public class Tester
		{
			public readonly Func<T1, T2, bool> Test;
			public readonly string Name;

			public Tester(Expression<Func<T1, T2, bool>> testExp) {
				Name = testExp.ToString();
				Test = testExp.Compile();
			}

		}

		private DataTransmogrifier.EpsgRepository _repository;

		public DataTransmogrifier.EpsgRepository Repository { get { return _repository; }}

		[FixtureSetUp]
		public void FixtureSetUp() {
			_repository = new DataTransmogrifier.EpsgRepository(new FileInfo("EPSG_v7_9.mdb"));
		}

		[FixtureTearDown]
		public void FixtureTearDown() {
			if (null != _repository)
				_repository.Dispose();
		}

		protected void AssertMatches(IEnumerable<T1> a, IEnumerable<T2> b, params Tester[] matchTests) {
			using(var enuma = a.GetEnumerator())
			using(var enumb = b.GetEnumerator()) {
				do {
					if (enuma.MoveNext()) {
						if (!enumb.MoveNext())
						{
							Assert.Fail("First list has more items.");
							break;
						}
					}
					else {
						if (enumb.MoveNext())
							Assert.Fail("First list has fewer items.");

						break;
					}

					foreach (var matchTest in matchTests) {
						var isMatch = matchTest.Test(enuma.Current, enumb.Current);
						Assert.IsTrue(isMatch, matchTest.Name);
					}

				} while (true);
			}
			
		}

	}
}
