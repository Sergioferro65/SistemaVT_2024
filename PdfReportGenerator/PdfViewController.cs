// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PdfViewController.cs" company="SemanticArchitecture">
//   http://www.SemanticArchitecture.net pkalkie@gmail.com
// </copyright>
// <summary>
//   Extends the controller with functionality for rendering PDF views
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ReportManagement
{
    using log4net;
    using System;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Extends the controller with functionality for rendering PDF views
    /// </summary>
    public class PdfViewController : Controller
    {
        private readonly HtmlViewRenderer htmlViewRenderer;
        private readonly StandardPdfRenderer standardPdfRenderer;

        public ILog Logger { get; set; }

        public PdfViewController()
        {
            this.htmlViewRenderer = new HtmlViewRenderer();
            this.standardPdfRenderer = new StandardPdfRenderer();
        }

        protected override void Initialize(RequestContext requestContext)
        {
            Logger = log4net.LogManager.GetLogger(this.GetType());
            //Repository.CurrentUserId = CurrentUserId;
            base.Initialize(requestContext);
        }

        protected ActionResult ViewPdf(string pageTitle, string viewName, object model)
        {
            // Render the view html to a string.
            string htmlText = this.htmlViewRenderer.RenderViewToString(this, viewName, model);

            // Let the html be rendered into a PDF document through iTextSharp.
            byte[] buffer = standardPdfRenderer.Render(htmlText, pageTitle);

            // Return the PDF as a binary stream to the client.
            return new BinaryContentResult(buffer, "application/pdf");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            StringBuilder parameterInformation = new StringBuilder();
            foreach (var parameter in filterContext.ActionParameters)
            {
                parameterInformation.AppendFormat("Key: {0} Value: {1} ", parameter.Key, parameter.Value);

                // Para no loguear las imagenes que se suben al sitio (sino logueamos toda la imagen y ocupa mucho espacio sin sentido)
                if (parameter.Key != "uploadedFileMeta" && parameter.Key != "imageType")
                {
                    var stream = filterContext.HttpContext.Request.InputStream;
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    parameterInformation.AppendFormat("Key: {0} Value: {1}", parameter.Key, Encoding.UTF8.GetString(data));
                }
            }

            Logger.Debug(string.Format("Executing Controller: {0} Action: {1} Parameters: {2} Url: {3}",
                                filterContext.Controller, filterContext.ActionDescriptor.ActionName, parameterInformation.ToString(),
                                filterContext.HttpContext.Request.Url));

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Logger.Debug(string.Format("Executed Controller: {0} Action: {1}", filterContext.Controller, filterContext.ActionDescriptor.ActionName));

            if (filterContext.Exception != null)
            {
                LogException(filterContext.Exception);
            }

            base.OnActionExecuted(filterContext);
        }

        private void LogException(Exception exception)
        {
            Logger.Error(string.Format("Exception: {0}. Stack Trace: {1}", exception.Message, exception.StackTrace, exception));
        }
    }
}