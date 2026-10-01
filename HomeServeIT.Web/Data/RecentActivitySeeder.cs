using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeServeIT.Web.Data;

public static class RecentActivitySeeder
{
    private const string CompletedRequestMarker =
        "Wi-Fi 6 coverage tune-up and secure backup verification for the Davao service office.";
    private const string NewRequestMarker =
        "Guest Wi-Fi drops during video calls in the upstairs office.";
    private const string RecentCustomerEmail = "rina@homeserveit.local";
    private const string MeshItemSku = "DEMO-NET-MESH-2026";

    public static async Task SeedAsync(IServiceProvider services, IHostEnvironment environment)
    {
        DemoDataSeeder.RequireSafeTarget(services, environment);

        var configuration = services.GetRequiredService<IConfiguration>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (await context.ServiceRequests.AnyAsync(request =>
                request.IssueDescription == CompletedRequestMarker
                || request.IssueDescription == NewRequestMarker))
        {
            Console.WriteLine("Current-period activity already exists; no additional records were changed.");
            return;
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var profiles = services.GetRequiredService<HomeServeIT.Web.Services.AccountProfileService>();

        var recentCustomerUser = await EnsureRecentCustomerAsync(context, users, profiles);

        var mariaUser = await FindAccountAsync(users,
            configuration["BusinessData:MariaEmail"] ?? "maria@homeserveit.local");
        var technicianUser = await FindAccountAsync(users,
            configuration["BusinessData:NetworkTechnicianEmail"] ?? "aisha@homeserveit.local");
        var operationsUser = await FindAccountAsync(users,
            configuration["BusinessData:OperationsEmail"] ?? "sofia@homeserveit.local");

        var maria = await context.Customers.SingleAsync(customer => customer.UserID == mariaUser.Id);
        var recentCustomer = await context.Customers.SingleAsync(customer => customer.UserID == recentCustomerUser.Id);
        var technician = await context.Technicians.SingleAsync(profile => profile.UserID == technicianUser.Id);

        var localNow = DateTime.Now;
        var today = DateTime.Today;
        var completedLocal = today.DayOfWeek == DayOfWeek.Sunday || today.Day == 1
            ? localNow.AddHours(-2)
            : today.AddDays(-1).AddHours(14);
        if (completedLocal > localNow)
            completedLocal = localNow.AddHours(-2);

        var todayAppointmentLocal = today.AddHours(9);
        if (todayAppointmentLocal <= localNow)
        {
            todayAppointmentLocal = localNow.AddHours(1);
            if (todayAppointmentLocal.Date != today)
                todayAppointmentLocal = today.AddHours(23);
        }

        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        var followUpDate = today.AddDays(1);
        if (followUpDate > weekEnd)
            followUpDate = today;
        var followUpLocal = followUpDate.AddHours(followUpDate == today ? 15 : 10);

        var completedAtUtc = ToManilaUtc(completedLocal);
        var completedScheduledUtc = ToManilaUtc(completedLocal.Date.AddHours(9));
        var todayAppointmentUtc = ToManilaUtc(todayAppointmentLocal);
        var todayDeadlineUtc = todayAppointmentUtc.AddHours(3);
        var followUpUtc = ToManilaUtc(followUpLocal);
        var nowUtc = DateTime.UtcNow;

        var meshNode = new InventoryItem
        {
            ItemName = "TP-Link Deco X50 Wi-Fi 6 Mesh Node",
            SKU = MeshItemSku,
            Category = "Networking",
            StockQuantity = 2,
            UnitCost = 4300m,
            UnitPrice = 6500m,
            ReorderLevel = 3
        };

        var completedRequest = new ServiceRequest
        {
            CustomerID = maria.CustomerID,
            TechID = technician.TechID,
            IssueDescription = CompletedRequestMarker,
            ScheduledDate = completedScheduledUtc,
            EstimatedDeadline = completedAtUtc,
            CompletedDate = completedAtUtc,
            Status = "Completed",
            Priority = "Normal",
            ServiceCategory = "Network Setup",
            Check1_Diagnostic = true,
            Check2_Hardware = true,
            Check3_Firmware = true,
            Check4_QA = true,
            Check5_Handover = true
        };

        // Add a second finished visit earlier in the same Sun–Sat week when there is
        // room before today. This gives weekly charts and customer history a useful
        // timeline without inventing future-dated completion records.
        var priorWeekRequest = today.DayOfWeek > DayOfWeek.Monday
            ? new ServiceRequest
            {
                CustomerID = maria.CustomerID,
                TechID = technician.TechID,
                IssueDescription = "Follow-up on intermittent DHCP lease drops across the studio workstations.",
                ScheduledDate = ToManilaUtc(weekStart.AddDays(1).AddHours(13)),
                EstimatedDeadline = ToManilaUtc(weekStart.AddDays(1).AddHours(15)),
                CompletedDate = ToManilaUtc(weekStart.AddDays(1).AddHours(14).AddMinutes(30)),
                Status = "Completed",
                Priority = "Normal",
                ServiceCategory = "Network Setup",
                Check1_Diagnostic = true,
                Check2_Hardware = true,
                Check3_Firmware = true,
                Check4_QA = true,
                Check5_Handover = true
            }
            : null;

        var currentRequest = new ServiceRequest
        {
            CustomerID = recentCustomer.CustomerID,
            TechID = technician.TechID,
            IssueDescription = NewRequestMarker,
            ScheduledDate = todayAppointmentUtc,
            EstimatedDeadline = todayDeadlineUtc,
            Status = "Pending",
            Priority = "Urgent",
            ServiceCategory = "Network Setup"
        };

        var followUpRequest = new ServiceRequest
        {
            CustomerID = recentCustomer.CustomerID,
            IssueDescription = "Configure a secure guest network for the home office and family devices.",
            ScheduledDate = followUpUtc,
            Status = "Pending",
            Priority = "Normal",
            ServiceCategory = "Network Setup"
        };

        await using var transaction = await context.Database.BeginTransactionAsync();

        if (await context.InventoryItems.AnyAsync(item => item.SKU == MeshItemSku))
            throw new InvalidOperationException("The recent activity inventory SKU already exists without its activity marker.");

        context.InventoryItems.Add(meshNode);
        context.Devices.Add(new Device
        {
            CustomerID = recentCustomer.CustomerID,
            Name = "Lenovo ThinkPad E14 Gen 5",
            Type = "Laptop",
            SerialNumber = "HS-RINA-E14-2026",
            DateAdded = nowUtc
        });
        context.ServiceRequests.AddRange(new[] { completedRequest, priorWeekRequest, currentRequest, followUpRequest }
            .OfType<ServiceRequest>());
        await context.SaveChangesAsync();

        var invoices = new List<Invoice>
        {
            new()
            {
                RequestID = completedRequest.RequestID,
                TotalAmount = 8350m,
                PaymentStatus = "Paid",
                DateIssued = completedAtUtc.AddHours(1),
                BreakdownDetails = "Wi-Fi 6 mesh node ₱6,500; installation, coverage tuning, and backup verification ₱1,850"
            },
            new()
            {
                RequestID = currentRequest.RequestID,
                TotalAmount = 4850m,
                PaymentStatus = "Unpaid",
                IsQuotation = true,
                QuotationStatus = "PendingAdmin",
                DateIssued = nowUtc,
                BreakdownDetails = "Guest network configuration ₱2,100; access-point optimization ₱1,900; coverage test ₱850"
            }
        };
        if (priorWeekRequest != null)
        {
            invoices.Add(new Invoice
            {
                RequestID = priorWeekRequest.RequestID,
                TotalAmount = 4250m,
                PaymentStatus = "Paid",
                DateIssued = priorWeekRequest.CompletedDate!.Value.AddHours(1),
                BreakdownDetails = "Network diagnostics, DHCP configuration, and workstation connectivity verification"
            });
        }
        context.Invoices.AddRange(invoices);

        context.JobInventoryUsages.Add(new JobInventoryUsage
        {
            RequestID = completedRequest.RequestID,
            ItemID = meshNode.ItemID,
            Quantity = 1,
            UnitPrice = meshNode.UnitPrice,
            IsDeducted = true
        });
        context.StockMovements.AddRange(
            DemoDataSeeder.InitialStock(meshNode, 3, nowUtc.AddDays(-2)),
            DemoDataSeeder.JobUsage(meshNode, completedRequest, -1, completedAtUtc, "Aisha Lim"));

        context.JobDeliverables.Add(new JobDeliverable
        {
            RequestID = completedRequest.RequestID,
            Phase = "FinalProof",
            Description = "Mesh coverage verified in the upstairs office; backup schedule and a sample restore were checked.",
            CreatedAt = completedAtUtc
        });
        context.ServiceMessages.Add(new ServiceMessage
        {
            RequestID = currentRequest.RequestID,
            SenderID = recentCustomerUser.Id,
            Content = "The connection drops most often during video calls after the access point reconnects.",
            Timestamp = nowUtc.AddMinutes(-25),
            IsRead = false
        });
        context.SupportTickets.Add(new SupportTicket
        {
            CustomerID = recentCustomer.CustomerID,
            Subject = "Help checking upstairs Wi-Fi coverage",
            Message = "Could you include the upstairs office in the coverage check during the scheduled visit?",
            Status = "Open",
            CreatedAt = nowUtc.AddMinutes(-15)
        });
        context.UserNotifications.AddRange(
            new UserNotification
            {
                RecipientUserID = operationsUser.Id,
                AudienceRole = Roles.Administrator,
                Category = "Quotations",
                Icon = "file-text",
                Title = "Quotation awaiting review",
                Message = "Rina Villanueva's guest network quotation is ready for review.",
                ActionUrl = $"/Admin/Finance/Quotations#{currentRequest.RequestID}",
                SourceKey = $"recent-activity:admin-quotation:{currentRequest.RequestID}",
                CreatedAt = nowUtc.AddMinutes(-5)
            },
            new UserNotification
            {
                RecipientUserID = recentCustomerUser.Id,
                AudienceRole = Roles.Customer,
                Category = "Jobs",
                Icon = "calendar",
                Title = "Service visit scheduled",
                Message = "Your guest Wi-Fi coverage check is scheduled for today.",
                ActionUrl = $"/Customer/ServiceRequests?jobId={currentRequest.RequestID}",
                SourceKey = $"recent-activity:customer-schedule:{currentRequest.RequestID}",
                CreatedAt = nowUtc.AddMinutes(-3)
            },
            new UserNotification
            {
                RecipientUserID = technicianUser.Id,
                AudienceRole = Roles.Technician,
                Category = "Jobs",
                Icon = "wrench",
                Title = "New visit on today's schedule",
                Message = "Rina Villanueva's guest Wi-Fi coverage check is scheduled for today.",
                ActionUrl = $"/Technician/AssignedJobs?jobId={currentRequest.RequestID}",
                SourceKey = $"recent-activity:technician-schedule:{currentRequest.RequestID}",
                CreatedAt = nowUtc.AddMinutes(-2)
            });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine("Current-period fictional customer and service activity was added.");
        Console.WriteLine("The demo data includes this-week revenue, a new customer, scheduled work, a quotation, support activity, and low stock.");
    }

    private static async Task<ApplicationUser> EnsureRecentCustomerAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> users,
        HomeServeIT.Web.Services.AccountProfileService profiles)
    {
        var user = await users.FindByEmailAsync(RecentCustomerEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                FullName = "Rina Villanueva",
                Email = RecentCustomerEmail,
                UserName = RecentCustomerEmail,
                EmailConfirmed = true,
                PhoneNumber = "+639000000121",
                StreetAddress = "Buhangin",
                BarangayCity = "Davao City",
                DateCreated = DateTime.UtcNow
            };
            var result = await profiles.CreateAsync(user, Roles.Customer);
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not create the recent-activity demo customer.");
            return user;
        }

        if (!await users.IsInRoleAsync(user, Roles.Customer))
            throw new InvalidOperationException("The reserved recent-activity demo account has an unexpected role.");

        if (!await context.Customers.AnyAsync(customer => customer.UserID == user.Id))
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await profiles.EnsureDomainProfileAsync(user, Roles.Customer);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        return user;
    }

    private static async Task<ApplicationUser> FindAccountAsync(
        UserManager<ApplicationUser> users,
        string email) => await users.FindByEmailAsync(email)
        ?? throw new InvalidOperationException("Run the base company data seed before recent activity seeding.");

    private static DateTime ToManilaUtc(DateTime localDateTime)
    {
        var manila = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), manila);
    }
}
