using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

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

		[CLSCompliant(false)]
		public DataTransmogrifier.EpsgRepository Repository { get { return _repository; }}

		[TestFixtureSetUp]
		public void FixtureSetUp() {
			var asmDirectory = new FileInfo(new Uri(typeof(EpsgDataTestBase<,>).Assembly.CodeBase).LocalPath).Directory;
            var dataFolderDirectory = new DirectoryInfo("../../build/data");
            var file = dataFolderDirectory.GetFiles("EPSG_v*.mdb").OrderByDescending(x => x.Name).First();
			_repository = new DataTransmogrifier.EpsgRepository(file);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown() {
			if (null != _repository)
				_repository.Dispose();
		}

		protected void AssertMatches(IEnumerable<T1> a, IEnumerable<T2> b, params Tester[] matchTests) {
			using(var aEnumerator = a.GetEnumerator())
			using(var bEnumerator = b.GetEnumerator())
			do {
				var aMoved = aEnumerator.MoveNext();
				var bMoved = bEnumerator.MoveNext();
					
				if(!aMoved || !bMoved)
				{
					if (!aMoved && !bMoved)
						return;
					if (aMoved)
						Assert.Fail("First list has more items.");
                    Assert.Fail("Second list has more items.");
				}

				foreach (var matchTest in matchTests) {
					var isMatch = matchTest.Test(aEnumerator.Current, bEnumerator.Current);
					Assert.IsTrue(isMatch, matchTest.Name);
				}

			} while (true);
			
			
		}

	}
}
