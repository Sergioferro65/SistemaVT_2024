
using BugsMVC.DAL;
using BugsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugsMVC.Handlers
{
    public class AuditAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Stores the Request in an Accessible object
            var request = filterContext.HttpContext.Request;
            // Generate an audit
            Auditoria auditoria = new Auditoria()
            {
                // Your Audit Identifier     
                AuditoriaID = Guid.NewGuid(),
                // Our Username (if available)
                UserName = (request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name : "Anonymous",
                // The IP Address of the Request
                IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress,
                // The URL that was accessed
                AreaAccessed = request.RawUrl,
                // Creates our Timestamp
                TimeAccessed = DateTime.Now
            };

            // Stores the Audit in the Database
            BugsContext context = new BugsContext();
            context.RegistrosAuditoria.Add(auditoria);
            context.SaveChanges();

            // Finishes executing the Action as normal 
            base.OnActionExecuting(filterContext);
        }
    }
}