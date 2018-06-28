﻿using System.Collections.Generic;
using FluentData;
using IntegrationTests.Providers.MySql;
using IntegrationTests._Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Providers.PostgreSql
{
	[TestClass]
	[Ignore]
	public class PostgreSqlTests : IDbProviderTests
	{
		public PostgreSqlTests()
		{
			Context.Sql(@"drop table if exists Category;
							drop table if exists Product;
							drop function if exists ProductUpdate(integer, varchar);

							CREATE TABLE Category(
								CategoryId INTEGER PRIMARY KEY,
								Name VARCHAR(50) not null);

							CREATE TABLE Product(
								ProductId serial primary key,
								Name VARCHAR(50) not null,
								CategoryId INTEGER not null);

							insert into Category(CategoryId, Name)
							select 1, 'Books'
							union select 2, 'Movies';

							insert into Product(Name, CategoryId)
							select 'The Warren Buffet Way', 1
							union select 'Bill Gates Bio', 1
							union select 'James Bond - Goldeneye', 2
							union select 'The Bourne Identity', 2;

							CREATE FUNCTION ProductUpdate(ParamProductId integer, ParamName varchar(100)) RETURNS void AS $$
							BEGIN
								update Product set Name = ParamName where ProductId = ParamProductId;
							END;
							$$ LANGUAGE plpgsql;
							").Execute();
		}

		protected IDbContext Context
		{
		    get
		    {
		        return new DbContext().ConnectionString(TestHelper.GetConnectionStringValue("PostgreSql"),
		                                                new PostgreSqlProvider());
		    }
		}

		[TestMethod]
		public void Query_many_dynamic()
		{
			var products = Context.Sql("select * from Product").QueryMany<dynamic>();

			Assert.IsTrue(products.Count > 0);
		}

		[TestMethod]
		public void Query_single_dynamic()
		{
			var product = Context.Sql("select * from Product where ProductId = 1").QuerySingle<dynamic>();

			Assert.IsNotNull(product);
		}

		[TestMethod]
		public void Query_many_strongly_typed()
		{
			var products = Context.Sql("select * from Product").QueryMany<Product>();

			Assert.IsTrue(products.Count > 0);
		}

		[TestMethod]
		public void Query_single_strongly_typed()
		{
			var product = Context.Sql("select * from Product where ProductId = 1").QuerySingle<Product>();

			Assert.IsNotNull(product);
		}

		[TestMethod]
		public void Query_auto_mapping_alias()
		{
			var product = Context.Sql(@"select p.*,
											c.CategoryId as Category_CategoryId,
											c.Name as Category_Name
											from Product p
											inner join Category c on p.CategoryId = c.CategoryId
											where ProductId = 1")
									.QuerySingle<Product>();

			Assert.IsNotNull(product);
			Assert.IsNotNull(product.Category);
			Assert.IsNotNull(product.Category.Name);
		}

		[TestMethod]
		public void Query_custom_mapping_dynamic()
		{
			var products = Context.Sql(@"select * from Product")
									.QueryMany<Product>(Custom_mapper_using_dynamic);

			Assert.IsNotNull(products[0].Name);
		}

		public void Custom_mapper_using_dynamic(Product product, dynamic row)
		{
			product.ProductId = row.ProductId;
			product.Name = row.Name;
		}

		[TestMethod]
		public void Query_custom_mapping_datareader()
		{
			var products = Context.Sql(@"select * from Product")
									.QueryMany<Product>(Custom_mapper_using_datareader);

			Assert.IsNotNull(products[0].Name);
		}

		public void Custom_mapper_using_datareader(Product product, IDataReader row)
		{
			product.ProductId = row.GetInt32("ProductId");
			product.Name = row.GetString("Name");
		}

		[TestMethod]
		public void QueryValue()
		{
			int categoryId = Context.Sql("select CategoryId from Product where ProductId = 1")
										.QuerySingle<int>();

			Assert.AreEqual(1, categoryId);
		}

		[TestMethod]
		public void QueryValues()
		{
			var categories = Context.Sql("select CategoryId from Category order by CategoryId").QueryMany<int>();

			Assert.AreEqual(2, categories.Count);
			Assert.AreEqual(1, categories[0]);
			Assert.AreEqual(2, categories[1]);
		}

		[TestMethod]
		public void Unnamed_parameters_one()
		{
			var product = Context.Sql("select * from Product where ProductId = :0", 1).QuerySingle<dynamic>();

			Assert.IsNotNull(product);
		}

		[TestMethod]
		public void Unnamed_parameters_many()
		{
			var products = Context.Sql("select * from Product where ProductId = :0 or ProductId = :1", 1, 2)
									.QueryMany<dynamic>();

			Assert.AreEqual(2, products.Count);
		}

		[TestMethod]
		public void Named_parameters()
		{
			var products = Context.Sql("select * from Product where ProductId = :ProductId1 or ProductId = :ProductId2")
									.Parameter("ProductId1", 1)
									.Parameter("ProductId2", 2)
									.QueryMany<dynamic>();

			Assert.AreEqual(2, products.Count);
		}

		[TestMethod]
		public void In_Query()
		{
			var ids = new List<int>() { 1, 2, 3, 4 };

			var products = Context.Sql("select * from Product where ProductId in(:0)", ids)
									.QueryMany<dynamic>();

			Assert.AreEqual(4, products.Count);
		}

		[TestMethod]
		public void SelectBuilder_Paging()
		{
			var category = Context
				.Select<Category>("CategoryId, Name")
				.From("Category")
				.OrderBy("Name asc")
                .Paging(1, 1).QuerySingle();
			Assert.AreEqual("Books", category.Name);

			category = Context
				.Select<Category>("CategoryId, Name")
				.From("Category")
				.OrderBy("Name asc")
                .Paging(2, 1).QuerySingle();
			Assert.AreEqual("Movies", category.Name);
		}

		[TestMethod]
		public void MultipleResultset()
		{
				using (var command = Context.MultiResultSql)
				{
					var categories = command.Sql(@"select * from Category;
													select * from Product;").QueryMany<dynamic>();
					var products = command.QueryMany<dynamic>();

				Assert.IsTrue(categories.Count > 0);
				Assert.IsTrue(products.Count > 0);
			}
		}

		[TestMethod]
		public void Insert_data_sql()
		{
			var productId = Context.Sql("insert into Product(Name, CategoryId) values(:0, :1);", "The Warren Buffet Way", 1)
							.ExecuteReturnLastId<int>();

			Assert.IsTrue(productId > 0);
		}

		[TestMethod]
		public void Insert_data_builder_no_automapping()
		{
			var productId = Context.Insert("Product")
								.Column("CategoryId", 1)
								.Column("Name", "The Warren Buffet Way")
								.ExecuteReturnLastId<int>();

			Assert.IsTrue(productId > 0);
		}

		[TestMethod]
		public void Insert_data_builder_automapping()
		{
			var product = new Product();
			product.CategoryId = 1;
			product.Name = "The Warren Buffet Way";

			var productId = Context.Insert<Product>("Product", product)
								.AutoMap(x => x.ProductId)
								.ExecuteReturnLastId<int>();

			Assert.IsTrue(productId > 0);
		}

		[TestMethod]
		public void Update_data_sql()
		{
			var rowsAffected = Context.Sql("update Product set Name = :0 where ProductId = :1", "The Warren Buffet Way", 1)
								.Execute();

			Assert.AreEqual(1, rowsAffected);
		}

		[TestMethod]
		public void Update_data_builder()
		{
			var rowsAffected = Context.Update("Product")
								.Column("Name", "The Warren Buffet Way")
								.Where("ProductId", 1)
								.Execute();

			Assert.AreEqual(1, rowsAffected);
		}

		[TestMethod]
		public void Update_data_builder_automapping()
		{
			var product = Context.Sql("select * from Product where ProductId = 1")
								.QuerySingle<Product>();
			
			product.Name = "The Warren Buffet Way";

			var rowsAffected = Context.Update<Product>("Product", product)
										.Where(x => x.ProductId)
										.AutoMap()
										.Execute();

			Assert.AreEqual(1, rowsAffected);
		}

		[TestMethod]
		public void Delete_data_sql()
		{
			var productId = Context.Sql("insert into Product(Name, CategoryId) values(:0, :1);", "The Warren Buffet Way", 1)
							.ExecuteReturnLastId<int>();

			var rowsAffected = Context.Sql("delete from Product where ProductId = :0", productId)
									.Execute();

			Assert.AreEqual(1, rowsAffected);
		}

		[TestMethod]
		public void Delete_data_builder()
		{
			var productId = Context.Sql(@"insert into Product(Name, CategoryId) values(:0, :1)", "The Warren Buffet Way", 1)
								.ExecuteReturnLastId<int>();

			var rowsAffected = Context.Delete("Product")
									.Where("ProductId", productId)
									.Execute();

			Assert.AreEqual(1, rowsAffected);
		}

		[TestMethod]
		public void Transactions()
		{
			using (var context = Context.UseTransaction(true))
			{
				context.Sql("update Product set Name = :0 where ProductId = :1", "The Warren Buffet Way", 1)
							.Execute();

				context.Sql("update Product set Name = :0 where ProductId = :1", "Bill Gates Bio", 2)
							.Execute();

				context.Commit();
			}
		}

		[TestMethod]
		public void Stored_procedure_sql()
		{
			Context.Sql("ProductUpdate")
										.CommandType(DbCommandTypes.StoredProcedure)
										.Parameter("ParamProductId", 1)
										.Parameter("ParamName", "The Warren Buffet Way")
										.Execute();
		}

		[TestMethod]
		public void Stored_procedure_builder()
		{
			Context.StoredProcedure("ProductUpdate")
										.Parameter("ParamProductId", 1)
										.Parameter("ParamName", "The Warren Buffet Way")
										.Execute();
		}

		[TestMethod]
		public void StoredProcedure_builder_automapping()
		{
		    var product = Context.Sql("select * from Product where ProductId = 1")
		                    .QuerySingle<Product>();
		    product.Name = "The Warren Buffet Way";

			var mysqlProduct = new MySqlProduct(product);

			Context.StoredProcedure<MySqlProduct>("ProductUpdate", mysqlProduct)
											.AutoMap(x => x.ParamCategoryId).Execute();
		}

		[TestMethod]
		public void StoredProcedure_builder_using_expression()
		{
			var product = Context.Sql("select * from Product where ProductId = 1")
							.QuerySingle<Product>();
			product.Name = "The Warren Buffet Way";

			var mysqlProduct = new MySqlProduct(product);
			Context.StoredProcedure<MySqlProduct>("ProductUpdate", mysqlProduct)
											.Parameter(x => x.ParamProductId)
											.Parameter(x => x.ParamName).Execute();
		}
	}
}
