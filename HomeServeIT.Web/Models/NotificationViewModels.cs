namespace HomeServeIT.Web.Models;

public class NotificationFeedViewModel
{
    public IReadOnlyList<UserNotification> Notifications { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public IReadOnlyDictionary<string, int> CategoryCounts { get; init; } = new Dictionary<string, int>();
    public string ActiveCategory { get; init; } = "All";
    public string Area { get; init; } = string.Empty;
    public int UnreadCount { get; init; }
    public int TotalCount { get; init; }
    public int FilteredCount { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; }
    public int TotalPages { get; init; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int PageStart => FilteredCount == 0 || PageSize <= 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int PageEnd => PageSize <= 0 ? 0 : Math.Min(CurrentPage * PageSize, FilteredCount);

    public string AllNotificationsUrl => $"/{Area}/Notifications";
}
