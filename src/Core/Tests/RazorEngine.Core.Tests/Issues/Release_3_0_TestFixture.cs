﻿namespace RazorEngine.Tests.TestTypes.Issues
{
    using System;
    using Microsoft.CSharp.RuntimeBinder;

    using NUnit.Framework;

    using Configuration;
    using Templating;

    /// <summary>
    /// Provides tests for the Release 3.0
    /// </summary>
    [TestFixture]
    public class Release_3_0_TestFixture
    {
        #region Tests
        /// <summary>
        /// When using a template layout, the model needs to be passed to the layout template from the child.
        /// 
        /// Issue 6: https://github.com/Antaris/RazorEngine/issues/6
        /// </summary>
        [Test]
        public void Issue6_ModelShouldBePassedToLayout()
        {
            using (var service = new TemplateService())
            {
                const string layoutTemplate = "<h1>@Model.PageTitle</h1> @RenderSection(\"Child\")";
                const string childTemplate = "@{ _Layout = \"Parent\"; }@section Child {<h2>@Model.PageDescription</h2>}";
                const string expected = "<h1>Test Page</h1> <h2>Test Page Description</h2>";

                var model = new {
                                    PageTitle = "Test Page",
                                    PageDescription = "Test Page Description"
                                };

                var type = model.GetType();

                service.Compile(layoutTemplate, type, "Parent");

                string result = service.Parse(childTemplate, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// A viewbag property is an easy way to share state between layout templates and the rendering template. The ViewBag property
        /// needs to persist from layouts and child templates.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        [Test]
        public void Issue7_ViewBagShouldPersistThroughLayout()
        {
            using (var service = new TemplateService())
            {
                const string layoutTemplate = "<h1>@ViewBag.Title</h1>@RenderSection(\"Child\")";
                const string childTemplate = "@{ _Layout =  \"Parent\"; ViewBag.Title = \"Test\"; }@section Child {}";

                service.Compile(layoutTemplate, "Parent");

                string result = service.Parse(childTemplate);

                Assert.That(result.StartsWith("<h1>Test</h1>"));
            }
        }

        /// <summary>
        /// A viewbag property should not persist through using @Include as this will create a new <see cref="ExecuteContext" />, which
        /// initialises a new viewbag property.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        [Test]
        public void Issue7_ViewBagShouldNotPersistThroughInclude_UsingCSharp()
        {
            using (var service = new TemplateService())
            {
                const string parentTemplate = "@{ ViewBag.Title = \"Test\"; }@Include(\"Child\")";
                const string childTemplate = "@ViewBag.Title";

                service.Compile(childTemplate, "Child");

                // The C# runtime binder will throw a RuntimeBinderException...
                Assert.Throws<RuntimeBinderException>(() =>
                {
                    string result = service.Parse(parentTemplate);
                });
            }
        }

        /// <summary>
        /// A viewbag property should not persist through using @Include as this will create a new <see cref="ExecuteContext" />, which
        /// initialises a new viewbag property.
        /// 
        /// Issue 7: https://github.com/Antaris/RazorEngine/issues/7
        /// </summary>
        [Test]
        public void Issue7_ViewBagShouldNotPersistThroughInclude_UsingVB()
        {
            var config = new TemplateServiceConfiguration() { Language = Language.VisualBasic };
            using (var service = new TemplateService(config))
            {
                const string parentTemplate = @"
@Code 
    ViewBag.Title = ""Test"" 
End Code
@Include(""Child"")";
                const string childTemplate = "@ViewBag.Title";

                service.Compile(childTemplate, "Child");

                // The VB runtime binder will through a MissingMemberException...
                Assert.Throws<MissingMemberException>(() =>
                {
                    string result = service.Parse(parentTemplate);
                });
            }
        }
        #endregion
    }
}