using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeServeIT.Web.Data;

public static class BusinessActivitySeeder
{
    private const string ActivityMarker = "Quarterly office network refresh and secure backup planning for Maria Santos.";
    private const string LegacySeedMarker = "Demo seed: completed workstation repair with realistic workflow data.";

    public static async Task SeedAsync(IServiceProvider services, IHostEnvironment environment)
    {
        DemoDataSeeder.RequireSafeTarget(services, environment);

        var configuration = services.GetRequiredService<IConfiguration>();
        var password = configuration["BusinessData:Password"] ?? configuration["DemoData:Password"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Set BusinessData__Password before seeding company data.");

        var context = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var profiles = services.GetRequiredService<HomeServeIT.Web.Services.AccountProfileService>();

        var operationsLead = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:OperationsEmail"] ?? "sofia@homeserveit.local",
            "Sofia Reyes", "+639221234567", "Poblacion, Davao City", Roles.Administrator);
        var networkTechnician = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:NetworkTechnicianEmail"] ?? "aisha@homeserveit.local",
            "Aisha Lim", "+639231234567", "Buhangin, Davao City", Roles.Technician,
            "Network and security systems");
        var fieldTechnician = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:FieldTechnicianEmail"] ?? "jordan@homeserveit.local",
            "Jordan Cruz", "+639241234567", "Matina, Davao City", Roles.Technician,
            "Hardware and data recovery");
        var mariaUser = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:MariaEmail"] ?? "maria@homeserveit.local",
            "Maria Santos", "+639251234567", "Buhangin, Davao City", Roles.Customer);
        var rubenUser = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:RubenEmail"] ?? "ruben@homeserveit.local",
            "Ruben Dela Cruz", "+639261234567", "Matina, Davao City", Roles.Customer);
        var lizaUser = await DemoDataSeeder.EnsureAccountAsync(
            context, users, profiles, password,
            configuration["BusinessData:LizaEmail"] ?? "liza@homeserveit.local",
            "Liza Navarro", "+639271234567", "Lanang, Davao City", Roles.Customer);

        await NormalizeExistingLabelsAsync(context);
        if (await context.ServiceRequests.AnyAsync(request => request.IssueDescription == ActivityMarker))
        {
            Console.WriteLine("Company activity already exists; no additional records were changed.");
            return;
        }

        var maria = await context.Customers.SingleAsync(customer => customer.UserID == mariaUser.Id);
        var ruben = await context.Customers.SingleAsync(customer => customer.UserID == rubenUser.Id);
        var liza = await context.Customers.SingleAsync(customer => customer.UserID == lizaUser.Id);
        var aisha = await context.Technicians.SingleAsync(technician => technician.UserID == networkTechnician.Id);
        var jordan = await context.Technicians.SingleAsync(technician => technician.UserID == fieldTechnician.Id);
        var now = DateTime.UtcNow;

        await using var transaction = await context.Database.BeginTransactionAsync();

        context.Devices.AddRange(
            new Device
            {
                CustomerID = maria.CustomerID,
                Name = "Dell Latitude 5440",
                Type = "Laptop",
                SerialNumber = "DL5440-MARIA-01",
                DateAdded = now.AddDays(-120)
            },
            new Device
            {
                CustomerID = maria.CustomerID,
                Name = "Ubiquiti UniFi Dream Router",
                Type = "Network Device",
                SerialNumber = "UDR-MARIA-01",
                DateAdded = now.AddDays(-92)
            },
            new Device
            {
                CustomerID = ruben.CustomerID,
                Name = "Lenovo ThinkPad E14",
                Type = "Laptop",
                SerialNumber = "LNV-E14-RUBEN-01",
                DateAdded = now.AddDays(-86)
            },
            new Device
            {
                CustomerID = ruben.CustomerID,
                Name = "Synology DS224+",
                Type = "Network Storage",
                SerialNumber = "SYN-DS224-RUBEN-01",
                DateAdded = now.AddDays(-70)
            },
            new Device
            {
                CustomerID = liza.CustomerID,
                Name = "MacBook Air M2",
                Type = "Laptop",
                SerialNumber = "MBA-M2-LIZA-01",
                DateAdded = now.AddDays(-54)
            },
            new Device
            {
                CustomerID = liza.CustomerID,
                Name = "Brother MFC-L8900CDW",
                Type = "Printer",
                SerialNumber = "BTH-L8900-LIZA-01",
                DateAdded = now.AddDays(-51)
            });

        var accessPoint = new InventoryItem
        {
            ItemName = "UniFi U6+ Access Point",
            SKU = "NET-U6P-001",
            Category = "Networking",
            StockQuantity = 11,
            UnitCost = 6200,
            UnitPrice = 7800,
            ReorderLevel = 2
        };
        var nvmeDrive = new InventoryItem
        {
            ItemName = "2TB NVMe Solid State Drive",
            SKU = "STR-NVME2T-001",
            Category = "Storage",
            StockQuantity = 5,
            UnitCost = 5400,
            UnitPrice = 6900,
            ReorderLevel = 2
        };
        var networkSwitch = new InventoryItem
        {
            ItemName = "8-Port Gigabit Managed Switch",
            SKU = "NET-SW8G-001",
            Category = "Networking",
            StockQuantity = 11,
            UnitCost = 1800,
            UnitPrice = 2400,
            ReorderLevel = 3
        };
        var ups = new InventoryItem
        {
            ItemName = "1200VA Line-Interactive UPS",
            SKU = "PWR-UPS12-001",
            Category = "Power Protection",
            StockQuantity = 4,
            UnitCost = 3300,
            UnitPrice = 4200,
            ReorderLevel = 2
        };
        var toner = new InventoryItem
        {
            ItemName = "Brother TN-3479 Black Toner",
            SKU = "PRT-TN3479-001",
            Category = "Printing",
            StockQuantity = 14,
            UnitCost = 2400,
            UnitPrice = 2950,
            ReorderLevel = 4
        };
        context.InventoryItems.AddRange(accessPoint, nvmeDrive, networkSwitch, ups, toner);
        await context.SaveChangesAsync();

        var mariaCompleted = new ServiceRequest
        {
            CustomerID = maria.CustomerID,
            TechID = aisha.TechID,
            IssueDescription = "Quarterly office network refresh and secure backup planning for Maria Santos.",
            ScheduledDate = now.AddDays(-42),
            EstimatedDeadline = now.AddDays(-40),
            Status = "Completed",
            Priority = "Normal",
            CompletedDate = now.AddDays(-40),
            ServiceCategory = "Network Setup",
            Check1_Diagnostic = true,
            Check2_Hardware = true,
            Check3_Firmware = true,
            Check4_QA = true,
            Check5_Handover = true
        };
        var rubenCompleted = new ServiceRequest
        {
            CustomerID = ruben.CustomerID,
            TechID = jordan.TechID,
            IssueDescription = "NAS storage recovery and UPS replacement after repeated brownouts.",
            ScheduledDate = now.AddDays(-65),
            EstimatedDeadline = now.AddDays(-63),
            Status = "Completed",
            Priority = "Urgent",
            CompletedDate = now.AddDays(-63),
            ServiceCategory = "Data Recovery",
            Check1_Diagnostic = true,
            Check2_Hardware = true,
            Check3_Firmware = true,
            Check4_QA = true,
            Check5_Handover = true
        };
        var lizaReview = new ServiceRequest
        {
            CustomerID = liza.CustomerID,
            TechID = aisha.TechID,
            IssueDescription = "Printer fleet maintenance and laptop storage optimization.",
            ScheduledDate = now.AddDays(-5),
            EstimatedDeadline = now.AddDays(-3),
            Status = "PendingCustomerReview",
            Priority = "Normal",
            ServiceCategory = "Hardware Repair",
            Check1_Diagnostic = true,
            Check2_Hardware = true,
            Check3_Firmware = true,
            Check4_QA = true,
            Check5_Handover = true
        };
        var mariaPending = new ServiceRequest
        {
            CustomerID = maria.CustomerID,
            TechID = aisha.TechID,
            IssueDescription = "Add a managed switch and extend Wi-Fi coverage to the training room.",
            ScheduledDate = now.AddDays(2),
            EstimatedDeadline = now.AddDays(3),
            Status = "Pending",
            Priority = "Normal",
            ServiceCategory = "Network Setup"
        };
        var rubenDiagnosing = new ServiceRequest
        {
            CustomerID = ruben.CustomerID,
            TechID = jordan.TechID,
            IssueDescription = "Laptop freezes during large file transfers and video calls.",
            ScheduledDate = now.AddHours(-1),
            EstimatedDeadline = now.AddHours(4),
            Status = "Diagnosing",
            Priority = "Urgent",
            ServiceCategory = "Hardware Repair"
        };
        var mariaQuotation = new ServiceRequest
        {
            CustomerID = maria.CustomerID,
            TechID = jordan.TechID,
            IssueDescription = "Branch office backup upgrade with encrypted off-site storage.",
            ScheduledDate = now.AddDays(5),
            EstimatedDeadline = now.AddDays(7),
            Status = "PendingAdminApproval",
            Priority = "Normal",
            ServiceCategory = "Data Recovery"
        };
        var rubenPending = new ServiceRequest
        {
            CustomerID = ruben.CustomerID,
            IssueDescription = "Set up a secure workstation for a new finance team member.",
            ScheduledDate = now.AddDays(8),
            Status = "Pending",
            Priority = "Low",
            ServiceCategory = "Software / OS"
        };
        var lizaCancelled = new ServiceRequest
        {
            CustomerID = liza.CustomerID,
            IssueDescription = "Home office camera and access-control installation.",
            ScheduledDate = now.AddDays(-12),
            Status = "Cancelled",
            Priority = "Normal",
            ServiceCategory = "CCTV / Security",
            IsArchived = true,
            IsCancellationRequested = true,
            CancellationReason = "The customer postponed the installation indefinitely.",
            CancellationStatus = "Approved"
        };
        context.ServiceRequests.AddRange(
            mariaCompleted, rubenCompleted, lizaReview, mariaPending,
            rubenDiagnosing, mariaQuotation, rubenPending, lizaCancelled);
        await context.SaveChangesAsync();

        context.Invoices.AddRange(
            new Invoice
            {
                RequestID = mariaCompleted.RequestID,
                TotalAmount = 9600,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-40),
                BreakdownDetails = "Network assessment ₱800; access point ₱7,800; installation and tuning ₱1,000"
            },
            new Invoice
            {
                RequestID = rubenCompleted.RequestID,
                TotalAmount = 8200,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-63),
                BreakdownDetails = "Storage recovery ₱2,000; managed switch ₱2,400; UPS replacement ₱2,800; testing ₱1,000"
            },
            new Invoice
            {
                RequestID = lizaReview.RequestID,
                TotalAmount = 12050,
                PaymentStatus = "Paid",
                IsQuotation = true,
                QuotationStatus = "Approved",
                DateIssued = now.AddDays(-4),
                BreakdownDetails = "NVMe storage upgrade ₱6,900; toner ₱2,950; installation and maintenance ₱2,200"
            },
            new Invoice
            {
                RequestID = mariaPending.RequestID,
                TotalAmount = 3400,
                PaymentStatus = "Unpaid",
                IsQuotation = true,
                QuotationStatus = "ApprovedByAdmin",
                DateIssued = now.AddDays(-1),
                BreakdownDetails = "Managed switch ₱2,400; site setup and testing ₱1,000"
            },
            new Invoice
            {
                RequestID = mariaQuotation.RequestID,
                TotalAmount = 26000,
                PaymentStatus = "Unpaid",
                IsQuotation = true,
                QuotationStatus = "PendingAdmin",
                DateIssued = now,
                BreakdownDetails = "Two access points ₱15,600; NVMe backup storage ₱6,900; encrypted deployment ₱3,500"
            });

        context.JobInventoryUsages.AddRange(
            new JobInventoryUsage { RequestID = mariaCompleted.RequestID, ItemID = accessPoint.ItemID, Quantity = 1, UnitPrice = accessPoint.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = rubenCompleted.RequestID, ItemID = networkSwitch.ItemID, Quantity = 1, UnitPrice = networkSwitch.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = rubenCompleted.RequestID, ItemID = ups.ItemID, Quantity = 1, UnitPrice = ups.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = lizaReview.RequestID, ItemID = nvmeDrive.ItemID, Quantity = 1, UnitPrice = nvmeDrive.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = lizaReview.RequestID, ItemID = toner.ItemID, Quantity = 1, UnitPrice = toner.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = mariaPending.RequestID, ItemID = networkSwitch.ItemID, Quantity = 1, UnitPrice = networkSwitch.UnitPrice, IsDeducted = false },
            new JobInventoryUsage { RequestID = mariaQuotation.RequestID, ItemID = accessPoint.ItemID, Quantity = 2, UnitPrice = accessPoint.UnitPrice, IsDeducted = false },
            new JobInventoryUsage { RequestID = mariaQuotation.RequestID, ItemID = nvmeDrive.ItemID, Quantity = 1, UnitPrice = nvmeDrive.UnitPrice, IsDeducted = false });

        context.StockMovements.AddRange(
            DemoDataSeeder.InitialStock(accessPoint, 8, now.AddDays(-135)),
            DemoDataSeeder.InitialStock(nvmeDrive, 6, now.AddDays(-135)),
            DemoDataSeeder.InitialStock(networkSwitch, 8, now.AddDays(-135)),
            DemoDataSeeder.InitialStock(ups, 5, now.AddDays(-135)),
            DemoDataSeeder.InitialStock(toner, 15, now.AddDays(-135)),
            Restock(accessPoint, 4, now.AddDays(-58), "PO-1048", "Sofia Reyes"),
            Restock(networkSwitch, 4, now.AddDays(-31), "PO-1061", "Sofia Reyes"),
            DemoDataSeeder.JobUsage(accessPoint, mariaCompleted, -1, now.AddDays(-40), "Aisha Lim"),
            DemoDataSeeder.JobUsage(networkSwitch, rubenCompleted, -1, now.AddDays(-63), "Jordan Cruz"),
            DemoDataSeeder.JobUsage(ups, rubenCompleted, -1, now.AddDays(-63), "Jordan Cruz"),
            DemoDataSeeder.JobUsage(nvmeDrive, lizaReview, -1, now.AddDays(-3), "Aisha Lim"),
            DemoDataSeeder.JobUsage(toner, lizaReview, -1, now.AddDays(-3), "Aisha Lim"));

        context.ServiceMessages.AddRange(
            Message(mariaCompleted, networkTechnician.Id, "The new access point is online and the backup schedule has been tested against the office NAS.", now.AddDays(-40), true),
            Message(mariaCompleted, mariaUser.Id, "Thanks, the training room is now connected and the backup report came through.", now.AddDays(-39), true),
            Message(rubenCompleted, fieldTechnician.Id, "The recovery completed successfully. I also replaced the failing UPS battery and ran a restore test.", now.AddDays(-63), true),
            Message(rubenDiagnosing, rubenUser.Id, "The freezes happen mostly when the shared drive is syncing. I can reproduce it during large uploads.", now.AddMinutes(-35), false),
            Message(rubenDiagnosing, fieldTechnician.Id, "I’m checking the SSD health and driver logs now. I’ll update the quotation once the root cause is confirmed.", now.AddMinutes(-20), false),
            Message(lizaReview, networkTechnician.Id, "The maintenance checklist and storage upgrade proof are ready for your review.", now.AddDays(-3), false));

        context.JobDeliverables.AddRange(
            Deliverable(mariaCompleted, "FinalProof", "Network refresh completed with access-point coverage test and verified backup schedule.", now.AddDays(-40)),
            Deliverable(rubenCompleted, "FinalProof", "NAS recovery completed and a sample archive was restored successfully.", now.AddDays(-63)),
            Deliverable(lizaReview, "FinalProof", "Storage optimization and printer maintenance completed; review the attached service summary.", now.AddDays(-3)));

        context.SupportTickets.AddRange(
            new SupportTicket
            {
                CustomerID = maria.CustomerID,
                Subject = "Request for monthly backup report",
                Message = "Could we receive a monthly confirmation that the branch backup is completing successfully?",
                Status = "Open",
                CreatedAt = now.AddDays(-1)
            },
            new SupportTicket
            {
                CustomerID = ruben.CustomerID,
                Subject = "Follow-up on workstation freezes",
                Message = "The issue returned briefly this morning while the shared drive was syncing.",
                AdminResponse = "Jordan is investigating the storage health and will update the active request.",
                Status = "In Progress",
                CreatedAt = now.AddHours(-5),
                RespondedAt = now.AddHours(-4)
            },
            new SupportTicket
            {
                CustomerID = liza.CustomerID,
                Subject = "Invoice copy requested",
                Message = "Please send a copy of the completed maintenance invoice for our records.",
                AdminResponse = "A copy of the invoice is available from Bills & Payments.",
                Status = "Resolved",
                CreatedAt = now.AddDays(-9),
                RespondedAt = now.AddDays(-8)
            });

        context.UserNotifications.AddRange(
            Notification(operationsLead, Roles.Administrator, "Quotations", "file-text", "Quotation awaiting review", "Maria Santos has a branch backup upgrade quotation awaiting review.", $"/Admin/Finance/Quotations#{mariaQuotation.RequestID}", $"operations:admin:quotation:{mariaQuotation.RequestID}", now),
            Notification(networkTechnician, Roles.Technician, "Jobs", "wrench", "Customer approval received", "Liza Navarro’s completed maintenance job is ready for customer review.", $"/Technician/AssignedJobs?jobId={lizaReview.RequestID}", $"operations:technician:review:{lizaReview.RequestID}", now.AddDays(-3)),
            Notification(fieldTechnician, Roles.Technician, "Jobs", "tool", "Diagnosis in progress", "Ruben Dela Cruz’s laptop issue is being investigated today.", $"/Technician/AssignedJobs?jobId={rubenDiagnosing.RequestID}", $"operations:technician:diagnosis:{rubenDiagnosing.RequestID}", now.AddMinutes(-20)),
            Notification(mariaUser, Roles.Customer, "Billing", "credit-card", "Quotation ready for review", "Your training-room network quotation has been approved and is ready for payment.", $"/Customer/Quotations?requestId={mariaPending.RequestID}", $"operations:customer:quotation:{mariaPending.RequestID}", now.AddDays(-1)),
            Notification(rubenUser, Roles.Customer, "Jobs", "wrench", "Technician update", "Jordan Cruz is investigating the workstation issue and will share the next update soon.", $"/Customer/ServiceRequests?jobId={rubenDiagnosing.RequestID}", $"operations:customer:diagnosis:{rubenDiagnosing.RequestID}", now.AddMinutes(-20)),
            Notification(lizaUser, Roles.Customer, "Jobs", "check-circle", "Service proof ready", "Your completed maintenance work is ready for review.", $"/Customer/ServiceRequests?jobId={lizaReview.RequestID}", $"operations:customer:review:{lizaReview.RequestID}", now.AddDays(-3)));

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine("Additional company accounts, operations, and activity were added.");
        Console.WriteLine("All account passwords use BusinessData__Password and were not printed.");
    }

    private static async Task NormalizeExistingLabelsAsync(ApplicationDbContext context)
    {
        var legacyRequest = await context.ServiceRequests
            .SingleOrDefaultAsync(request => request.IssueDescription == LegacySeedMarker);
        if (legacyRequest != null)
            legacyRequest.IssueDescription = "Completed workstation repair and storage upgrade for Kyle Cabanig.";

        var oldDevices = await context.Devices
            .Where(device => device.SerialNumber != null && device.SerialNumber.StartsWith("DEMO-"))
            .ToListAsync();
        foreach (var device in oldDevices)
            device.SerialNumber = device.SerialNumber!.Replace("DEMO-", "HIT-", StringComparison.Ordinal);

        var oldMovements = await context.StockMovements
            .Where(movement => movement.DestinationOrSource == "Demo opening stock"
                || movement.Notes == "Fictional demo inventory record."
                || movement.Notes == "Fictional demo inventory usage record.")
            .ToListAsync();
        foreach (var movement in oldMovements)
        {
            if (movement.DestinationOrSource == "Demo opening stock")
                movement.DestinationOrSource = "Warehouse receiving";
            movement.Notes = movement.MovementType == "Initial Stock"
                ? "Opening stock received into the Davao service warehouse."
                : "Material consumed during a completed service operation.";
        }

        var oldNotifications = await context.UserNotifications
            .Where(notification => notification.SourceKey.StartsWith("demo:")
                || notification.Message.Contains("demo"))
            .ToListAsync();
        foreach (var notification in oldNotifications)
        {
            notification.SourceKey = notification.SourceKey.Replace("demo:", "operations:", StringComparison.Ordinal);
            notification.Message = notification.Message.Replace("A demo hardware-upgrade", "A hardware-upgrade", StringComparison.OrdinalIgnoreCase);
        }

        if (legacyRequest != null || oldDevices.Count > 0 || oldMovements.Count > 0 || oldNotifications.Count > 0)
            await context.SaveChangesAsync();
    }

    private static StockMovement Restock(InventoryItem item, int quantity, DateTime timestamp,
        string purchaseOrder, string performedBy) => new()
    {
        ItemID = item.ItemID,
        MovementType = "Restock",
        Quantity = quantity,
        UnitCost = item.UnitCost,
        UnitPrice = item.UnitPrice,
        Timestamp = timestamp,
        PerformedBy = performedBy,
        DestinationOrSource = $"Supplier delivery {purchaseOrder}",
        Notes = "Stock replenishment received and counted by operations."
    };

    private static ServiceMessage Message(ServiceRequest request, string senderId, string content,
        DateTime timestamp, bool isRead) => new()
    {
        RequestID = request.RequestID,
        SenderID = senderId,
        Content = content,
        Timestamp = timestamp,
        IsRead = isRead
    };

    private static JobDeliverable Deliverable(ServiceRequest request, string phase, string description,
        DateTime createdAt) => new()
    {
        RequestID = request.RequestID,
        Phase = phase,
        Description = description,
        CreatedAt = createdAt
    };

    private static UserNotification Notification(ApplicationUser recipient, string role, string category,
        string icon, string title, string message, string actionUrl, string sourceKey, DateTime createdAt) => new()
    {
        RecipientUserID = recipient.Id,
        AudienceRole = role,
        Category = category,
        Icon = icon,
        Title = title,
        Message = message,
        ActionUrl = actionUrl,
        SourceKey = sourceKey,
        CreatedAt = createdAt
    };
}
