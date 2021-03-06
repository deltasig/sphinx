﻿namespace Dsp.Web
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    public static class HtmlHelpers
    {
        public static MvcHtmlString EmbedCss(this HtmlHelper htmlHelper, string path)
        {
            // take a path that starts with "~" and map it to the filesystem.
            var cssFilePath = HttpContext.Current.Server.MapPath(path);
            // load the contents of that file
            try
            {
                var cssText = System.IO.File.ReadAllText(cssFilePath);
                var styleElement = new TagBuilder("style");
                styleElement.SetInnerText(cssText);
                return MvcHtmlString.Create(styleElement.ToString());
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                // return nothing if we can't read the file for any reason
                return null;
            }
        }
    }
}