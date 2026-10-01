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
using CustomerProfile = HomeServeIT.Web.Areas.Customer.Controllers.ProfileAndSettingsController;
using TechnicianProfile = HomeServeIT.Web.Areas.Technician.Controllers.ProfileAndSettingsController;

namespace HomeServeIT.Web.Tests;

public sealed class NotificationPreferencesTests
{
    [Theory]
    [InlineData(Roles.Customer)]
    [InlineData(Roles.Technician)]
    public async Task PreferencesPersistForCurrentAccountAndFilterFeedTrayAndCounts(string role)
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
        var user = role == Roles.Customer ? accounts.CustomerUser : accounts.TechnicianUser;
        var other = role == Roles.Customer ? accounts.TechnicianUser : accounts.CustomerUser;
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var profiles = new AccountProfileService(db, users);
        var notifications = new NotificationService(db, users);
        Controller controller = role == Roles.Customer
            ? new CustomerProfile(users, null!, profiles) : new TechnicianProfile(users, null!, profiles);
        var http = new DefaultHttpContext { User = TestDataBuilder.CreatePrincipal(user, role) };
        // A forged ownership value must have no effect: actions only use the current principal.
        http.Request.QueryString = new QueryString($"?userId={other.Id}");
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new EmptyTempDataProvider());
        async Task<IActionResult> Save(NotificationPreferencesViewModel model) => controller is CustomerProfile customer
            ? await customer.UpdateNotifications(model) : await ((TechnicianProfile)controller).UpdateNotifications(model);

        foreach (var category in new[] { "Jobs", "Quotations", "Billing", "Support" })
            db.UserNotifications.Add(new UserNotification { RecipientUserID = user.Id, AudienceRole = role,
                SourceKey = $"fixture:{category}", Category = category, Title = category, Message = "Fixture update",
                Icon = "info", ActionUrl = "/", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        foreach (var muted in new[] { "Jobs", "Quotations", "Billing" })
        {
            var preferences = new NotificationPreferencesViewModel
            {
                JobUpdates = muted != "Jobs", Quotations = muted != "Quotations", Billing = muted != "Billing"
            };
            Assert.IsType<RedirectToActionResult>(await Save(preferences));
            await using var verification = database.CreateContext();
            var saved = await verification.Users.SingleAsync(u => u.Id == user.Id);
            Assert.Equal(preferences.JobUpdates, saved.PrefApptReminders);
            Assert.Equal(preferences.Quotations, saved.PrefQuotations);
            Assert.Equal(preferences.Billing, saved.PrefInvoices);
            var untouched = await verification.Users.SingleAsync(u => u.Id == other.Id);
            Assert.True(untouched.PrefApptReminders && untouched.PrefQuotations && untouched.PrefInvoices);
            var feed = await notifications.GetFeedAsync(saved);
            Assert.DoesNotContain(feed.Notifications, n => n.Category == muted);
            Assert.Equal(3, feed.UnreadCount);
            Assert.Equal(0, feed.CategoryCounts[muted]);
            Assert.DoesNotContain((await notifications.GetFeedAsync(saved, take: 5)).Notifications, n => n.Category == muted);
            Assert.Empty((await notifications.GetFeedAsync(saved, muted)).Notifications);
        }

        var allOff = new NotificationPreferencesViewModel { JobUpdates = false, Quotations = false, Billing = false };
        await Save(allOff);
        await Save(allOff); // Repeat submission is harmless.
        Assert.Equal("Support", Assert.Single((await notifications.GetFeedAsync(user)).Notifications).Category);
        await notifications.MarkAllReadAsync(user);
        Assert.Equal(3, await db.UserNotifications.CountAsync(n => !n.IsRead)); // Muted updates keep their unread state.
        await Save(new NotificationPreferencesViewModel());
        Assert.Equal(4, (await notifications.GetFeedAsync(user)).TotalCount);
        Assert.Equal(3, (await notifications.GetFeedAsync(user)).UnreadCount);
        Assert.True(ProfileViewModel.FromUser(user).Notifications.Billing);

        controller.ModelState.AddModelError("Billing", "Invalid boolean");
        await Save(allOff);
        Assert.True(user.PrefInvoices);
        http.User = new System.Security.Claims.ClaimsPrincipal();
        Assert.IsType<ChallengeResult>(await Save(allOff));
    }

    [Fact]
    public async Task SynchronizationRetainsMutedCustomerUpdatesForReEnabling()
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
        await TestDataBuilder.AddRequestAsync(db, accounts.Customer.CustomerID, DateTime.UtcNow, accounts.Technician.TechID);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var notifications = new NotificationService(db, users);
        await notifications.SynchronizeUserAsync(accounts.CustomerUser.Id);
        var before = Assert.Single((await notifications.GetFeedAsync(accounts.CustomerUser)).Notifications);
        await notifications.SetReadStateAsync(accounts.CustomerUser, before.NotificationID, true);
        var profiles = new AccountProfileService(db, users);
        await profiles.UpdateNotificationPreferencesAsync(accounts.CustomerUser, new() { JobUpdates = false });
        await notifications.SynchronizeUserAsync(accounts.CustomerUser.Id);
        Assert.Empty((await notifications.GetFeedAsync(accounts.CustomerUser)).Notifications);
        await profiles.UpdateNotificationPreferencesAsync(accounts.CustomerUser, new());
        var restored = Assert.Single((await notifications.GetFeedAsync(accounts.CustomerUser)).Notifications);
        Assert.Equal(before.NotificationID, restored.NotificationID);
        Assert.True(restored.IsRead);
    }

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
