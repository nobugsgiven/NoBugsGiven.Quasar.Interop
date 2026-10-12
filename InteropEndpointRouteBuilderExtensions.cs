using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace NoBugsGiven.Quasar.Interop;

public static class InteropEndpointRouteBuilderExtensions
{
    /// <summary>Map the versioned JSON bridge. The endpoint group requires authentication;
    /// individual operations are then checked by the configured default-deny policy.</summary>
    public static RouteGroupBuilder MapQuasarInterop(
        this IEndpointRouteBuilder endpoints,
        string prefix = "/api/interop/v1")
    {
        var group = endpoints.MapGroup(prefix).RequireAuthorization();

        group.MapGet("/capabilities", async (
            HttpContext context,
            InteropCapabilityRegistry registry,
            CancellationToken cancellationToken) =>
        {
            var capabilities = await registry.ListAllowedAsync(context.User, cancellationToken);
            return Results.Ok(new { capabilities });
        });

        group.MapPost("/invoke", async (
            HttpContext context,
            InteropInvocationRequest request,
            InteropCapabilityRegistry registry,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var requestId = context.TraceIdentifier;
                var result = await registry.InvokeAsync(context.User, requestId, request, cancellationToken);
                return Results.Ok(new { requestId, result });
            }
            catch (InteropCapabilityException exception)
            {
                return Results.Problem(
                    title: exception.Code,
                    detail: exception.Message,
                    statusCode: exception.StatusCode,
                    extensions: new Dictionary<string, object?> { ["code"] = exception.Code });
            }
        });

        return group;
    }
}
