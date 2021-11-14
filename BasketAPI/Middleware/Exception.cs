using Core.Exception;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasketAPI.Middleware
{
    public class Middleware
    {
        private readonly RequestDelegate next;
        private ILogger logger = Log.ForContext<Middleware>();

        private static readonly HashSet<string> CorsHeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HeaderNames.AccessControlAllowCredentials,
            HeaderNames.AccessControlAllowHeaders,
            HeaderNames.AccessControlAllowMethods,
            HeaderNames.AccessControlAllowOrigin,
            HeaderNames.AccessControlExposeHeaders,
            HeaderNames.AccessControlMaxAge,
        };

        public Middleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException validationException)
            {
                var validationMesage = string.Empty;
                if (validationException.Errors.Any())
                {
                    validationMesage = string.Join(",", validationException.Errors.Select(q => q.ErrorMessage));
                }
                else
                {
                    validationMesage = "Requesr error.";
                }


                await WriteLog(context, validationException, StatusCodes.Status400BadRequest).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, validationMesage, StatusCodes.Status400BadRequest).ConfigureAwait(false);
            }
            catch (NotFound customException)
            {
                await WriteLog(context, customException, StatusCodes.Status404NotFound).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, customException.Message, statusCode: StatusCodes.Status404NotFound).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await WriteLog(context, ex, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, "Server Internal Error", StatusCodes.Status500InternalServerError).ConfigureAwait(false);
            }
        }

        private static async Task ClearResponseAndBuildErrorDto(HttpContext context, string message, int statusCode)
        {
            var headers = new HeaderDictionary();

            // Make sure problem responses are never cached.
            headers.Append(HeaderNames.CacheControl, "no-cache, no-store, must-revalidate");
            headers.Append(HeaderNames.Pragma, "no-cache");
            headers.Append(HeaderNames.Expires, "0");

            foreach (var header in context.Response.Headers)
            {
                // Because the CORS middleware adds all the headers early in the pipeline,
                // we want to copy over the existing Access-Control-* headers after resetting the response.
                if (CorsHeaderNames.Contains(header.Key))
                {
                    headers.Add(header);
                }
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            foreach (var header in headers)
            {
                context.Response.Headers.Add(header);
            }

            await WriteResultAsync(context, new ObjectResult(message));
        }

        private async Task WriteLog(HttpContext context, Exception exception, int statusCode)
        {
            var request = context.Request;
            var requestPathAndQuery = request.GetEncodedPathAndQuery();

            logger = logger.ForContext("MachineName", Environment.MachineName)
                .ForContext("RequestHost", request.Host.Host)
                .ForContext("RequestProtocol", request.Protocol)
                .ForContext("RequestMethod", request.Method)
                .ForContext("ResponseStatusCode", statusCode)
                .ForContext("RequestPath", request.Path)
                .ForContext("RequestPathAndQuery", requestPathAndQuery)
                .ForContext("Exception", exception, true)
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => (object)h.Value.ToString()), true)
                .ForContext("Exception", exception, true);

            var errorTemplate = $"HTTP {request.Method} {requestPathAndQuery} responded {statusCode}";

            logger.Error(exception, errorTemplate);

            await Task.FromResult(true);
        }


        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        public static Task WriteResultAsync<TResult>(HttpContext context, TResult result) where TResult : IActionResult
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.RequestServices.GetService<IActionResultExecutor<TResult>>();

            if (executor == null)
            {
                throw new InvalidOperationException($"No result executor for '{typeof(TResult).FullName}' has been registered.");
            }

            var routeData = context.GetRouteData() ?? EmptyRouteData;

            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }
    }
}
