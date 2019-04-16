using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker.Models.Filters
{
    public class UserLogFilters : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var context = new ApplicationDbContext();
            var user = filterContext.HttpContext.User.Identity.Name;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var log = new UserLog();
                log.UserName = user;

                log.ActionName = filterContext
                    .ActionDescriptor
                    .ActionName;

                log.ControllerName = filterContext
                    .ActionDescriptor
                    .ControllerDescriptor
                    .ControllerName;

                log.Time = DateTime.Now;

                context.UserLogs.Add(log);
                context.SaveChanges();
            }
        }
    }
}