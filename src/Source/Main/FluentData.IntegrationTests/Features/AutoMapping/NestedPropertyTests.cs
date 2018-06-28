﻿using IntegrationTests._Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.AutoMapping
{
	[TestClass]
	public class NestedPropertyTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test_multiple_property_levels()
		{
			var report = Context.Sql(@"select o.*,
												l.OrderLineId as OrderLine_OrderLineId,
												p.ProductId as OrderLine_Product_ProductId,
												p.Name as OrderLine_Product_Name,
												c.CategoryId as OrderLine_Product_Category_CategoryId,
												c.Name as OrderLine_Product_Category_Name
											from [Order] o
											inner join OrderLine l on o.OrderId = l.OrderId
											inner join Product p on l.ProductId = p.ProductId
											inner join Category c on p.CategoryId = c.CategoryId")
									.QueryMany<OrderReport>();

			Assert.IsTrue(report.Count > 0);
		}
	}
}
