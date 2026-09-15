using Stub.Domain.Enums;

namespace Stub.Infrastructure.Providers.Abstractions;

public interface IPosTransactionAdapterResolver
{
    IPosTransactionAdapter GetRequiredAdapter(Provider provider);
}
