namespace dotnetcorev7.extensions
{
   using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
 using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;



namespace THSCMWeb.Extensions {
    public static class HtmlExtensions {

        public static IHtmlContent Script(this IHtmlHelper htmlHelper, Func<object, HelperResult> template)
        {

            htmlHelper.ViewContext.HttpContext.Items["_script_" + Guid.NewGuid()] = template;
            return HtmlString.Empty;

        }
        // public static IHtmlContent RadioYesNo (HtmlString sValue) {
        //     if (string.IsNullOrEmpty (sValue.ToString ())) {
        //         return sValue;
        //     }
        //     return new HtmlString (Convert.ToBoolean (sValue.ToString ()) ? _localizer["Yes"] : _localizer["No"]);
        // }

        public static IHtmlContent RadioYesNo (string bValue) {
            // return new HtmlString ((Convert.ToBoolean(bValue.ToString())  ? _localizer["Yes"].ToString() : _localizer["No"].ToString()));
            
             return new HtmlString ((Convert.ToBoolean(bValue.ToString()))  ? "Yes" :"No");
        }


        #region  Partial for

        public static IHtmlContent ParseHTML(this IHtmlHelper helper, string htmlString)
        {
            return new HtmlString(htmlString);
        }

        public static IHtmlContent PartialWithPrefix(this IHtmlHelper htmlHelper, string partialViewName, object model, string prefix)
        {
            var htmlFieldPrefix = (string.Empty.Equals(prefix) ? "." : "") + prefix;
            return htmlHelper.Partial(partialViewName, model, new ViewDataDictionary(htmlHelper.ViewData) { TemplateInfo = { HtmlFieldPrefix = htmlFieldPrefix } });
        }

        public static Task<IHtmlContent> PartialWithPrefixAsync(this IHtmlHelper htmlHelper, string partialViewName, object model, string prefix)
        {
            var htmlFieldPrefix = (string.Empty.Equals(prefix) ? "." : "") + prefix;
            return htmlHelper.PartialAsync(partialViewName, model, new ViewDataDictionary(htmlHelper.ViewData) { TemplateInfo = { HtmlFieldPrefix = htmlFieldPrefix } });
        }

        public static IHtmlContent PartialWithPrefixFor<TModel, TProperty>(this IHtmlHelper<TModel> helper, 
        Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string prefix = ExpressionHelper.GetExpressionText(expression);
            object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;
            return PartialWithPrefix(helper, partialViewName, model, prefix);
        }

        public static Task<IHtmlContent> PartialWithPrefixForAsync<TModel, TProperty>(this IHtmlHelper<TModel> helper,
         Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            string prefix = ExpressionHelper.GetExpressionText(expression);
            object model = ExpressionMetadataProvider.FromLambdaExpression(expression, helper.ViewData, helper.MetadataProvider).Model;
            return PartialWithPrefixAsync(helper, partialViewName, model, prefix);
        }
       
        #endregion
        #region  Render Action


        public static IHtmlContent RenderAction(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(controller));
            if (controller == null)
                throw new ArgumentNullException(nameof(action));

            var task = RenderActionAsync(helper, action, controller, area, parameters);
            return task.Result;
        }

        private static async Task<IHtmlContent> RenderActionAsync(this IHtmlHelper helper, string action, string controller, string area, object parameters = null)
        {
            // fetching required services for invocation
            var currentHttpContext = helper.ViewContext.HttpContext;
            var httpContextFactory = GetServiceOrFail<IHttpContextFactory>(currentHttpContext);
            var actionInvokerFactory = GetServiceOrFail<IActionInvokerFactory>(currentHttpContext);
            var actionSelector = GetServiceOrFail<IActionDescriptorCollectionProvider>(currentHttpContext);

            // creating new action invocation context
            var routeData = new RouteData();
            var routeParams = new RouteValueDictionary(parameters ?? new { });
            var routeValues = new RouteValueDictionary(new { area, controller, action });
            var newHttpContext = httpContextFactory.Create(currentHttpContext.Features);

            newHttpContext.Response.Body = new MemoryStream();

            foreach (var router in helper.ViewContext.RouteData.Routers)
                routeData.PushState(router, null, null);

            routeData.PushState(null, routeValues, null);
            routeData.PushState(null, routeParams, null);

            var actionDescriptor = actionSelector.ActionDescriptors.Items.First(i => i.RouteValues["Controller"] == controller && i.RouteValues["Action"] == action);
            var actionContext = new ActionContext(newHttpContext, routeData, actionDescriptor);

            // invoke action and retreive the response body
            var invoker = actionInvokerFactory.CreateInvoker(actionContext);
            string content = null;

            await invoker.InvokeAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    content = task.Exception.Message;
                }
                else if (task.IsCompleted)
                {
                    newHttpContext.Response.Body.Position = 0;
                    using (var reader = new StreamReader(newHttpContext.Response.Body))
                        content = reader.ReadToEnd();
                }
            });

            return new HtmlString(content);
        }

        private static TService GetServiceOrFail<TService>(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var service = httpContext.RequestServices.GetService(typeof(TService));

            if (service == null)
                throw new InvalidOperationException($"Could not locate service: {nameof(TService)}");

            return (TService)service;
        }
        #endregion
        public static IHtmlContent RadioLimitGender (HtmlString sValue) {
            string GenderName = "Unlimit" ;
            if (sValue.ToString () == "M") {
                GenderName = "LimitMale" ;
            } else if (sValue.ToString () == "F") {
                GenderName = "LimitFemale" ;
            }

            return new HtmlString (GenderName);
        }
        
        #region BuildOrgTree

      
      

        #endregion BuildOrgTree

   

       

    }
}
}