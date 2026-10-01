namespace HomeServeIT.Web.Models;

public class ReportsViewModel
{
    public int TotalJobsCompleted { get; set; }
    public decimal RevenueLast30Days { get; set; }
    public double RevenueGrowthPercentage { get; set; }
    public int JobsCompletedGrowthPercentage { get; set; }
    public double AvgResolutionTimeDays { get; set; }
    public double ResolutionTimeChangeDays { get; set; }
    
    // Chart Data
    public List<decimal> RevenueTrend { get; set; } = new();
    public List<string> RevenueLabels { get; set; } = new();
    public Dictionary<string, int> JobsByCategory { get; set; } = new();

    public List<RecentReportItem> RecentReports { get; set; } = new();
}

public class RecentReportItem
{
    public int GeneratedReportID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
}
