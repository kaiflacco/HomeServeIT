using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Areas.Admin.Models;

namespace HomeServeIT.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Administrator)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? period = null)
        {
            var today = DateTime.Today;
            var timeframe = ResolveTimeframe(period, today);
            var activeRequests = _context.ServiceRequests
                .Where(r => !r.IsArchived && r.Status != "Cancelled");
            var vm = new AdminDashboardViewModel
            {
                Period = timeframe.Key,
                PeriodLabel = timeframe.Label,
                PeriodDescription = timeframe.Description,
                ChartRangeLabel = timeframe.ChartRangeLabel,
                RevenueChartTitle = timeframe.RevenueChartTitle,
                TotalActiveRequests = await _context.ServiceRequests
                    .CountAsync(r => !r.IsArchived && r.Status != "Completed" && r.Status != "Cancelled"),
                JobsTodayCount = await _context.ServiceRequests
                    .CountAsync(r => !r.IsArchived && r.Status != "Cancelled"
                        && r.ScheduledDate >= today && r.ScheduledDate < today.AddDays(1)),
                TotalTechnicians = await _context.Technicians.CountAsync(),
                AvailableTechniciansCount = await _context.Technicians.CountAsync(t => t.IsAvailable),
                OutstandingInvoicesAmount = await _context.Invoices
                    .Where(i => i.PaymentStatus != "Paid")
                    .SumAsync(i => (decimal?)i.TotalAmount) ?? 0,
                OutstandingInvoicesCount = await _context.Invoices
                    .CountAsync(i => i.PaymentStatus != "Paid"),
                TotalRevenue = await _context.Invoices
                    .Where(i => i.PaymentStatus == "Paid"
                        && i.DateIssued >= timeframe.Start && i.DateIssued < timeframe.End)
                    .SumAsync(i => (decimal?)i.TotalAmount) ?? 0,
                LowStockItemsCount = await _context.InventoryItems
                    .CountAsync(i => !i.IsArchived && i.StockQuantity <= i.ReorderLevel),
                NewCustomersThisWeekCount = await _context.Customers
                    .Include(c => c.User)
                    .CountAsync(c => c.User != null
                        && c.User.DateCreated >= timeframe.Start && c.User.DateCreated < timeframe.End),
                CompletedJobsThisMonthCount = await _context.ServiceRequests
                    .CountAsync(r => r.Status == "Completed" && r.CompletedDate.HasValue
                        && r.CompletedDate.Value >= timeframe.Start && r.CompletedDate.Value < timeframe.End),
                
                // Status distribution is a current-workload snapshot, independent of the selected KPI period.
                CompletedCount = await activeRequests.CountAsync(r => r.Status == "Completed"),
                InProgressCount = await activeRequests.CountAsync(r => r.Status == "In Progress"),
                ScheduledCount = await activeRequests
                    .CountAsync(r => r.Status == "Pending" && r.TechID != null),
                PendingCount = await activeRequests
                    .CountAsync(r => r.Status == "Pending" && r.TechID == null),
                RecentRequests = await _context.ServiceRequests
                    .Include(r => r.Customer)
                    .Include(r => r.Technician)
                    .Where(r => !r.IsArchived && r.Status != "Cancelled")
                    .OrderByDescending(r => r.ScheduledDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string? period)
        {
            var today = DateTime.Today;
            var timeframe = ResolveTimeframe(period, today);
            var buckets = BuildChartBuckets(timeframe.Key, today);
            var labels = new List<string>(buckets.Count);
            var revenues = new List<decimal>(buckets.Count);
            var requests = new List<int>(buckets.Count);

            foreach (var bucket in buckets)
            {
                labels.Add(bucket.Label);

                var revenue = await _context.Invoices
                    .Where(inv => inv.PaymentStatus == "Paid"
                        && inv.DateIssued >= bucket.Start && inv.DateIssued < bucket.End)
                    .SumAsync(inv => (decimal?)inv.TotalAmount) ?? 0;
                revenues.Add(revenue);

                var requestVolume = await _context.ServiceRequests
                    .CountAsync(sr => !sr.IsArchived
                        && sr.Status != "Cancelled"
                        && sr.ScheduledDate >= bucket.Start && sr.ScheduledDate < bucket.End);
                requests.Add(requestVolume);
            }

            return Json(new {
                period = timeframe.Key,
                labels,
                revenueData = revenues,
                requestData = requests
            });
        }

        private static DashboardTimeframe ResolveTimeframe(string? period, DateTime today)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "week" => new DashboardTimeframe(
                    "week",
                    "This week",
                    "Current week",
                    today.AddDays(-(int)today.DayOfWeek),
                    today.AddDays(7 - (int)today.DayOfWeek),
                    "Last 7 days",
                    "Daily revenue"),
                "year" => new DashboardTimeframe(
                    "year",
                    "This year",
                    "Current year",
                    new DateTime(today.Year, 1, 1),
                    new DateTime(today.Year + 1, 1, 1),
                    "This year",
                    "Monthly revenue"),
                _ => new DashboardTimeframe(
                    "month",
                    "This month",
                    "Current month",
                    new DateTime(today.Year, today.Month, 1),
                    new DateTime(today.Year, today.Month, 1).AddMonths(1),
                    "Last 6 months",
                    "Monthly revenue")
            };
        }

        private static IReadOnlyList<ChartBucket> BuildChartBuckets(string period, DateTime today)
        {
            if (period == "week")
            {
                var start = today.AddDays(-(int)today.DayOfWeek);
                return Enumerable.Range(0, 7)
                    .Select(offset =>
                    {
                        var bucketStart = start.AddDays(offset);
                        return new ChartBucket(bucketStart.ToString("ddd d"), bucketStart, bucketStart.AddDays(1));
                    })
                    .ToList();
            }

            if (period == "year")
            {
                return Enumerable.Range(0, 12)
                    .Select(month =>
                    {
                        var bucketStart = new DateTime(today.Year, month + 1, 1);
                        return new ChartBucket(bucketStart.ToString("MMM"), bucketStart, bucketStart.AddMonths(1));
                    })
                    .ToList();
            }

            var currentMonth = new DateTime(today.Year, today.Month, 1);
            return Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    var bucketStart = currentMonth.AddMonths(offset - 5);
                    return new ChartBucket(bucketStart.ToString("MMM"), bucketStart, bucketStart.AddMonths(1));
                })
                .ToList();
        }

        private sealed record DashboardTimeframe(
            string Key,
            string Label,
            string Description,
            DateTime Start,
            DateTime End,
            string ChartRangeLabel,
            string RevenueChartTitle);

        private sealed record ChartBucket(string Label, DateTime Start, DateTime End);
    }
}
