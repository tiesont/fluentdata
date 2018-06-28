﻿using FluentData;
using IntegrationTests._Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.Builders.StoredProcedure
{
	[TestClass]
    public class StoredProcedureGenericTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test_No_Automap()
		{
			var product = new Product();
			product.Name = "TestProduct";
			product.Category = new Category();
			product.CategoryId = 1;

			using (var context = Context.UseTransaction(true))
			{
				var storedProcedure = context.StoredProcedure<Product>("ProductInsert", product)
							.ParameterOut("ProductId", DataTypes.Int32)
							.Parameter("Name", product.Name)
							.Parameter("CategoryId", product.Category.CategoryId);

				storedProcedure.Execute();
				product.ProductId = storedProcedure.ParameterValue<int>("ProductId");

				Assert.IsTrue(product.ProductId > 0);
			}
		}

		[TestMethod]
		public void TestAutomap()
		{
			var product = new Product();
			product.Name = "TestProduct";
			product.CategoryId = 1;

			using (var context = Context.UseTransaction(true))
			{
				var storedProcedure = context.StoredProcedure<Product>("ProductInsert", product)
					.ParameterOut("ProductId", DataTypes.Int32)
					.AutoMap(x => x.ProductId, x => x.Category);

				storedProcedure.Execute();
				product.ProductId = storedProcedure.ParameterValue<int>("ProductId");

				Assert.IsTrue(product.ProductId > 0);
			}
		}
	}
}
