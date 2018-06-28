﻿using System;
using System.Collections.Generic;
using FluentData;
using IntegrationTests._Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Documentation
{
	[TestClass]
    public class EntityFactoryTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test()
		{
			List<Product> products = Context.EntityFactory(new CustomEntityFactory()).Sql("select * from Product").QueryMany<Product>();

			Assert.IsTrue(products.Count > 0);
		}

		public class CustomEntityFactory : IEntityFactory
		{
		public virtual object Create(Type type)
		{
			return Activator.CreateInstance(type);
		}
		}
	}
}
