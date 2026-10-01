using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeServeIT.Web.Services;

public sealed record ReportGenerationRequest(
    string Type,
    string? DateRange,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record ReportGenerationResult(
    bool Succeeded,
    string? ErrorMessage = null,
    byte[]? PdfContent = null,
    string? FileName = null,
    bool RequiresAuthorization = false);

public sealed record GeneratedReportDownload(byte[] PdfContent, string FileName);

public sealed class ReportingModule(ApplicationDbContext context)
{
    public async Task<ReportsViewModel> GetOverviewAsync(string? userId, DateTime now)
    {
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);
        var financialRecords = ActiveFinancialRecords();

        var invoicesLast30 = await financialRecords
            .Where(invoice => invoice.PaymentStatus == "Paid" && invoice.DateIssued >= thirtyDaysAgo)
            .ToListAsync();
        var revenueLast30 = invoicesLast30.Sum(invoice => invoice.TotalAmount);

        var invoicesPrev30 = await financialRecords
            .Where(invoice => invoice.PaymentStatus == "Paid"
                && invoice.DateIssued >= sixtyDaysAgo
                && invoice.DateIssued < thirtyDaysAgo)
            .ToListAsync();
        var revenuePrev30 = invoicesPrev30.Sum(invoice => invoice.TotalAmount);
        var revenueGrowth = revenuePrev30 == 0
            ? (revenueLast30 > 0 ? 100 : 0)
            : (double)((revenueLast30 - revenuePrev30) / revenuePrev30) * 100;

        var jobsLast30 = await OverviewCompletedRequests(thirtyDaysAgo, now).ToListAsync();
        var completedJobsLast30 = jobsLast30.Count;
        var jobsPrev30 = await OverviewCompletedRequests(sixtyDaysAgo, thirtyDaysAgo).ToListAsync();
        var completedJobsPrev30 = jobsPrev30.Count;
        var jobsGrowth = completedJobsPrev30 == 0
            ? (completedJobsLast30 > 0 ? 100 : 0)
            : (double)(completedJobsLast30 - completedJobsPrev30) / completedJobsPrev30 * 100;

        var avgResLast30 = CompletionTiming.AverageDays(jobsLast30);
        var avgResPrev30 = CompletionTiming.AverageDays(jobsPrev30);
        var sixMonthsAgo = now.AddMonths(-6);
        var invoicesLast6Months = await financialRecords
            .Where(invoice => invoice.PaymentStatus == "Paid" && invoice.DateIssued >= sixMonthsAgo)
            .ToListAsync();

        var recentReports = string.IsNullOrWhiteSpace(userId)
            ? []
            : await context.GeneratedReports
                .AsNoTracking()
                .Where(report => report.GeneratedByUserID == userId)
                .OrderByDescending(report => report.GeneratedAtUtc)
                .Take(10)
                .Select(report => new RecentReportItem
                {
                    GeneratedReportID = report.GeneratedReportID,
                    FileName = report.FileName,
                    ReportType = report.ReportType,
                    PeriodStartUtc = report.PeriodStartUtc,
                    PeriodEndUtc = report.PeriodEndUtc,
                    GeneratedAtUtc = report.GeneratedAtUtc
                })
                .ToListAsync();

        var revenueTrend = new List<decimal>();
        var revenueLabels = new List<string>();
        for (var i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            revenueTrend.Add(invoicesLast6Months
                .Where(invoice => invoice.DateIssued >= monthStart && invoice.DateIssued < monthEnd)
                .Sum(invoice => invoice.TotalAmount));
            revenueLabels.Add(monthStart.ToString("MMM"));
        }

        return new ReportsViewModel
        {
            TotalJobsCompleted = completedJobsLast30,
            RevenueLast30Days = revenueLast30,
            RevenueGrowthPercentage = Math.Round(revenueGrowth, 1),
            JobsCompletedGrowthPercentage = (int)Math.Round(jobsGrowth),
            AvgResolutionTimeDays = Math.Round(avgResLast30, 1),
            ResolutionTimeChangeDays = Math.Round(avgResLast30 - avgResPrev30, 1),
            JobsByCategory = jobsLast30
                .GroupBy(request => request.ServiceCategory ?? "Other")
                .ToDictionary(group => group.Key, group => group.Count()),
            RevenueTrend = revenueTrend,
            RevenueLabels = revenueLabels,
            RecentReports = recentReports
        };
    }

    public async Task<ReportGenerationResult> GenerateAsync(
        ReportGenerationRequest request,
        string? userId,
        DateTime now)
    {
        if (!ReportPdfService.SupportedTypes.Contains(request.Type, StringComparer.Ordinal))
            return new(false, "Choose a valid report type.");

        if (!TryGetReportPeriod(request.DateRange, request.StartDate, request.EndDate, now, out var period, out var periodError))
            return new(false, periodError);
        if (string.IsNullOrWhiteSpace(userId))
            return new(false, RequiresAuthorization: true);

        var invoices = await ActiveFinancialRecords()
            .Where(invoice => invoice.DateIssued >= period.CurrentStart
                && invoice.DateIssued < period.CurrentEndExclusive)
            .Include(invoice => invoice.ServiceRequest)
                .ThenInclude(serviceRequest => serviceRequest.Customer)
            .OrderByDescending(invoice => invoice.DateIssued)
            .ToListAsync();

        var previousRevenue = await ActiveFinancialRecords()
            .Where(invoice => invoice.PaymentStatus == "Paid"
                && invoice.DateIssued >= period.PreviousStart
                && invoice.DateIssued < period.PreviousEndExclusive)
            .SumAsync(invoice => (decimal?)invoice.TotalAmount) ?? 0;

        var completedRequests = await CompletedRequests(period.CurrentStart, period.CurrentEndExclusive).ToListAsync();
        var previousCompletedRequests = await CompletedRequests(period.PreviousStart, period.PreviousEndExclusive).ToListAsync();
        var serviceRequests = await context.ServiceRequests
            .AsNoTracking()
            .Include(serviceRequest => serviceRequest.Customer)
            .Include(serviceRequest => serviceRequest.Technician)
            .Where(serviceRequest => !serviceRequest.IsArchived
                && serviceRequest.Status != "Cancelled"
                && !serviceRequest.Customer.User.IsArchived
                && serviceRequest.ScheduledDate >= period.CurrentStart
                && serviceRequest.ScheduledDate < period.CurrentEndExclusive)
            .OrderByDescending(serviceRequest => serviceRequest.ScheduledDate)
            .ToListAsync();

        var statuses = serviceRequests
            .GroupBy(requestItem => DisplayStatus(requestItem.Status))
            .Select(group => new ReportCountRow(group.Key, group.Count()))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Label)
            .ToList();
        var categories = serviceRequests
            .GroupBy(requestItem => string.IsNullOrWhiteSpace(requestItem.ServiceCategory) ? "Other" : requestItem.ServiceCategory)
            .Select(group => new ReportCountRow(group.Key, group.Count()))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Label)
            .ToList();
        var technicians = completedRequests
            .Where(requestItem => requestItem.TechID.HasValue)
            .GroupBy(requestItem => new
            {
                requestItem.TechID,
                Name = requestItem.Technician == null
                    ? "Unassigned technician"
                    : $"{requestItem.Technician.FirstName} {requestItem.Technician.LastName}".Trim()
            })
            .Select(group => new ReportTechnicianRow(
                group.Key.Name,
                group.Count(),
                CompletionTiming.AverageDays(group)))
            .OrderByDescending(row => row.JobsCompleted)
            .ThenBy(row => row.Name)
            .ToList();

        var report = new ReportSnapshot(
            request.Type,
            period.CurrentStart,
            period.DisplayEnd,
            now,
            invoices.Where(invoice => invoice.PaymentStatus == "Paid").Sum(invoice => invoice.TotalAmount),
            previousRevenue,
            invoices.Where(invoice => invoice.PaymentStatus == "Unpaid").Sum(invoice => invoice.TotalAmount),
            invoices.Count(invoice => invoice.PaymentStatus == "Paid"),
            invoices.Count(invoice => invoice.PaymentStatus == "Unpaid"),
            completedRequests.Count,
            previousCompletedRequests.Count,
            CompletionTiming.AverageDays(completedRequests),
            CompletionTiming.AverageDays(previousCompletedRequests),
            statuses,
            categories,
            technicians,
            invoices.Select(invoice => new ReportInvoiceRow(
                $"{(invoice.IsQuotation ? "QT" : "INV")}-{invoice.InvoiceID:D5}",
                CustomerName(invoice.ServiceRequest.Customer),
                invoice.DateIssued,
                invoice.TotalAmount,
                invoice.PaymentStatus)).ToList(),
            serviceRequests.Select(ToReportService).ToList(),
            completedRequests
                .OrderByDescending(serviceRequest => serviceRequest.CompletedDate)
                .Select(ToReportService)
                .ToList());

        var pdfBytes = ReportPdfService.Generate(report);
        var fileName = BuildReportFileName(request.Type, period);
        context.GeneratedReports.Add(new GeneratedReport
        {
            GeneratedByUserID = userId,
            ReportType = request.Type,
            FileName = fileName,
            PeriodStartUtc = period.CurrentStart,
            PeriodEndUtc = period.DisplayEnd,
            GeneratedAtUtc = now,
            PdfContent = pdfBytes
        });
        await context.SaveChangesAsync();

        return new(true, PdfContent: pdfBytes, FileName: fileName);
    }

    public async Task<GeneratedReportDownload?> GetRecentReportAsync(int reportId, string userId)
    {
        return await context.GeneratedReports
            .AsNoTracking()
            .Where(report => report.GeneratedReportID == reportId && report.GeneratedByUserID == userId)
            .Select(report => new GeneratedReportDownload(report.PdfContent, report.FileName))
            .SingleOrDefaultAsync();
    }

    private IQueryable<Invoice> ActiveFinancialRecords() => context.Invoices
        .AsNoTracking()
        .CustomerFacingInvoices()
        .ActiveFinancialRecords()
        .Where(invoice => !invoice.ServiceRequest.IsArchived
            && !invoice.ServiceRequest.Customer.User.IsArchived
            && invoice.ServiceRequest.Status != "Cancelled");

    private IQueryable<ServiceRequest> CompletedRequests(DateTime start, DateTime endExclusive) =>
        context.ServiceRequests
            .AsNoTracking()
            .Include(request => request.Customer)
            .Include(request => request.Technician)
            .Where(request => !request.IsArchived
                && !request.Customer.User.IsArchived
                && request.Status == "Completed"
                && request.CompletedDate.HasValue
                && request.CompletedDate.Value >= start
                && request.CompletedDate.Value < endExclusive);

    private IQueryable<ServiceRequest> OverviewCompletedRequests(DateTime start, DateTime endExclusive) =>
        context.ServiceRequests
            .Where(request => request.Status == "Completed"
                && request.CompletedDate.HasValue
                && request.CompletedDate.Value >= start
                && request.CompletedDate.Value < endExclusive);

    private static ReportServiceRow ToReportService(ServiceRequest request) => new(
        $"SR-{request.RequestID:D5}",
        string.IsNullOrWhiteSpace(request.ServiceCategory) ? "Other" : request.ServiceCategory,
        CustomerName(request.Customer),
        request.Technician == null ? "Unassigned" : $"{request.Technician.FirstName} {request.Technician.LastName}".Trim(),
        request.ScheduledDate,
        DisplayStatus(request.Status));

    private static string CustomerName(Customer customer)
    {
        var name = $"{customer.FirstName} {customer.LastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? "Customer" : name;
    }

    private static string DisplayStatus(string status) => status switch
    {
        "Pending" => "Scheduled",
        "PendingAdminApproval" => "Awaiting Admin Review",
        "PendingCustomerReview" => "Awaiting Customer Approval",
        "Diagnosing" => "Diagnosis & Quotation",
        _ => status
    };

    private static string BuildReportFileName(string type, ReportPeriod period)
    {
        var typeName = type.Replace(' ', '-');
        var periodName = period.CurrentStart.Date == period.DisplayEnd.Date
            ? period.CurrentStart.ToString("yyyy-MM-dd")
            : $"{period.CurrentStart:yyyy-MM-dd}-to-{period.DisplayEnd:yyyy-MM-dd}";
        return $"HomeServeIT-{typeName}-Report-{periodName}.pdf";
    }

    private static bool TryGetReportPeriod(
        string? dateRange,
        DateTime? startDate,
        DateTime? endDate,
        DateTime now,
        out ReportPeriod period,
        out string error)
    {
        DateTime currentStart;
        DateTime currentEndExclusive;
        DateTime displayEnd;

        switch (dateRange)
        {
            case "7":
            case "30":
            case "90":
                var days = int.Parse(dateRange);
                currentStart = now.AddDays(-days);
                currentEndExclusive = now;
                displayEnd = now;
                break;
            case "month":
                currentStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                currentEndExclusive = now;
                displayEnd = now;
                break;
            case "quarter":
                var quarterStartMonth = ((now.Month - 1) / 3 * 3) + 1;
                currentStart = new DateTime(now.Year, quarterStartMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                currentEndExclusive = now;
                displayEnd = now;
                break;
            case "custom":
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    period = default;
                    error = "Choose both a start date and an end date.";
                    return false;
                }

                currentStart = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                displayEnd = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
                if (currentStart > displayEnd)
                {
                    period = default;
                    error = "The start date must be on or before the end date.";
                    return false;
                }

                currentEndExclusive = displayEnd.AddDays(1);
                break;
            default:
                period = default;
                error = "Choose a valid date range.";
                return false;
        }

        var duration = currentEndExclusive - currentStart;
        period = new ReportPeriod(
            currentStart,
            currentEndExclusive,
            currentStart - duration,
            currentStart,
            displayEnd);
        error = string.Empty;
        return true;
    }

    private readonly record struct ReportPeriod(
        DateTime CurrentStart,
        DateTime CurrentEndExclusive,
        DateTime PreviousStart,
        DateTime PreviousEndExclusive,
        DateTime DisplayEnd);
}
