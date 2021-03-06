// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MonoRail.Views.AspView.Tests.ViewTests
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Configuration;
	using Framework;
	using Framework.Services;
	using NUnit.Framework;

	[TestFixture]
	public class ViewPerformanceFixture : AbstractViewTestFixture
	{
		public interface IPrimaryLayout { }
		public interface ISecondaryLayout { }
		public interface IMainView { }
		public interface ISecondaryView { }
		public interface ITernaryView { }
		public class PrimaryLayout : EmptyView<IPrimaryLayout>
		{
			public override void Render()
			{
				OutputLine("Primary layout start");
				OutputLine("Top capture:");
				OutputLine(Properties["TopCapture"]);
				OutputLine("Top capture end.");
				OutputLine(ViewContents);
				OutputLine("Bottom capture:");
				OutputLine(Properties["BottomCapture"]);
				OutputLine("Bottom capture end:");
				OutputLine("Primary layout end");
			}
		}
		public class SecondaryLayout : EmptyView<ISecondaryLayout>
		{
			public override void Render()
			{
				OutputLine("Secondary layout start");
				OutputLine(ViewContents);
				OutputLine("Secondary layout end");
			}
		}

		public class MainView : EmptyView<IMainView>
		{
			public override void Render()
			{
				OutputLine("Main view start");
				for (var i = 0; i < 100; ++i)
					OutputSubView("SecondaryView", N("Val", i));
				OutputLine("Main view end");
			}
		}

		public class SecondaryView : EmptyView<ISecondaryView>
		{
			public override void Render()
			{
				OutputLine("Secondary view start");
				OutputLine("Val = " + Properties["Val"]);
				for (var i = 0; i < 3; ++i)
					OutputSubView("TrenaryView", N("InnerVal", i));
				OutputLine("Secondary view end");
				InvokeViewComponent("CaptureFor", N("id", "TopCapture").N("append", "after"), () => Output("Data" + Properties["Val"]), null);
				InvokeViewComponent("CaptureFor", N("id", "BottomCapture").N("append", "before"), () => Output("Data" + Properties["Val"]), null);
			}
		}

		public class TrenaryView : EmptyView<ITernaryView>
		{
			public override void Render()
			{
				OutputLine("Trenary view start");
				OutputLine("Val = " + Properties["Val"]);
				OutputLine("InnerVal = " + Properties["InnerVal"]);
				OutputLine("Trenary view end");
			}
		}

		public abstract class EmptyView<T> : AspViewBase<T>
		{
			protected override string ViewDirectory
			{
				get { return ""; }
			}

			protected override string ViewName
			{
				get { return ""; }
			}

			protected void OutputLine(object s)
			{
				Output("" + s + Environment.NewLine);
			}
		}
		[Test]
		public void InitializeProperties()
		{

			var viewComponentFactory = new DefaultViewComponentFactory();
			viewComponentFactory.Service(context);
			context.AddService(typeof(IViewComponentFactory), viewComponentFactory);
			engine.Options.ViewProperties = ViewPropertiesInclusionOptions.QueryString;
			controllerContext.LayoutNames = new[] { "PrimaryLayout", "SecondaryLayout" };

			AddCompilation("MainView", typeof(MainView));
			AddCompilation("Layouts_PrimaryLayout", typeof(PrimaryLayout));
			AddCompilation("Layouts_SecondaryLayout", typeof(SecondaryLayout));
			AddCompilation("SecondaryView", typeof(SecondaryView));
			AddCompilation("TrenaryView", typeof(TrenaryView));

			FillPropertyBag(300, context.Request.Params);
			FillPropertyBag(4, context.Request.QueryString);
			FillPropertyBag(40, controllerContext.PropertyBag);

			var outputString = new StringBuilder();
			var stopWatch = Stopwatch.StartNew();
			for (var i = 0; i < 20; ++i)
				engine.Process("MainView", new StringWriter(outputString), context, controller, controllerContext);
			stopWatch.Stop();

			var elapsed = stopWatch.ElapsedMilliseconds;
			Console.WriteLine(elapsed / 20);
		}

		static void FillPropertyBag(int count, IDictionary properties)
		{
			var props = from k in Enumerable.Range(0, count).Select(i => "Key" + i) select k;
			foreach (var prop in props)
			{
				properties[prop] = "Value of " + prop;
			}
		}
		static void FillPropertyBag(int count, NameValueCollection properties)
		{
			var props = from k in Enumerable.Range(0, count).Select(i => "Key" + i) select k;
			foreach (var prop in props)
			{
				properties[prop] = "Value of " + prop;
			}
		}
	}
}