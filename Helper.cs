﻿using System.IO;
using System.Web.Mvc;

public static class Helper
{
    public static string RenderPartialViewToString(Controller controller, string viewName, object model)
    {
        controller.ViewData.Model = model;

        using (var sw = new StringWriter())
        {
            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
            var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
            viewResult.View.Render(viewContext, sw);
            return sw.GetStringBuilder().ToString();
        }
    }
}
