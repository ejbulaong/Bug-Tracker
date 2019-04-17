using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker.Models.Filters
{
    public class ExceptionLogFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            var log = new ExceptionLog();
            log.Message = filterContext.Exception.Message;
            log.DateTime = DateTime.Now;

            var dbContext = new ApplicationDbContext();

            dbContext.ExceptionLogs.Add(log);
            dbContext.SaveChanges();

            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult()
            {
                ViewName = "Error"
            };
        }
    }
}