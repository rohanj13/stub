using Stub.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Stub.Api.Tests;

public class ReceiptPlatformServiceTests
{
    private static ReceiptPlatformService CreateService()
    {
        var options = new DbContextOptionsBuilder<ReceiptPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ReceiptPlatformDbContext(options);
        return new ReceiptPlatformService(context);
    }

    [Fact]
    public void CreateMerchant_ConnectAndProcessTransaction_CreatesReceipt()
    {
        var service = CreateService();
        var merchant = service.CreateMerchant("Demo", "sq-account-1");

        var connected = service.ConnectMerchantToSquare(merchant.Id);

        var receipt = service.ProcessSquareTransaction(new SquareTransactionWebhook(
            merchant.Id,
            "txn-1",
            "order-1",
            15.99m,
            "usd",
            [new SquareTransactionItem("Coffee", 15.99m, 1)]));

        Assert.True(connected);
        Assert.Equal("USD", receipt.Currency);
        Assert.Equal(merchant.Id, receipt.MerchantId);
        Assert.Single(receipt.Items);
    }

    [Fact]
    public void AssignCustomerAndLookup_ByCustomerId_ReturnsReceipt()
    {
        var service = CreateService();
        var merchant = service.CreateMerchant("Demo", "sq-account-1");
        var receipt = service.ProcessSquareTransaction(new SquareTransactionWebhook(
            merchant.Id,
            "txn-2",
            null,
            8.50m,
            "USD",
            []));

        var assigned = service.AssignCustomerToReceipt(receipt.Id, "customer-123");
        var receipts = service.GetReceiptsByCustomer("customer-123");

        Assert.True(assigned);
        Assert.Single(receipts);
        Assert.Equal(receipt.Id, receipts.Single().Id);
    }

    [Fact]
    public void ProcessTransaction_WithUnknownMerchant_Throws()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() => service.ProcessSquareTransaction(new SquareTransactionWebhook(
            Guid.NewGuid(),
            "txn-3",
            null,
            9.99m,
            "USD",
            [])));
    }
}
