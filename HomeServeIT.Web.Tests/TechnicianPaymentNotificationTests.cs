using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Services;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServeIT.Web.Tests;

public sealed class TechnicianPaymentNotificationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PaymentNotifiesAssignedTechnicianOnceAndRemovesInaccessibleRecords(bool isQuotation)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => database.CreateContext());
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(db);
        var job = await TestDataBuilder.AddRequestAsync(db, accounts.Customer.CustomerID, DateTime.UtcNow, accounts.Technician.TechID);
        var invoice = new Invoice { RequestID = job.RequestID, IsQuotation = isQuotation,
            QuotationStatus = "Approved", TotalAmount = 150 };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        var notifications = new NotificationService(db, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        await notifications.SynchronizeUserAsync(accounts.TechnicianUser.Id);
        Assert.Empty((await notifications.GetFeedAsync(accounts.TechnicianUser, "Billing")).Notifications);

        invoice.PaymentStatus = "Paid";
        await db.SaveChangesAsync();
        await notifications.SynchronizeUserAsync(accounts.TechnicianUser.Id);
        var feed = await notifications.GetFeedAsync(accounts.TechnicianUser, "Billing");
        Assert.Equal("Billing", feed.ActiveCategory);
        var notification = Assert.Single(feed.Notifications);
        Assert.Equal(accounts.TechnicianUser.Id, notification.RecipientUserID);
        Assert.Equal($"/Technician/AssignedJobs?jobId={job.RequestID}", notification.ActionUrl);
        Assert.Contains("150.00", notification.Message);
        Assert.False(notification.IsRead);
        await notifications.SetReadStateAsync(accounts.TechnicianUser, notification.NotificationID, true);
        await notifications.SynchronizeUserAsync(accounts.TechnicianUser.Id);
        Assert.True(Assert.Single((await notifications.GetFeedAsync(accounts.TechnicianUser, "Billing")).Notifications).IsRead);

        // Assignment, cancellation, and archival must also remove an existing alert.
        foreach (var hiddenState in new[] { "unassigned", "cancelled", "archived", "customerArchived" })
        {
            job.TechID = hiddenState == "unassigned" ? null : accounts.Technician.TechID;
            job.Status = hiddenState == "cancelled" ? "Cancelled" : "Pending";
            job.IsArchived = hiddenState == "archived";
            accounts.CustomerUser.IsArchived = hiddenState == "customerArchived";
            await db.SaveChangesAsync();
            await notifications.SynchronizeUserAsync(accounts.TechnicianUser.Id);
            Assert.Empty((await notifications.GetFeedAsync(accounts.TechnicianUser, "Billing")).Notifications);
            job.TechID = accounts.Technician.TechID;
            job.Status = "Pending";
            job.IsArchived = false;
            accounts.CustomerUser.IsArchived = false;
            await db.SaveChangesAsync();
            await notifications.SynchronizeUserAsync(accounts.TechnicianUser.Id);
            Assert.Single((await notifications.GetFeedAsync(accounts.TechnicianUser, "Billing")).Notifications);
        }
    }
}
