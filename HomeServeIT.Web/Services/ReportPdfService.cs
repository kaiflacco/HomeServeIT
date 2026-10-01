using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HomeServeIT.Web.Services;

public sealed record ReportInvoiceRow(string Number, string Customer, DateTime Issued, decimal Amount, string Status);
public sealed record ReportServiceRow(string Number, string Category, string Customer, string Technician, DateTime Scheduled, string Status);
public sealed record ReportCountRow(string Label, int Count);
public sealed record ReportTechnicianRow(string Name, int JobsCompleted, double AverageResolutionDays);

public sealed record ReportSnapshot(
    string Type,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime GeneratedAt,
    decimal Revenue,
    decimal PreviousRevenue,
    decimal Outstanding,
    int PaidInvoices,
    int UnpaidInvoices,
    int CompletedJobs,
    int PreviousCompletedJobs,
    double AverageResolutionDays,
    double PreviousAverageResolutionDays,
    IReadOnlyList<ReportCountRow> Statuses,
    IReadOnlyList<ReportCountRow> Categories,
    IReadOnlyList<ReportTechnicianRow> Technicians,
    IReadOnlyList<ReportInvoiceRow> Invoices,
    IReadOnlyList<ReportServiceRow> Services,
    IReadOnlyList<ReportServiceRow> CompletedServices);

public static class ReportPdfService
{
    public static readonly string[] SupportedTypes = ["Full Summary", "Financial", "Performance", "Service Summary"];
    private static readonly CultureInfo Philippines = CultureInfo.GetCultureInfo("en-PH");
    private const string Ink = "#172033";
    private const string Muted = "#667085";
    private const string Blue = "#0878F9";
    private const string Line = "#E4E9F0";
    private const string Surface = "#F7F9FC";
    private const string Green = "#067647";
    private const string Amber = "#B54708";

    public static byte[] Generate(ReportSnapshot report)
    {
        if (!SupportedTypes.Contains(report.Type, StringComparer.Ordinal))
            throw new ArgumentException("Unsupported report type.", nameof(report));

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(34);
                page.MarginVertical(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontFamily("Arial").FontSize(9).FontColor(Ink));
                page.Header().Element(container => Header(container, report));
                page.Content().PaddingTop(22).Element(container => Content(container, report));
                page.Footer().Element(container => Footer(container, report));
            });
        }).GeneratePdf();
    }

    private static void Header(IContainer container, ReportSnapshot report)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("HOMESERVE IT").FontSize(10).Bold().FontColor(Blue).LetterSpacing(0.12f);
                    left.Item().PaddingTop(5).Text(report.Type).FontSize(22).Bold().FontColor(Ink);
                    left.Item().PaddingTop(3).Text("Management report").FontSize(10).FontColor(Muted);
                });
                row.ConstantItem(178).AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text("REPORTING PERIOD").FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.1f);
                    right.Item().PaddingTop(4).AlignRight().Text($"{report.PeriodStart:MMM d, yyyy} - {report.PeriodEnd:MMM d, yyyy}").FontSize(10).Bold();
                    right.Item().PaddingTop(3).AlignRight().Text($"Generated {report.GeneratedAt:MMM d, yyyy, h:mm tt} UTC").FontSize(8).FontColor(Muted);
                });
            });
            column.Item().PaddingTop(16).Height(3).Background(Blue);
        });
    }

    private static void Footer(IContainer container, ReportSnapshot report)
    {
        container.PaddingTop(12).BorderTop(1).BorderColor(Line).Row(row =>
        {
            row.RelativeItem().Text("HomeServe IT - Internal management report").FontSize(7.5f).FontColor(Muted);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(7.5f).FontColor(Muted));
                text.Span($"{report.Type}  |  Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    private static void Content(IContainer container, ReportSnapshot report)
    {
        container.Column(column =>
        {
            column.Spacing(18);
            if (report.Type == "Financial") Financial(column, report);
            else if (report.Type == "Performance") Performance(column, report);
            else if (report.Type == "Service Summary") ServiceSummary(column, report);
            else FullSummary(column, report);
        });
    }

    private static void FullSummary(ColumnDescriptor column, ReportSnapshot report)
    {
        column.Item().Element(c => Intro(c, "At-a-glance business health", "A balanced view of revenue, delivery volume, turnaround, and the current service pipeline."));
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Metric(c, "PAID REVENUE", Money(report.Revenue), Trend(report.Revenue, report.PreviousRevenue), Blue));
            row.Spacing(8);
            row.RelativeItem().Element(c => Metric(c, "JOBS COMPLETED", report.CompletedJobs.ToString("N0"), Trend(report.CompletedJobs, report.PreviousCompletedJobs), Green));
            row.RelativeItem().Element(c => Metric(c, "AVG. TURNAROUND", Days(report.AverageResolutionDays), ResolutionTrend(report), Amber));
        });
        column.Item().Element(c => SectionTitle(c, "Financial pulse", "Cash collected and open balances in the selected period"));
        column.Item().Element(c => ComparisonTable(c, report));
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Breakdown(c, "Service mix", report.Categories, "No service categories in this period."));
            row.Spacing(12);
            row.RelativeItem().Element(c => Breakdown(c, "Pipeline", report.Statuses, "No service requests in this period."));
        });
        column.Item().Element(c => TechnicianTable(c, report.Technicians.Take(8).ToList()));
    }

    private static void Financial(ColumnDescriptor column, ReportSnapshot report)
    {
        var paymentRate = report.PaidInvoices + report.UnpaidInvoices == 0 ? 0 : report.PaidInvoices * 100d / (report.PaidInvoices + report.UnpaidInvoices);
        column.Item().Element(c => Intro(c, "Revenue and collections", "A focused view of paid revenue, outstanding balances, invoice activity, and period-over-period movement."));
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Metric(c, "PAID REVENUE", Money(report.Revenue), Trend(report.Revenue, report.PreviousRevenue), Blue));
            row.Spacing(8);
            row.RelativeItem().Element(c => Metric(c, "OUTSTANDING", Money(report.Outstanding), $"{report.UnpaidInvoices:N0} unpaid invoice(s)", Amber));
            row.RelativeItem().Element(c => Metric(c, "PAYMENT RATE", $"{paymentRate:0}%", $"{report.PaidInvoices:N0} of {report.PaidInvoices + report.UnpaidInvoices:N0} paid", Green));
        });
        column.Item().Element(c => SectionTitle(c, "Period comparison", "Paid invoice performance against the preceding equivalent period"));
        column.Item().Element(c => ComparisonTable(c, report, financialOnly: true));
        column.Item().Element(c => InvoiceTable(c, report.Invoices));
    }

    private static void Performance(ColumnDescriptor column, ReportSnapshot report)
    {
        var jobsPerTechnician = report.Technicians.Count == 0 ? 0 : report.CompletedJobs / (double)report.Technicians.Count;
        column.Item().Element(c => Intro(c, "Delivery performance", "Completion volume and technician throughput for work approved by customers during the selected period."));
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Metric(c, "JOBS COMPLETED", report.CompletedJobs.ToString("N0"), Trend(report.CompletedJobs, report.PreviousCompletedJobs), Green));
            row.Spacing(8);
            row.RelativeItem().Element(c => Metric(c, "AVG. TURNAROUND", Days(report.AverageResolutionDays), ResolutionTrend(report), Blue));
            row.RelativeItem().Element(c => Metric(c, "JOBS / TECHNICIAN", jobsPerTechnician.ToString("0.0"), $"{report.Technicians.Count:N0} contributor(s)", Amber));
        });
        column.Item().Element(c => TechnicianTable(c, report.Technicians));
        column.Item().Element(c => ServiceTable(c, report.CompletedServices, "Recently completed services"));
    }

    private static void ServiceSummary(ColumnDescriptor column, ReportSnapshot report)
    {
        var total = report.Statuses.Sum(item => item.Count);
        var completed = report.Statuses.FirstOrDefault(item => item.Label == "Completed")?.Count ?? 0;
        var awaitingApproval = report.Statuses.FirstOrDefault(item => item.Label == "Awaiting Customer Approval")?.Count ?? 0;
        column.Item().Element(c => Intro(c, "Service operations", "Request volume, service categories, current status distribution, and scheduled work for the selected period."));
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Metric(c, "TOTAL REQUESTS", total.ToString("N0"), "Scheduled within this period", Blue));
            row.Spacing(8);
            row.RelativeItem().Element(c => Metric(c, "COMPLETED", completed.ToString("N0"), "Scheduled requests now completed", Green));
            row.RelativeItem().Element(c => Metric(c, "AWAITING APPROVAL", awaitingApproval.ToString("N0"), "Customer action required", Amber));
        });
        column.Item().Row(row =>
        {
            row.RelativeItem().Element(c => Breakdown(c, "Status distribution", report.Statuses, "No service requests in this period."));
            row.Spacing(12);
            row.RelativeItem().Element(c => Breakdown(c, "Service categories", report.Categories, "No service categories in this period."));
        });
        column.Item().Element(c => ServiceTable(c, report.Services, "Service request register"));
    }

    private static void Intro(IContainer container, string title, string description) =>
        container.Background(Surface).Border(1).BorderColor(Line).Padding(14).Column(column =>
        {
            column.Item().Text(title).FontSize(12).Bold();
            column.Item().PaddingTop(4).Text(description).FontSize(8.5f).FontColor(Muted).LineHeight(1.35f);
        });

    private static void Metric(IContainer container, string label, string value, string detail, string accent) =>
        container.MinHeight(86).Border(1).BorderColor(Line).Background(Colors.White).Padding(12).Column(column =>
        {
            column.Item().Height(3).Width(28).Background(accent);
            column.Item().PaddingTop(9).Text(label).FontSize(7).Bold().FontColor(Muted).LetterSpacing(0.08f);
            column.Item().PaddingTop(3).Text(value).FontSize(16).Bold().FontColor(Ink);
            column.Item().PaddingTop(3).Text(detail).FontSize(7.5f).FontColor(Muted);
        });

    private static void SectionTitle(IContainer container, string title, string subtitle) => container.Column(column =>
    {
        column.Item().Text(title).FontSize(12).Bold();
        column.Item().PaddingTop(2).Text(subtitle).FontSize(8).FontColor(Muted);
    });

    private static void ComparisonTable(IContainer container, ReportSnapshot report, bool financialOnly = false)
    {
        var rows = new List<(string Metric, string Current, string Previous, string Change)>
        {
            ("Paid revenue", Money(report.Revenue), Money(report.PreviousRevenue), Trend(report.Revenue, report.PreviousRevenue))
        };
        if (!financialOnly)
        {
            rows.Add(("Jobs completed", report.CompletedJobs.ToString("N0"), report.PreviousCompletedJobs.ToString("N0"), Trend(report.CompletedJobs, report.PreviousCompletedJobs)));
            rows.Add(("Average turnaround", Days(report.AverageResolutionDays), Days(report.PreviousAverageResolutionDays), ResolutionTrend(report)));
        }

        StandardTable(container, ["METRIC", "CURRENT PERIOD", "PREVIOUS PERIOD", "CHANGE"], [2.1f, 1.4f, 1.4f, 1.1f], table =>
        {
            foreach (var row in rows)
            {
                BodyCell(table, row.Metric, true);
                BodyCell(table, row.Current);
                BodyCell(table, row.Previous);
                var negativeIsBetter = row.Metric == "Average turnaround";
                var unfavorable = negativeIsBetter ? row.Change.StartsWith("+") : row.Change.StartsWith("-");
                BodyCell(table, row.Change, color: unfavorable ? Amber : Green);
            }
        });
    }

    private static void Breakdown(IContainer container, string title, IReadOnlyList<ReportCountRow> rows, string emptyText)
    {
        container.Border(1).BorderColor(Line).Padding(13).Column(column =>
        {
            column.Item().Text(title).FontSize(11).Bold();
            if (rows.Count == 0)
            {
                column.Item().PaddingTop(14).PaddingBottom(8).Text(emptyText).FontSize(8).FontColor(Muted);
                return;
            }

            var max = Math.Max(1, rows.Max(item => item.Count));
            foreach (var item in rows.Take(7))
            {
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text(item.Label).FontSize(8).SemiBold();
                    row.ConstantItem(28).AlignRight().Text(item.Count.ToString("N0")).FontSize(8).Bold();
                });
                column.Item().PaddingTop(4).Height(4).Background(Line).AlignLeft().Width((float)item.Count / max * 190).Background(Blue);
            }
        });
    }

    private static void TechnicianTable(IContainer container, IReadOnlyList<ReportTechnicianRow> technicians)
    {
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "Technician performance", "Customer-approved completions and average turnaround"));
            column.Item().PaddingTop(8).Element(c =>
            {
                if (technicians.Count == 0) Empty(c, "No technician completions in this period.");
                else StandardTable(c, ["TECHNICIAN", "COMPLETED JOBS", "AVG. TURNAROUND"], [2.3f, 1.1f, 1.2f], table =>
                {
                    foreach (var technician in technicians)
                    {
                        BodyCell(table, technician.Name, true);
                        BodyCell(table, technician.JobsCompleted.ToString("N0"));
                        BodyCell(table, Days(technician.AverageResolutionDays));
                    }
                });
            });
        });
    }

    private static void InvoiceTable(IContainer container, IReadOnlyList<ReportInvoiceRow> invoices)
    {
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, "Invoice register", "Invoices issued during the selected period"));
            column.Item().PaddingTop(8).Element(c =>
            {
                if (invoices.Count == 0) Empty(c, "No invoices were issued in this period.");
                else StandardTable(c, ["INVOICE", "CUSTOMER", "ISSUED", "AMOUNT", "STATUS"], [0.9f, 1.9f, 1.1f, 1.2f, 0.9f], table =>
                {
                    foreach (var invoice in invoices.Take(18))
                    {
                        BodyCell(table, invoice.Number, true);
                        BodyCell(table, invoice.Customer);
                        BodyCell(table, invoice.Issued.ToString("MMM d, yyyy"));
                        BodyCell(table, Money(invoice.Amount));
                        BodyCell(table, invoice.Status, color: invoice.Status == "Paid" ? Green : Amber);
                    }
                });
            });
        });
    }

    private static void ServiceTable(IContainer container, IReadOnlyList<ReportServiceRow> services, string title)
    {
        container.Column(column =>
        {
            column.Item().Element(c => SectionTitle(c, title, "Most recent records in the selected period"));
            column.Item().PaddingTop(8).Element(c =>
            {
                if (services.Count == 0) Empty(c, "No matching service requests in this period.");
                else StandardTable(c, ["REQUEST", "CATEGORY", "CUSTOMER", "TECHNICIAN", "SCHEDULED", "STATUS"], [0.8f, 1.2f, 1.3f, 1.3f, 1.1f, 1.3f], table =>
                {
                    foreach (var service in services.Take(18))
                    {
                        BodyCell(table, service.Number, true);
                        BodyCell(table, service.Category);
                        BodyCell(table, service.Customer);
                        BodyCell(table, service.Technician);
                        BodyCell(table, service.Scheduled.ToString("MMM d, yyyy"));
                        BodyCell(table, service.Status);
                    }
                });
            });
        });
    }

    private static void StandardTable(IContainer container, string[] headers, float[] widths, Action<TableDescriptor> content)
    {
        container.Border(1).BorderColor(Line).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var width in widths) columns.RelativeColumn(width);
            });
            table.Header(header =>
            {
                foreach (var label in headers)
                    header.Cell().Background(Ink).PaddingVertical(7).PaddingHorizontal(8).Text(label).FontSize(6.5f).Bold().FontColor(Colors.White).LetterSpacing(0.05f);
            });
            content(table);
        });
    }

    private static void BodyCell(TableDescriptor table, string value, bool bold = false, string color = Ink)
    {
        var text = table.Cell().BorderBottom(1).BorderColor(Line).PaddingVertical(6).PaddingHorizontal(8).Text(value).FontSize(7.5f).FontColor(color);
        if (bold) text.SemiBold();
    }

    private static void Empty(IContainer container, string text) =>
        container.Border(1).BorderColor(Line).Background(Surface).Padding(18).AlignCenter().Text(text).FontSize(8).FontColor(Muted);

    private static string Money(decimal value) => $"₱{value.ToString("N2", Philippines)}";
    private static string Days(double value) => value <= 0 ? "0.0 days" : $"{value:0.0} days";
    private static string Trend(decimal current, decimal previous) => previous == 0 ? (current > 0 ? "+100.0% vs prior" : "No change") : $"{(current - previous) / previous:+0.0%;-0.0%;0.0%} vs prior";
    private static string Trend(int current, int previous) => previous == 0 ? (current > 0 ? "+100.0% vs prior" : "No change") : $"{(current - previous) / (double)previous:+0.0%;-0.0%;0.0%} vs prior";
    private static string ResolutionTrend(ReportSnapshot report)
    {
        if (report.PreviousAverageResolutionDays <= 0) return "No prior baseline";
        var difference = report.AverageResolutionDays - report.PreviousAverageResolutionDays;
        return difference == 0 ? "No change" : $"{difference:+0.0;-0.0} days vs prior";
    }
}
