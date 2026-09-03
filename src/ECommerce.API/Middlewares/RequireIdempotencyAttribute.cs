using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Middlewares;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireIdempotencyAttribute : Attribute, IAsyncActionFilter
{
    private const string IdempotencyHeader = "Idempotency-Key";
    public bool Required { get; set; } = true;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        if (!request.Headers.TryGetValue(IdempotencyHeader, out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
        {
            if (Required)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    success = false,
                    message = $"The '{IdempotencyHeader}' HTTP header is required for this operation to guarantee safe retryability."
                });
                return;
            }

            await next();
            return;
        }

        var idempotencyKey = headerValue.ToString().Trim();
        var idempotencyService = httpContext.RequestServices.GetRequiredService<IIdempotencyService>();

        // Enable buffering to read request body and hash it
        request.EnableBuffering();
        string requestBody;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var requestHash = ComputeSha256(requestBody);
        var checkResult = await idempotencyService.CheckAsync(
            idempotencyKey,
            request.Path,
            request.Method,
            requestHash,
            httpContext.RequestAborted);

        if (checkResult.Exists)
        {
            if (checkResult.HashMismatch)
            {
                context.Result = new UnprocessableEntityObjectResult(new
                {
                    success = false,
                    message = "The provided Idempotency-Key has already been used with a different request payload."
                });
                return;
            }

            if (checkResult.IsProcessing)
            {
                context.Result = new ConflictObjectResult(new
                {
                    success = false,
                    message = "A request with this Idempotency-Key is currently being processed. Please retry in a few moments."
                });
                return;
            }

            if (checkResult.StatusCode.HasValue && !string.IsNullOrEmpty(checkResult.ResponseBody))
            {
                httpContext.Response.Headers["X-Cache-Lookup"] = "HIT-IDEMPOTENT";
                httpContext.Response.ContentType = "application/json";
                context.Result = new ContentResult
                {
                    StatusCode = checkResult.StatusCode.Value,
                    Content = checkResult.ResponseBody,
                    ContentType = "application/json"
                };
                return;
            }
        }

        // Register in-flight execution
        await idempotencyService.RegisterProcessingAsync(
            idempotencyKey,
            request.Path,
            request.Method,
            requestHash,
            cancellationToken: httpContext.RequestAborted);

        var executedContext = await next();

        if (executedContext.Exception != null && !executedContext.ExceptionHandled)
        {
            await idempotencyService.MarkFailedAsync(idempotencyKey, httpContext.RequestAborted);
            return;
        }

        // Capture response
        if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode.HasValue)
        {
            var json = JsonSerializer.Serialize(objectResult.Value);
            await idempotencyService.SaveResponseAsync(idempotencyKey, objectResult.StatusCode.Value, json, httpContext.RequestAborted);
        }
        else if (executedContext.Result is StatusCodeResult statusCodeResult)
        {
            await idempotencyService.SaveResponseAsync(idempotencyKey, statusCodeResult.StatusCode, string.Empty, httpContext.RequestAborted);
        }
    }

    private static string ComputeSha256(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
