using HomeServeIT.Web.Areas.Admin.Controllers;
using HomeServeIT.Web.Areas.Admin.Models;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HomeServeIT.Web.Tests;

public sealed class DashboardTests
{
    [Fact]
    public async Task IndexSeparatesAssignedPendingRequestsAsScheduled()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);

        await TestDataBuilder.AddRequestAsync(
            context,
            accounts.Customer.CustomerID,
            DateTime.Today.AddDays(1),
            status: "Pending");
        await TestDataBuilder.AddRequestAsync(
            context,
            accounts.Customer.CustomerID,
            DateTime.Today.AddDays(2),
            accounts.Technician.TechID,
            status: "Pending");

        var controller = new DashboardController(context, null!);
        var result = Assert.IsType<ViewResult>(await controller.Index());
        var model = Assert.IsType<AdminDashboardViewModel>(result.Model);

        Assert.Equal(1, model.PendingCount);
        Assert.Equal(1, model.ScheduledCount);
    }

    [Fact]
    public async Task IndexUsesSelectedWeekForPeriodRevenueAndCompletedJobs()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var accounts = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        var inPeriod = await TestDataBuilder.AddRequestAsync(
            context,
            accounts.Customer.CustomerID,
            weekStart.AddDays(1),
            status: "Completed");
        var outsidePeriod = await TestDataBuilder.AddRequestAsync(
            context,
            accounts.Customer.CustomerID,
            weekStart.AddDays(-1),
            status: "Completed");

        context.Invoices.AddRange(
            new Invoice
            {
                RequestID = inPeriod.RequestID,
                TotalAmount = 125,
                PaymentStatus = "Paid",
                DateIssued = weekStart.AddDays(1)
            },
            new Invoice
            {
                RequestID = outsidePeriod.RequestID,
                TotalAmount = 75,
                PaymentStatus = "Paid",
                DateIssued = weekStart.AddDays(-1)
            });
        await context.SaveChangesAsync();

        var controller = new DashboardController(context, null!);
        var result = Assert.IsType<ViewResult>(await controller.Index("week"));
        var model = Assert.IsType<AdminDashboardViewModel>(result.Model);

        Assert.Equal("week", model.Period);
        Assert.Equal("This week", model.PeriodLabel);
        Assert.Equal(125, model.TotalRevenue);
        Assert.Equal(1, model.CompletedJobsThisMonthCount);
    }
}
