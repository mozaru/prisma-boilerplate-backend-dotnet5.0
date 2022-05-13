using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace prisma.core
{
    public class StaticPagesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private string _dir;
        public StaticPagesMiddleware(RequestDelegate next, string Dir, ILoggerFactory loggerFactory)
        {
            _dir = Dir;
            _next = next;
            _logger = loggerFactory.CreateLogger<StaticPagesMiddleware>();
        }

        private string GetFileName(string requestPath)
        {
            if (requestPath == "/" || string.IsNullOrEmpty(requestPath))
                return System.IO.Path.Combine(_dir, "tela-home.html");
            return System.IO.Path.Combine(_dir, requestPath.Trim('/').Replace('/',System.IO.Path.DirectorySeparatorChar));
        }

        private bool isFileName(string requestPath)
        {
            return requestPath == "/" || string.IsNullOrEmpty(requestPath) || Path.GetExtension(requestPath).Length>1;
        }


        private bool IsStaticRequest(string requestPath, string requestMethod)
        {
            return requestMethod == "GET" && isFileName(requestPath);
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                string requestPath = context.Request?.Path.Value.ToLower();
                string requestMethod = context.Request?.Method.ToUpper();

                if (IsStaticRequest(requestPath, requestMethod))
                {
                    string FileName = GetFileName(requestPath);
                    if (System.IO.File.Exists(FileName))
                        await EnviarFile(context, FileName);
                    else
                        await BadRequest(context.Response);
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "StaticPagesMiddlewarecs");
                await BadRequest(context.Response, ex.Message);
            }
            finally
            {
                _logger.LogTrace(
                    "StaticPagesMiddlewarecs {Method} {Url} => {StatusCode}",
                    context.Request?.Method,
                    context.Request?.Path.Value,
                    context.Response?.StatusCode);
            }
        }

        private async Task EnviarFile(HttpContext context, string fileName)
        {
            byte[] content = await File.ReadAllBytesAsync(fileName);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.Body.WriteAsync(content, 0, content.Length);
        }

        private async Task BadRequest(HttpResponse response, string msg = "Count Request")
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            byte[] content = Encoding.UTF8.GetBytes(msg);
            await response.Body.WriteAsync(content, 0, content.Length);
        }
    }
}
