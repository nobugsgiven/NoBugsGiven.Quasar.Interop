using System.Security.Claims;
using System.Text.Json;

namespace NoBugsGiven.Quasar.Interop;

/// <summary>A capability exposed by a .NET host to a Quasar/Vue client. Capability IDs
/// and versions are stable protocol identifiers, not implementation type names.</summary>
public sealed record InteropCapabilityDescriptor(
    string Id,
    string Version,
    IReadOnlyList<string> Operations,
    string? Description = null);

/// <summary>A browser request for one explicitly named operation.</summary>
public sealed record InteropInvocationRequest(
    string CapabilityId,
    string Version,
    string Operation,
    JsonElement Arguments);

/// <summary>The user and request context passed to a registered capability handler.</summary>
public sealed record InteropInvocationContext(
    ClaimsPrincipal User,
    string RequestId);

/// <summary>Host-implemented capability. Implementations should expose narrow operations
/// with typed request/response JSON and must not expose ambient host services.</summary>
public interface IInteropCapability
{
    InteropCapabilityDescriptor Descriptor { get; }

    ValueTask<JsonElement> InvokeAsync(
        string operation,
        JsonElement arguments,
        InteropInvocationContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>Authorization hook for each capability operation. The default implementation
/// denies everything until the host supplies an explicit policy.</summary>
public interface IInteropCapabilityPolicy
{
    ValueTask<bool> IsAllowedAsync(
        ClaimsPrincipal user,
        string capabilityId,
        string operation,
        CancellationToken cancellationToken = default);
}

public sealed class InteropCapabilityException(string code, string message, int statusCode = 400)
    : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
