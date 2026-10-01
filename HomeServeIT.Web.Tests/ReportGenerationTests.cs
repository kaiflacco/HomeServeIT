using HomeServeIT.Web.Areas.Admin.Controllers;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;

namespace HomeServeIT.Web.Tests;

public sealed class ReportGenerationTests
{
    [Fact]
    public async Task DownloadReportGeneratesTypeSpecificPdfContent()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var fixture = await TestDataBuilder.SeedRoleAccountsAsync(context);
        var now = DateTime.UtcNow;

        var completed = AddRequest(
            fixture.Customer.CustomerID,
            fixture.Technician.TechID,
            now.AddDays(-8),
            "Completed",
            "Hardware Repair",
            now.AddDays(-5));
        var awaitingApproval = AddRequest(
            fixture.Customer.CustomerID,
            fixture.Technician.TechID,
            now.AddDays(-4),
            "PendingCustomerReview",
            "Network Setup");
        var inProgress = AddRequest(
            fixture.Customer.CustomerID,
            fixture.Technician.TechID,
            now.AddDays(-2),
            "In Progress",
            "Software / OS");
        var priorCompleted = AddRequest(
            fixture.Customer.CustomerID,
            fixture.Technician.TechID,
            now.AddDays(-31),
            "Completed",
            "Hardware Repair",
            now.AddDays(-27));

        context.ServiceRequests.AddRange(completed, awaitingApproval, inProgress, priorCompleted);
        await context.SaveChangesAsync();
        context.Invoices.AddRange(
            new Invoice
            {
                RequestID = completed.RequestID,
                TotalAmount = 4250,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-6)
            },
            new Invoice
            {
                RequestID = inProgress.RequestID,
                TotalAmount = 1800,
                PaymentStatus = "Unpaid",
                DateIssued = now.AddDays(-2)
            },
            new Invoice
            {
                RequestID = priorCompleted.RequestID,
                TotalAmount = 3100,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-28)
            },
            new Invoice
            {
                RequestID = awaitingApproval.RequestID,
                TotalAmount = 2500,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-3),
                IsQuotation = true,
                QuotationStatus = "Approved"
            },
            new Invoice
            {
                RequestID = awaitingApproval.RequestID,
                TotalAmount = 9999,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-2),
                IsQuotation = true,
                QuotationStatus = "PendingAdmin"
            });
        await context.SaveChangesAsync();

        var outputs = new Dictionary<string, (byte[] Bytes, string Text, string FileName)>();
        foreach (var type in new[] { "Full Summary", "Financial", "Performance", "Service Summary" })
        {
            var controller = Controller(context, fixture.Administrator);
            var result = Assert.IsType<FileContentResult>(await controller.DownloadReport(
                type,
                "custom",
                now.Date.AddDays(-20),
                now.Date));
            outputs[type] = (result.FileContents, PdfText(result.FileContents), result.FileDownloadName!);
        }

        Assert.All(outputs.Values, output => Assert.True(output.Bytes.Length > 1_000));
        Assert.Equal(4, outputs.Values.Select(output => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(output.Bytes))).Distinct().Count());
        Assert.All(outputs, output => Assert.Equal(
            $"HomeServeIT-{output.Key.Replace(' ', '-')}-Report-{now.Date.AddDays(-20):yyyy-MM-dd}-to-{now.Date:yyyy-MM-dd}.pdf",
            output.Value.FileName));

        Assert.Contains("At-a-glance business health", outputs["Full Summary"].Text);
        Assert.Contains("Financial pulse", outputs["Full Summary"].Text);
        Assert.Contains("Technician performance", outputs["Full Summary"].Text);

        Assert.Contains("Revenue and collections", outputs["Financial"].Text);
        Assert.Contains("Invoice register", outputs["Financial"].Text);
        Assert.Contains("₱6,750.00", outputs["Financial"].Text);
        Assert.Contains("QT-", outputs["Financial"].Text);
        Assert.DoesNotContain("₱9,999.00", outputs["Financial"].Text);
        Assert.DoesNotContain("Service request register", outputs["Financial"].Text);

        Assert.Contains("Delivery performance", outputs["Performance"].Text);
        Assert.Contains("Technician performance", outputs["Performance"].Text);
        Assert.Contains("Recently completed services", outputs["Performance"].Text);

        Assert.Contains("Service operations", outputs["Service Summary"].Text);
        Assert.Contains("Status distribution", outputs["Service Summary"].Text);
        Assert.Contains("Service request register", outputs["Service Summary"].Text);
        Assert.Contains("Awaiting Customer Approval", outputs["Service Summary"].Text);

        var savedReports = await context.GeneratedReports.AsNoTracking().OrderBy(r => r.GeneratedReportID).ToListAsync();
        Assert.Equal(4, savedReports.Count);
        Assert.All(savedReports, saved => Assert.Equal(fixture.Administrator.Id, saved.GeneratedByUserID));

        var financialHistory = savedReports.Single(saved => saved.ReportType == "Financial");
        var savedDownload = Assert.IsType<FileContentResult>(await Controller(context, fixture.Administrator)
            .DownloadRecentReport(financialHistory.GeneratedReportID));
        Assert.Equal(outputs["Financial"].Bytes, savedDownload.FileContents);
        Assert.Equal(outputs["Financial"].FileName, savedDownload.FileDownloadName);

        var otherUserDownload = await Controller(context, fixture.CustomerUser)
            .DownloadRecentReport(financialHistory.GeneratedReportID);
        Assert.IsType<NotFoundResult>(otherUserDownload);

        var reportsPage = Assert.IsType<ViewResult>(await Controller(context, fixture.Administrator).Reports());
        var pageModel = Assert.IsType<ReportsViewModel>(reportsPage.Model);
        Assert.Equal(9850m, pageModel.RevenueLast30Days);
        Assert.Equal(4, pageModel.RecentReports.Count);
        Assert.Contains(pageModel.RecentReports, recent => recent.FileName == outputs["Financial"].FileName);

        ServiceRequest AddRequest(
            int customerId,
            int technicianId,
            DateTime scheduled,
            string status,
            string category,
            DateTime? completedDate = null) => new()
            {
                CustomerID = customerId,
                TechID = technicianId,
                IssueDescription = $"{category} report test",
                ScheduledDate = scheduled,
                Status = status,
                ServiceCategory = category,
                CompletedDate = completedDate
            };
    }

    [Theory]
    [InlineData("month")]
    [InlineData("quarter")]
    public async Task DownloadReportSupportsCalendarPresets(string dateRange)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var fixture = await TestDataBuilder.SeedRoleAccountsAsync(context);

        var result = await Controller(context, fixture.Administrator).DownloadReport("Full Summary", dateRange);

        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadReportRejectsUnsupportedTypeAndInvalidCustomRange()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var controller = Controller(context);

        Assert.IsType<BadRequestObjectResult>(await controller.DownloadReport("Unknown", "30"));
        Assert.IsType<BadRequestObjectResult>(await controller.DownloadReport(
            "Financial",
            "custom",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(-1)));
    }

    private static CrmController Controller(
        HomeServeIT.Web.Data.ApplicationDbContext context,
        ApplicationUser? user = null) => new(context, new HomeServeIT.Web.Services.ReportingModule(context))
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user == null
                    ? new System.Security.Claims.ClaimsPrincipal()
                    : TestDataBuilder.CreatePrincipal(user, user.Role ?? Roles.Administrator)
            }
        }
    };

    private static string PdfText(byte[] bytes)
    {
        using var document = PdfDocument.Open(bytes);
        return string.Join(" ", document.GetPages().SelectMany(page => page.GetWords()).Select(word => word.Text));
    }
}
