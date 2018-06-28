﻿using FluentData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.Builders.StoredProcedure
{
	[TestClass]
    public class ParameterTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test_Output()
		{
			var builder = Context.StoredProcedure("TestOutputParameter").ParameterOut("ProductName", DataTypes.String, 50);

			builder.Execute();

			var value = builder.ParameterValue<string>("ProductName");

			Assert.IsNotNull(value);
		}
	}
}
