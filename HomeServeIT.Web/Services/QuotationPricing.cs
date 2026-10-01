using HomeServeIT.Web.Models;

namespace HomeServeIT.Web.Services;

public sealed record QuotationApprovalLine(
    string Name,
    string Sku,
    int Quantity,
    int Available,
    decimal UnitPrice,
    bool Sufficient);

public sealed record QuotationApprovalPreview(
    decimal QuotedPartsTotal,
    decimal LaborAmount,
    decimal CurrentPartsTotal,
    decimal TotalAmount,
    IReadOnlyList<QuotationApprovalLine> Parts,
    string Breakdown);

public static class QuotationPricing
{
    public static QuotationApprovalPreview Build(
        Invoice quotation,
        IEnumerable<JobInventoryUsage> usages)
    {
        var usageList = usages
            .Where(usage => usage.RequestID == quotation.RequestID)
            .OrderBy(usage => usage.UsageID)
            .ToList();
        var quotedPartsTotal = usageList.Sum(usage => usage.UnitPrice * usage.Quantity);
        var laborAmount = Math.Max(0m, quotation.TotalAmount - quotedPartsTotal);
        var currentPartsTotal = usageList.Sum(usage => usage.InventoryItem.UnitPrice * usage.Quantity);
        var totalAmount = decimal.Round(currentPartsTotal + laborAmount, 2, MidpointRounding.AwayFromZero);
        var parts = usageList
            .Select(usage => new QuotationApprovalLine(
                usage.InventoryItem.ItemName,
                usage.InventoryItem.SKU,
                usage.Quantity,
                usage.InventoryItem.StockQuantity,
                usage.InventoryItem.UnitPrice,
                usage.InventoryItem.StockQuantity >= usage.Quantity))
            .ToList();
        var breakdownLines = parts
            .Select(part => $"{part.Name} × {part.Quantity} — ₱{part.UnitPrice * part.Quantity:N2}")
            .ToList();
        if (laborAmount > 0)
            breakdownLines.Add($"Labor — ₱{laborAmount:N2}");

        return new QuotationApprovalPreview(
            quotedPartsTotal,
            laborAmount,
            currentPartsTotal,
            totalAmount,
            parts,
            string.Join("\n", breakdownLines));
    }
}
