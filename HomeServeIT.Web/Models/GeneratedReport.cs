using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeServeIT.Web.Models;

public sealed class GeneratedReport
{
    [Key]
    public int GeneratedReportID { get; set; }

    [Required, MaxLength(255)]
    public string GeneratedByUserID { get; set; } = string.Empty;

    [ForeignKey(nameof(GeneratedByUserID))]
    public ApplicationUser GeneratedByUser { get; set; } = null!;

    [Required, MaxLength(50)]
    public string ReportType { get; set; } = string.Empty;

    [Required, MaxLength(180)]
    public string FileName { get; set; } = string.Empty;

    public DateTime PeriodStartUtc { get; set; }

    public DateTime PeriodEndUtc { get; set; }

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public byte[] PdfContent { get; set; } = [];
}
