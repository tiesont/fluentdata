﻿using System.Collections.Generic;
using IntegrationTests._Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Documentation
{
	[TestClass]
	public class SampleCode : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Get_a_single_product()
		{
			Product product = Context.Sql(@"select *	from Product where ProductId = 1").QuerySingle<Product>();

			Assert.IsNotNull(product);
		}

		[TestMethod]
		public void Get_many_products()
		{
			List<Product> products = Context.Sql(@"select * from Product").QueryMany<Product>();

			Assert.IsTrue(products.Count > 0);
		}

		[TestMethod]
		public void Insert_a_new_product()
		{
			var productId = Context.Insert("Product")
										.Column("Name", "The Warren Buffet Way")
										.Column("CategoryId", 1)
										.ExecuteReturnLastId<int>();

			Assert.IsTrue(productId > 0);
		}

		[TestMethod]
		public void Insert_a_new_product_sql()
		{
			var productId = Context.Sql(@"insert into Product(Name, CategoryId)
											values('The Warren Buffet Way', 1);").ExecuteReturnLastId<int>();

			Assert.IsTrue(productId > 0);
		}

		[TestMethod]
		public void Update_existing_product()
		{
			var rowsAffected = Context.Update("Product")
									.Column("Name", "The Warren Buffet Way")
									.Column("CategoryId", 1)
									.Where("ProductId", 1)
									.Execute();

			Assert.IsTrue(rowsAffected > 0);
		}

		[TestMethod]
		public void Update_existing_product_sql()
		{
			var rowsAffected = Context.Sql(@"update Product
												set Name = 'The Warren Buffet Way', CategoryId = 1
												where ProductId = 1").Execute();

			Assert.IsTrue(rowsAffected > 0);
		}

		[TestMethod]
		public void Delete_a_product_sql()
		{
			var productId = Context.Insert("Product")
										.Column("Name", "The Warren Buffet Way")
										.Column("CategoryId", 1)
										.ExecuteReturnLastId<int>();

			var rowsAffected = Context.Sql("delete from Product where ProductId = @0", productId).Execute();

			Assert.IsTrue(rowsAffected > 0);
		}

		[TestMethod]
		public void Delete_a_product()
		{
			var productId = Context.Insert("Product")
										.Column("Name", "The Warren Buffet Way")
										.Column("CategoryId", 1)
										.ExecuteReturnLastId<int>();

			var rowsAffected = Context.Delete("Product").Where("ProductId", productId).Execute();

			Assert.IsTrue(rowsAffected > 0);
		}

		[TestMethod]
		public void Parameters_indexed()
		{
			Product product = Context.Sql(@"select *	from Product where ProductId = @0", 1).QuerySingle<Product>();

			Assert.IsNotNull(product);
		}

		[TestMethod]
		public void Parameters_named()
		{
			Product product = Context.Sql(@"select *	from Product where ProductId = @ProductId")
											.Parameter("ProductId", 1).QuerySingle<Product>();

			Assert.IsNotNull(product);
		}
	}
}
