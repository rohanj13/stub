using Stub.Domain.Enums;

namespace Stub.Infrastructure.Providers.Abstractions;

public interface IPosTransactionAdapter
{
    Provider Provider { get; }

    Task<string> FetchTransactionAsync(string externalTransactionId, CancellationToken cancellationToken = default);
}
