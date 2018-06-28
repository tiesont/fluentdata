﻿using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests.Features.Events
{
	[TestClass]
    public class OnConnectionClosedTests : BaseSqlServerIntegrationTest
	{
		[TestMethod]
		public void Test_non_transaction()
		{
			var eventFiredCounter = 0;
			var connectionState = ConnectionState.Open;

			using (var context = Context.OnConnectionClosed(args => { eventFiredCounter++; connectionState = args.Connection.State; }))
			{
				context.Sql("select top 1 * from product").QueryMany<dynamic>();
				context.Sql("select top 1 * from product").QueryMany<dynamic>();
			}

			Assert.AreEqual(2, eventFiredCounter);
			Assert.AreEqual(ConnectionState.Closed, connectionState);
		}

		[TestMethod]
		public void Test_multiple_resultset()
		{
			var eventFired = false;
			var connectionState = ConnectionState.Open;

			var context = Context.OnConnectionClosed(args => { eventFired = true; connectionState = args.Connection.State; });
			using (var cmd = context.MultiResultSql.Sql("select top 1 * from product;select top 1 * from Product"))
			{
				cmd.QueryMany<dynamic>();
				cmd.QueryMany<dynamic>();
			}

			Assert.IsTrue(eventFired);
			Assert.AreEqual(ConnectionState.Closed, connectionState);
		}

		[TestMethod]
		public void Test_transaction()
		{
			var eventFiredCounter = 0;
			var connectionState = ConnectionState.Open;

			using (var context = Context.UseTransaction(true).OnConnectionClosed(args => { eventFiredCounter++; connectionState = args.Connection.State; }))
			{
				context.Sql("select top 1 * from product").QueryMany<dynamic>();
				context.Sql("select top 1 * from product").QueryMany<dynamic>();
			}

			Assert.AreEqual(1, eventFiredCounter);
			Assert.AreEqual(ConnectionState.Closed, connectionState);
		}
	}
}
