﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.Events
{
	[TestClass]
    public class OnConnectionOpenedTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test()
		{
			var eventFired = false;

			Context.OnExecuted(args => eventFired = true).Sql("select top 1 * from product").QueryMany<dynamic>();

			Assert.IsTrue(eventFired);
		}
	}
}
