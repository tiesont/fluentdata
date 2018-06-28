﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Documentation
{
	[TestClass]
    public class QueryValueTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test()
		{
			int numberOfProducts = Context.Sql(@"select count(*)
													from Product").QuerySingle<int>();

			Assert.IsTrue(numberOfProducts > 0);
		}
	}
}
