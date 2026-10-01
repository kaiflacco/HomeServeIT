using HomeServeIT.Web.Areas.Admin.Controllers;
using HomeServeIT.Web.Areas.Admin.Models;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServeIT.Web.Tests;

public sealed class ArchivePaginationTests
{
    [Fact]
    public async Task ArchivedUsersReturnsTheRequestedServicePage()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => database.CreateContext());
        services.AddIdentityCore<HomeServeIT.Web.Models.ApplicationUser>()
            .AddEntityFrameworkStores<HomeServeIT.Web.Data.ApplicationDbContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<HomeServeIT.Web.Data.ApplicationDbContext>();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        accounts.CustomerUser.IsArchived = true;
        for (var index = 0; index < 20; index++)
        {
            var archivedCustomer = await TestDataBuilder.AddCustomerAsync(context);
            archivedCustomer.User.IsArchived = true;
            await context.SaveChangesAsync();
        }

        for (var index = 0; index < 21; index++)
        {
            await TestDataBuilder.AddRequestAsync(
                context,
                accounts.Customer.CustomerID,
                DateTime.UtcNow.AddDays(-(index + 1)),
                status: "Completed",
                isArchived: true);
        }

        var controller = new SystemController(
            scope.ServiceProvider.GetRequiredService<UserManager<HomeServeIT.Web.Models.ApplicationUser>>(),
            context,
            null!);
        var result = Assert.IsType<ViewResult>(await controller.ArchivedUsers(servicePage: 2, userPage: 2, section: "users"));
        var model = Assert.IsType<AdminArchiveViewModel>(result.Model);

        Assert.Equal(21, model.ServiceRequestTotal);
        Assert.Equal(3, model.ServiceRequestPageCount);
        Assert.Equal(2, model.ServiceRequestPage);
        Assert.Equal(10, model.ServiceRequests.Count);
        Assert.Equal(21, model.UserTotal);
        Assert.Equal(3, model.UserPageCount);
        Assert.Equal(2, model.UserPage);
        Assert.Equal(10, model.Users.Count);
        Assert.Equal("users", model.Section);
    }
}
