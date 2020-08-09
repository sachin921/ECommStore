﻿using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Filters
{
    public class CustomerLastActivityAttribute : FilterAttribute, IActionFilter
    {
		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<ICustomerService> CustomerService { get; set; }
		
		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            if (filterContext?.HttpContext?.Request == null)
                return;

            if (filterContext.IsChildAction)
                return;

            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                return;

            var customer = WorkContext.Value.CurrentCustomer;
            
            // update last activity date
            if (!customer.IsSystemAccount && customer.LastActivityDateUtc.AddMinutes(1.0) < DateTime.UtcNow)
            {
                customer.LastActivityDateUtc = DateTime.UtcNow;
				CustomerService.Value.UpdateCustomer(customer);
            }
        }

		public virtual void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}
