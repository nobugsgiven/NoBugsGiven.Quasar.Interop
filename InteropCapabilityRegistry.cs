using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NoBugsGiven.Quasar.Interop;

/// <summary>Dispatches only registered, explicitly authorized capabilities. There is no
/// reflection-based route to arbitrary .NET methods or services.</summary>
public sealed class InteropCapabilityRegistry(
    IEnumerable<IInteropCapability> capabilities,
    IInteropCapabilityPolicy policy)
{
    private readonly IReadOnlyDictionary<string, IInteropCapability> _capabilities =
        capabilities.ToDictionary(capability => capability.Descriptor.Id, StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<InteropCapabilityDescriptor>> ListAllowedAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var allowed = new List<InteropCapabilityDescriptor>();
        foreach (var capability in _capabilities.Values)
        {
            var operations = new List<string>();
            foreach (var operation in capability.Descriptor.Operations)
            {
                if (await policy.IsAllowedAsync(user, capability.Descriptor.Id, operation, cancellationToken))
                    operations.Add(operation);
            }
            if (operations.Count > 0)
                allowed.Add(capability.Descriptor with { Operations = operations });
        }
        return allowed.OrderBy(item => item.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async ValueTask<JsonElement> InvokeAsync(
        ClaimsPrincipal user,
        string requestId,
        InteropInvocationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_capabilities.TryGetValue(request.CapabilityId, out var capability))
            throw new InteropCapabilityException("interop.capability.not_found", "The requested capability is not available.", 404);
        if (!string.Equals(capability.Descriptor.Version, request.Version, StringComparison.Ordinal))
            throw new InteropCapabilityException("interop.capability.version_mismatch", "The requested capability version is not available.", 409);
        if (!capability.Descriptor.Operations.Contains(request.Operation, StringComparer.Ordinal))
            throw new InteropCapabilityException("interop.operation.not_found", "The requested operation is not available.", 404);
        if (!await policy.IsAllowedAsync(user, request.CapabilityId, request.Operation, cancellationToken))
            throw new InteropCapabilityException("interop.capability.denied", "The requested capability is not granted.", 403);

        return await capability.InvokeAsync(
            request.Operation,
            request.Arguments,
            new InteropInvocationContext(user, requestId),
            cancellationToken);
    }
}

public sealed class DenyAllInteropCapabilityPolicy : IInteropCapabilityPolicy
{
    public ValueTask<bool> IsAllowedAsync(
        ClaimsPrincipal user,
        string capabilityId,
        string operation,
        CancellationToken cancellationToken = default) => ValueTask.FromResult(false);
}

public static class InteropServiceCollectionExtensions
{
    public static IServiceCollection AddQuasarInterop(this IServiceCollection services)
    {
        services.TryAddSingleton<IInteropCapabilityPolicy, DenyAllInteropCapabilityPolicy>();
        services.TryAddSingleton<InteropCapabilityRegistry>();
        return services;
    }
}
