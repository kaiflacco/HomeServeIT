using HomeServeIT.Web.Areas.Customer.Controllers;
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

public sealed class CustomerWorkflowTests
{
    [Fact]
    public async Task CustomerCanManageOnlyTheirOwnDevices()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var otherCustomer = await TestDataBuilder.AddCustomerAsync(context);
        var ownDevice = new Device { CustomerID = accounts.Customer.CustomerID, Name = "Own laptop", Type = "Laptop" };
        var otherDevice = new Device { CustomerID = otherCustomer.CustomerID, Name = "Other laptop", Type = "Laptop" };
        context.Devices.AddRange(ownDevice, otherDevice);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new MyDevicesController(context, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()),
            accounts.CustomerUser,
            Roles.Customer);

        var result = await controller.DeleteDevice(ownDevice.DeviceID);
        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(await context.Devices.AnyAsync(device => device.DeviceID == ownDevice.DeviceID));

        var denied = await controller.DeleteDevice(otherDevice.DeviceID);
        Assert.IsType<NotFoundResult>(denied);
        Assert.True(await context.Devices.AnyAsync(device => device.DeviceID == otherDevice.DeviceID));
    }

    [Fact]
    public async Task CustomerCanCreateARequestAndCannotReadAnotherCustomersChat()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var otherCustomer = await TestDataBuilder.AddCustomerAsync(context);
        var otherRequest = await TestDataBuilder.AddRequestAsync(context, otherCustomer.CustomerID, DateTime.UtcNow.AddDays(1));
        using var environment = new TestWebHostEnvironment();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var controller = CreateController(
            new ServiceRequestsController(context, userManager, environment, new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);
        var uploads = new PrivateUploadService(environment);

        var created = await controller.Create(
            "Wi-Fi drops during video calls",
            DateTime.Now.AddDays(1),
            "Network Setup",
            "Urgent",
            null,
            uploads);

        Assert.IsType<RedirectToActionResult>(created);
        var request = await context.ServiceRequests.SingleAsync(item => item.IssueDescription == "Wi-Fi drops during video calls");
        Assert.Equal(accounts.Customer.CustomerID, request.CustomerID);
        Assert.Equal("Pending", request.Status);

        var denied = await controller.GetChatHistory(otherRequest.RequestID);
        Assert.IsType<ForbidResult>(denied);
    }

    [Fact]
    public async Task CustomerSupportIsScopedAndValidatesBoundaries()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var otherCustomer = await TestDataBuilder.AddCustomerAsync(context);
        context.SupportTickets.Add(new SupportTicket
        {
            CustomerID = otherCustomer.CustomerID,
            Subject = "Other customer's issue",
            Message = "Should not be visible"
        });
        await context.SaveChangesAsync();

        var controller = CreateController(
            new SupportController(context, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()),
            accounts.CustomerUser,
            Roles.Customer);
        var invalid = await controller.Create("", "");
        Assert.IsType<RedirectToActionResult>(invalid);
        Assert.Empty(await context.SupportTickets.Where(ticket => ticket.CustomerID == accounts.Customer.CustomerID).ToListAsync());

        var created = await controller.Create("Need help", "Please check the office access point.");
        Assert.IsType<RedirectToActionResult>(created);
        var ownTickets = await context.SupportTickets.Where(ticket => ticket.CustomerID == accounts.Customer.CustomerID).ToListAsync();
        Assert.Single(ownTickets);
        Assert.Equal("Need help", ownTickets[0].Subject);

        var view = Assert.IsType<ViewResult>(await controller.Index(null));
        var tickets = Assert.IsAssignableFrom<IReadOnlyCollection<SupportTicket>>(view.Model);
        Assert.DoesNotContain(tickets, ticket => ticket.CustomerID == otherCustomer.CustomerID);
    }

    [Fact]
    public async Task CustomerCannotPayUnapprovedQuotationAndApprovedPaymentIsIdempotent()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID);
        var allocation = await TestDataBuilder.AddInventoryAllocationAsync(context, request.RequestID, 4, 1);
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "PendingAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 100
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var controller = CreateController(
            new ServiceRequestsController(context, userManager, new TestWebHostEnvironment(), new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);
        var quotationController = CreateController(
            new QuotationsController(context, userManager, new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);

        Assert.IsType<NotFoundResult>(await quotationController.Checkout(quotation.InvoiceID));

        var denied = await controller.PayInvoice(quotation.InvoiceID);
        Assert.IsType<RedirectToActionResult>(denied);
        Assert.Equal("Unpaid", quotation.PaymentStatus);
        Assert.Equal(4, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());

        quotation.QuotationStatus = "ApprovedByAdmin";
        await context.SaveChangesAsync();
        var paid = await controller.PayInvoice(quotation.InvoiceID);
        Assert.IsType<RedirectToActionResult>(paid);
        Assert.Equal("Paid", quotation.PaymentStatus);
        Assert.Equal("In Progress", request.Status);
        Assert.Equal(3, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Single(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());

        var retry = await controller.PayInvoice(quotation.InvoiceID);
        Assert.IsType<RedirectToActionResult>(retry);
        Assert.Equal(3, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Single(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());
    }

    [Fact]
    public async Task QuotationPaymentRejectsVoidedQuotesAndNonStartableJobs()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID, status: "Diagnosing");
        var allocation = await TestDataBuilder.AddInventoryAllocationAsync(context, request.RequestID, 4, 1);
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "ApprovedByAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 100
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new QuotationsController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);

        var invalidMethod = await controller.ProcessPayment(quotation.InvoiceID, "Demo");
        Assert.IsType<RedirectToActionResult>(invalidMethod);
        Assert.Equal("Unpaid", quotation.PaymentStatus);

        var nonStartable = await controller.ProcessPayment(quotation.InvoiceID, "Cash");
        Assert.IsType<RedirectToActionResult>(nonStartable);
        Assert.Equal("Unpaid", quotation.PaymentStatus);
        Assert.Equal(4, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());

        request.Status = "Pending";
        quotation.PaymentStatus = "Voided";
        await context.SaveChangesAsync();

        var voided = await controller.ProcessPayment(quotation.InvoiceID, "Cash");
        Assert.IsType<RedirectToActionResult>(voided);
        Assert.Equal("Voided", quotation.PaymentStatus);
        Assert.Equal(4, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Empty(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());

        using var environment = new TestWebHostEnvironment();
        var primaryController = CreateController(
            new ServiceRequestsController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                environment,
                new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);
        var primaryVoided = await primaryController.PayInvoice(quotation.InvoiceID);
        Assert.IsType<RedirectToActionResult>(primaryVoided);
        Assert.Equal(4, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Empty(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());
    }

    [Fact]
    public async Task LegacyQuotationPaymentPathRequiresApprovalAndDoesNotDuplicateInventoryUsage()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var provider = await CreateIdentityProviderAsync(database);
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var request = await TestDataBuilder.AddRequestAsync(context, accounts.Customer.CustomerID, DateTime.UtcNow.AddDays(1), accounts.Technician.TechID);
        var allocation = await TestDataBuilder.AddInventoryAllocationAsync(context, request.RequestID, 3, 1);
        var quotation = new Invoice
        {
            RequestID = request.RequestID,
            IsQuotation = true,
            QuotationStatus = "PendingAdmin",
            PaymentStatus = "Unpaid",
            TotalAmount = 125
        };
        context.Invoices.Add(quotation);
        await context.SaveChangesAsync();

        var controller = CreateController(
            new QuotationsController(
                context,
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
                new JobInventoryService(context)),
            accounts.CustomerUser,
            Roles.Customer);

        var blocked = await controller.ProcessPayment(quotation.InvoiceID, "Cash");
        Assert.IsType<RedirectToActionResult>(blocked);
        Assert.Equal("Unpaid", quotation.PaymentStatus);

        quotation.QuotationStatus = "ApprovedByAdmin";
        await context.SaveChangesAsync();
        var first = await controller.ProcessPayment(quotation.InvoiceID, "Cash");
        Assert.IsType<RedirectToActionResult>(first);
        Assert.Equal("Paid", quotation.PaymentStatus);
        Assert.Equal(2, await context.InventoryItems.Where(item => item.ItemID == allocation.Item.ItemID).Select(item => item.StockQuantity).SingleAsync());
        Assert.Single(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());

        var second = await controller.ProcessPayment(quotation.InvoiceID, "Cash");
        Assert.IsType<RedirectToActionResult>(second);
        Assert.Single(await context.StockMovements.Where(movement => movement.RequestID == request.RequestID).ToListAsync());
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
