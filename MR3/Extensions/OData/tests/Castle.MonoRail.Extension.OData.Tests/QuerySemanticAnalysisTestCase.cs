﻿namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class QuerySemanticAnalysisTestCase
	{
		private StubModel _model;

		[SetUp]
		public void Init()
		{
			_model = new StubModel(
				m => m.EntitySet("catalogs", new List<Catalog2>().AsQueryable())
			);
		}

		[Test]
		public void JustRunning()
		{
			// filter = Customers/ContactName ne 'Fred'
			// var exp0 = QueryExpressionParser.parse("1 add 2 mul 3");
			// Console.WriteLine(exp0.ToStringTree());

			// var exp1 = QueryExpressionParser.parse("Name ne 'Cat1'");
			// Console.WriteLine(exp1.ToStringTree());

			// var exp2 = QueryExpressionParser.parse("Customers/N ne 'Fred'");
			// Console.WriteLine(exp2.ToStringTree());
			//
			// var exp3 = QueryExpressionParser.parse("Customers/Address/Street ne 'Fred'");
			// Console.WriteLine(exp3.ToStringTree());

			var exp4 = QueryExpressionParser.parse("Owner/Email eq 'John'");
			Console.WriteLine(exp4.ToStringTree());

			var tree = QuerySemanticAnalysis.analyze_and_convert(exp4, _model.GetResourceType("Catalog2").Value);
			Console.WriteLine(tree.ToStringTree());
		}

		public class Product2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class User2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public string Email { get; set; }
		}

		public class Catalog2
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product2> Products { get; set; }
			public User2 Owner { get; set; }
		}
	}
}
