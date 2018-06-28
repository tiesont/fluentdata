﻿using FluentData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.Parameters
{
	[TestClass]
	public class OutParametersTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test()
		{
			var command = Context.Sql("select top 1 @CategoryName = Name from Category")
												.ParameterOut("CategoryName", DataTypes.String, 50);
			command.Execute();

			var categoryName = command.ParameterValue<string>("CategoryName");

			Assert.IsFalse(string.IsNullOrEmpty(categoryName));
		}

		[TestMethod]
		public void Test_null()
		{
			var command = Context.Sql("select @CategoryName = null")
												.ParameterOut("CategoryName", DataTypes.String, 50);
			command.Execute();

			var categoryName = command.ParameterValue<string>("CategoryName");

			Assert.IsNull(categoryName);
		}
	}
}
