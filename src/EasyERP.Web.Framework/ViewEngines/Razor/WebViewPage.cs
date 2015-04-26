﻿namespace EasyERP.Web.Framework.ViewEngines.Razor
{
    using System;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.WebPages;
    using EasyErp.Core;
    using EasyErp.Core.Infrastructure;

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        public IWorkContext WorkContext { get; private set; }

        public override string Layout
        {
            get
            {
                var layout = base.Layout;

                if (!string.IsNullOrEmpty(layout))
                {
                    var filename = Path.GetFileNameWithoutExtension(layout);
                    var viewResult = ViewEngines.Engines.FindView(
                        ViewContext.Controller.ControllerContext,
                        filename,
                        "");

                    if (viewResult.View != null &&
                        viewResult.View is RazorView)
                    {
                        layout = (viewResult.View as RazorView).ViewPath;
                    }
                }

                return layout;
            }
            set { base.Layout = value; }
        }

        public override void InitHelpers()
        {
            base.InitHelpers();

            WorkContext = EngineContext.Current.Resolve<IWorkContext>();
        }

        public HelperResult RenderWrappedSection(string name, object wrapperHtmlAttributes)
        {
            Action<TextWriter> action = delegate(TextWriter tw)
            {
                var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(wrapperHtmlAttributes);
                var tagBuilder = new TagBuilder("div");
                tagBuilder.MergeAttributes(htmlAttributes);

                var section = RenderSection(name, false);
                if (section != null)
                {
                    tw.Write(tagBuilder.ToString(TagRenderMode.StartTag));
                    section.WriteTo(tw);
                    tw.Write(tagBuilder.ToString(TagRenderMode.EndTag));
                }
            };
            return new HelperResult(action);
        }

        public HelperResult RenderSection(string sectionName, Func<object, HelperResult> defaultContent)
        {
            return IsSectionDefined(sectionName) ? RenderSection(sectionName) : defaultContent(new object());
        }

        /// <summary>
        /// Return a value indicating whether the working language and theme support RTL (right-to-left)
        /// </summary>
        /// <returns></returns>
        public bool ShouldUseRtlTheme()
        {
            return false;
        }

        /// <summary>
        /// Gets a selected tab index (used in admin area to store selected tab index)
        /// </summary>
        /// <returns>Index</returns>
        public int GetSelectedTabIndex()
        {
            //keep this method synchornized with
            //"SetSelectedTabIndex" method of \Administration\Controllers\BaseNopController.cs
            var index = 0;
            var dataKey = "nop.selected-tab-index";
            if (ViewData[dataKey] is int)
            {
                index = (int)ViewData[dataKey];
            }
            if (TempData[dataKey] is int)
            {
                index = (int)TempData[dataKey];
            }

            //ensure it's not negative
            if (index < 0)
            {
                index = 0;
            }

            return index;
        }
    }

    public abstract class WebViewPage : WebViewPage<dynamic>
    {
    }
}