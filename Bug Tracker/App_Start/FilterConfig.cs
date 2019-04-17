using Bug_Tracker.Models.Filters;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ExceptionLogFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
