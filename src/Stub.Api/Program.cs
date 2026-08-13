using Stub.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ReceiptPlatformService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/merchants", (ReceiptPlatformService service) => Results.Ok(service.GetMerchants()));

app.MapPost("/api/merchants", (ReceiptPlatformService service, CreateMerchantRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.PosAccountId))
    {
        return Results.BadRequest("Name and POS account id are required.");
    }

    var merchant = service.CreateMerchant(request.Name, request.PosAccountId);
    return Results.Created($"/api/merchants/{merchant.Id}", merchant);
});

app.MapPost("/api/merchants/{merchantId:guid}/connect/square", (ReceiptPlatformService service, Guid merchantId) =>
{
    return service.ConnectMerchantToSquare(merchantId)
        ? Results.Ok(new { merchantId, webhookRegistered = true })
        : Results.NotFound();
});

app.MapPost("/api/webhooks/square/transactions", (ReceiptPlatformService service, SquareTransactionWebhook payload) =>
{
    if (string.IsNullOrWhiteSpace(payload.TransactionId) || string.IsNullOrWhiteSpace(payload.Currency))
    {
        return Results.BadRequest("TransactionId and currency are required.");
    }

    try
    {
        var receipt = service.ProcessSquareTransaction(payload);
        return Results.Created($"/api/receipts/{receipt.Id}", receipt);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound("Merchant is not registered.");
    }
});

app.MapGet("/api/receipts/{receiptId:guid}", (ReceiptPlatformService service, Guid receiptId) =>
{
    var receipt = service.GetReceipt(receiptId);
    return receipt is null ? Results.NotFound() : Results.Ok(receipt);
});

app.MapPost("/api/receipts/{receiptId:guid}/assign-customer", (ReceiptPlatformService service, Guid receiptId, AssignCustomerRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerId))
    {
        return Results.BadRequest("CustomerId is required.");
    }

    return service.AssignCustomerToReceipt(receiptId, request.CustomerId)
        ? Results.Ok(new { receiptId, customerId = request.CustomerId })
        : Results.NotFound();
});

app.MapGet("/api/customers/{customerId}/receipts", (ReceiptPlatformService service, string customerId) =>
{
    if (string.IsNullOrWhiteSpace(customerId))
    {
        return Results.BadRequest("CustomerId is required.");
    }

    return Results.Ok(service.GetReceiptsByCustomer(customerId));
});

app.Run();
