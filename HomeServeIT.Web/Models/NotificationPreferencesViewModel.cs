namespace HomeServeIT.Web.Models;

public sealed class NotificationPreferencesViewModel
{
    public bool JobUpdates { get; set; } = true;
    public bool Quotations { get; set; } = true;
    public bool Billing { get; set; } = true;

    public static NotificationPreferencesViewModel FromUser(ApplicationUser user) => new()
    {
        JobUpdates = user.PrefApptReminders,
        Quotations = user.PrefQuotations,
        Billing = user.PrefInvoices
    };
}
