﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scryber.Components;
using Scryber.Styles;
using Scryber.Drawing;

namespace Scryber.UnitSamples
{
    [TestClass]
    public class LinkTests : TestBase
    {

        #region public TestContext TestContext

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        [TestMethod()]
        public void SimpleNavigationLinks()
        {
            var path = GetTemplatePath("Links", "LinksSimple.html");

            using (var doc = Document.ParseDocument(path))
            {
                var pages = new [] { new { Id = "first" }, new { Id = "second" }, new { Id = "third" }, new { Id = "fourth" } };
                doc.Params["pages"] = pages;

                using (var stream = GetOutputStream("Links", "LinksSimple.pdf"))
                {
                    doc.SaveAsPDF(stream);
                }

            }
        }

        [TestMethod()]
        public void StyledFooterNavigationLinks()
        {
            var path = GetTemplatePath("Links", "LinksStyledFooter.html");

            using (var doc = Document.ParseDocument(path))
            {
                var pages = new[] { new { Id = "first" }, new { Id = "second" }, new { Id = "third" }, new { Id = "fourth" } };
                doc.Params["pages"] = pages;

                using (var stream = GetOutputStream("Links", "LinksStyledFooter.pdf"))
                {
                    doc.SaveAsPDF(stream);
                }

            }
        }
    }
}