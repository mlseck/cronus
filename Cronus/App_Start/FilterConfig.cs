﻿using Cronus.Login;
using System.Web;
using System.Web.Mvc;

namespace Cronus
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LogonAuthorize());
        }
    }
}
