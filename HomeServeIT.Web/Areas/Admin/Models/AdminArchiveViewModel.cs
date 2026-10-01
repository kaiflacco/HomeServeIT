using HomeServeIT.Web.Models;

namespace HomeServeIT.Web.Areas.Admin.Models;

public class AdminArchiveViewModel
{
    public IReadOnlyList<ServiceRequest> ServiceRequests { get; init; } = [];
    public IReadOnlyList<ApplicationUser> Users { get; init; } = [];
    public int ServiceRequestTotal { get; init; }
    public int ServiceRequestPage { get; init; } = 1;
    public int ServiceRequestPageCount { get; init; } = 1;
    public int UserTotal { get; init; }
    public int UserPage { get; init; } = 1;
    public int UserPageCount { get; init; } = 1;
    public string Section { get; init; } = "services";
}
