using Stub.Domain.Enums;

namespace Stub.Infrastructure.Providers.Abstractions;

public class PosTransactionAdapterResolver : IPosTransactionAdapterResolver
{
    private readonly IReadOnlyDictionary<Provider, IPosTransactionAdapter> _adapters;

    public PosTransactionAdapterResolver(IEnumerable<IPosTransactionAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(adapter => adapter.Provider);
    }

    public IPosTransactionAdapter GetRequiredAdapter(Provider provider)
    {
        if (_adapters.TryGetValue(provider, out var adapter))
        {
            return adapter;
        }

        throw new NotSupportedException($"No POS adapter has been configured for provider '{provider}'.");
    }
}
