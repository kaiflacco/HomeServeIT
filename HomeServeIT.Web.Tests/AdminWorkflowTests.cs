using HomeServeIT.Web.Areas.Admin.Controllers;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Services;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServeIT.Web.Tests;

public sealed class AdminWorkflowTests
{
    [Fact]
    public async Task ApprovalRejectsForgedAmountAndUsesCurrentCatalogPriceWhenApproved()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID);
        var allocation = await TestDataBuilder.AddInventoryAllocationAsync(context, request.RequestID, 5, 2);
        allocation.Item.UnitPrice = 20m;
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "PendingAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 140m,
            BreakdownDetails = "Parts ₱30.00\nLabor — ₱110.00"
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new FinanceController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new InventoryCatalogService(context)),
            accounts.Administrator,
            Roles.Administrator);

        var forged = await controller.ApproveQuotation(quotation.InvoiceID, 1m, "Forged breakdown");

        Assert.IsType<RedirectToActionResult>(forged);
        Assert.Equal("PendingAdmin", quotation.QuotationStatus);
        Assert.Equal(140m, quotation.TotalAmount);

        var result = await controller.ApproveQuotation(quotation.InvoiceID, 150m, "Ignored client breakdown");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(150m, quotation.TotalAmount);
        Assert.Equal("ApprovedByAdmin", quotation.QuotationStatus);
        Assert.Equal("Unpaid", quotation.PaymentStatus);
        Assert.Contains("₱40.00", quotation.BreakdownDetails);
        Assert.Contains("Labor — ₱110.00", quotation.BreakdownDetails);
        Assert.Equal(20m, allocation.Usage.UnitPrice);
        Assert.Equal(2, allocation.Usage.Quantity);
        var audit = await context.ApplicationSettingAudits.SingleAsync();
        Assert.Equal("QuotationApproval", audit.Section);
        Assert.Equal(accounts.Administrator.Id, audit.ActorUserId);
        Assert.Contains("TotalAmount=150.00", audit.ChangedFields);
    }

    [Fact]
    public async Task ApprovalRejectsInsufficientStockWithoutChangingTheQuotation()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID);
        var allocation = await TestDataBuilder.AddInventoryAllocationAsync(context, request.RequestID, 1, 2);
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "PendingAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 140m,
            BreakdownDetails = "Parts ₱30.00\nLabor — ₱110.00"
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new FinanceController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new InventoryCatalogService(context)),
            accounts.Administrator,
            Roles.Administrator);

        var result = await controller.ApproveQuotation(quotation.InvoiceID, 140m, "Approved by admin");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PendingAdmin", quotation.QuotationStatus);
        Assert.Equal("Unpaid", quotation.PaymentStatus);
        Assert.Equal(140m, quotation.TotalAmount);
        Assert.Equal(1, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Empty(await context.StockMovements.ToListAsync());
    }

    [Fact]
    public async Task ApprovalRejectsAZeroValueQuotation()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID);
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "PendingAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 0m
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new FinanceController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new InventoryCatalogService(context)),
            accounts.Administrator,
            Roles.Administrator);

        var result = await controller.ApproveQuotation(quotation.InvoiceID, 0.01m, null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PendingAdmin", quotation.QuotationStatus);
        Assert.Equal(0m, quotation.TotalAmount);
        Assert.Empty(await context.ApplicationSettingAudits.ToListAsync());
    }

    [Fact]
    public async Task RestockCreatesAnAuditableMovementAndRejectsInvalidQuantities()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var item = new InventoryItem
        {
            ItemName = "Replacement access point",
            SKU = "TEST-RESTOCK-001",
            Category = "Networking",
            StockQuantity = 2,
            UnitCost = 20,
            UnitPrice = 30,
            ReorderLevel = 1
        };
        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new FinanceController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new InventoryCatalogService(context)),
            accounts.Administrator,
            Roles.Administrator);

        var invalid = await controller.RestockInventory(item.ItemID, 0);
        Assert.IsType<RedirectToActionResult>(invalid);
        Assert.Equal(2, item.StockQuantity);
        Assert.Empty(await context.StockMovements.ToListAsync());

        var valid = await controller.RestockInventory(item.ItemID, 3);
        Assert.IsType<RedirectToActionResult>(valid);
        Assert.Equal(5, await context.InventoryItems.Where(candidate => candidate.ItemID == item.ItemID).Select(candidate => candidate.StockQuantity).SingleAsync());
        var movement = await context.StockMovements.SingleAsync();
        Assert.Equal("Restock", movement.MovementType);
        Assert.Equal(3, movement.Quantity);
        Assert.Equal(item.ItemID, movement.ItemID);
    }

    private static async Task<ServiceProvider> CreateIdentityProviderAsync(SqliteTestDatabase database)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => database.CreateContext());
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        return services.BuildServiceProvider();
    }

    private static TController CreateController<TController>(TController controller, ApplicationUser user, string role)
        where TController : Controller
    {
        var http = new DefaultHttpContext { User = TestDataBuilder.CreatePrincipal(user, role) };
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new TestTempDataProvider());
        return controller;
    }
}
