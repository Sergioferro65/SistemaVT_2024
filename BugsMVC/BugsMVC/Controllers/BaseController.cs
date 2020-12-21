using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugsMVC.Controllers
{
    public class BaseController : Controller
    {
        #region Private Members

        public ILog Logger { get; set; }

        #endregion

        protected override void Initialize(RequestContext requestContext)
        {
            Logger = log4net.LogManager.GetLogger(this.GetType());
            //Repository.CurrentUserId = CurrentUserId;
            base.Initialize(requestContext);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public ContentResult IsAlive()
        //{
        //    return Content(HttpStatusCode.OK);
        //}

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

            //if (this.CurrentUser != null)
            //{
            //    Logger.Debug(string.Format("Executing Controller: {0} Action: {1} Parameters: {2} UserId: {3} Url: {4}",
            //                    filterContext.Controller, filterContext.ActionDescriptor.ActionName, parameterInformation.ToString(),
            //                    this.CurrentUser.Id, filterContext.HttpContext.Request.Url));
            //}
            //else
            //{
            Logger.Debug(string.Format("Executing Controller: {0} Action: {1} Parameters: {2} Url: {3}",
                                filterContext.Controller, filterContext.ActionDescriptor.ActionName, parameterInformation.ToString(),
                                filterContext.HttpContext.Request.Url));
            //}

            //if (CurrentUser != null)
            //{
            //    //var notificationCommand = new GetNotificationsByUserCommand();
            //    NotificationsCommand.Configure(CurrentUser.Id, getAll: true).Execute();

            //    var notificationsUnchecked = NotificationsCommand.Result
            //        .Where(x => !x.Checked)
            //        .OrderByDescending(x => x.Date);

            //    ViewBag.Notifications = notificationsUnchecked
            //        .Take(5)
            //        .ToList()
            //        .Select(x => NotificationViewModel.From(x))
            //        .OrderBy(x => x.MinutesFromNow)
            //        .ToList();

            //    ViewBag.NotificationsTotalCount = notificationsUnchecked.Count();
            //}

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