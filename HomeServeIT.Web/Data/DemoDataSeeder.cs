using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeServeIT.Web.Data;

public static class DemoDataSeeder
{
    private const string DemoDatabasePrefix = "homeserve_demo_";
    private const string SeedMarker = "Completed workstation repair and storage upgrade for Kyle Cabanig.";
    private const string LegacySeedMarker = "Demo seed: completed workstation repair with realistic workflow data.";

    public static void RequireSafeTarget(IServiceProvider services, IHostEnvironment environment)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.RequireDisposableDatabase(context, environment, DemoDatabasePrefix, "Demo data seeding");
    }

    public static async Task SeedAsync(IServiceProvider services, IHostEnvironment environment)
    {
        RequireSafeTarget(services, environment);

        var configuration = services.GetRequiredService<IConfiguration>();
        var password = configuration["BusinessData:Password"] ?? configuration["DemoData:Password"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Set BusinessData__Password before seeding company data.");

        var context = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var profiles = services.GetRequiredService<AccountProfileService>();

        var administrator = await EnsureAccountAsync(
            context, users, profiles, password,
            configuration["DemoData:AdminEmail"] ?? "admin@homeserveit.local",
            "HomeServe Administrator", "+639171234567", "Davao City", Roles.Administrator);
        var technicianUser = await EnsureAccountAsync(
            context, users, profiles, password,
            configuration["DemoData:TechnicianEmail"] ?? "marco@homeserveit.local",
            "Marco Santos", "+639181234567", "Bajada, Davao City", Roles.Technician,
            "Hardware and network support");
        var customerUser = await EnsureAccountAsync(
            context, users, profiles, password,
            configuration["DemoData:CustomerEmail"] ?? "kyle@homeserveit.local",
            "Kyle Cabanig", "+639191234567", "J.P. Laurel Avenue, Davao City", Roles.Customer);

        if (await context.ServiceRequests.AnyAsync(request =>
            request.IssueDescription == SeedMarker || request.IssueDescription == LegacySeedMarker))
        {
            Console.WriteLine("Company data already exists; no base records were changed.");
            return;
        }

        var customer = await context.Customers.SingleAsync(profile => profile.UserID == customerUser.Id);
        var technician = await context.Technicians.SingleAsync(profile => profile.UserID == technicianUser.Id);
        var now = DateTime.UtcNow;

        await using var transaction = await context.Database.BeginTransactionAsync();

        var workstation = new Device
        {
            CustomerID = customer.CustomerID,
            Name = "Dell Inspiron 15",
            Type = "Laptop",
            SerialNumber = "HIT-DI15-2024",
            DateAdded = now.AddDays(-45)
        };
        var officePrinter = new Device
        {
            CustomerID = customer.CustomerID,
            Name = "Brother HL-L2375DW",
            Type = "Printer",
            SerialNumber = "HIT-BHL2375",
            DateAdded = now.AddDays(-30)
        };
        context.Devices.AddRange(workstation, officePrinter);

        var networkCable = new InventoryItem
        {
            ItemName = "Cat6 Ethernet Cable",
            SKU = "NET-CAT6-001",
            Category = "Networking",
            StockQuantity = 35,
            UnitCost = 120,
            UnitPrice = 180,
            ReorderLevel = 10
        };
        var solidStateDrive = new InventoryItem
        {
            ItemName = "1TB Solid State Drive",
            SKU = "STR-SSD1T-001",
            Category = "Storage",
            StockQuantity = 11,
            UnitCost = 2800,
            UnitPrice = 3500,
            ReorderLevel = 3
        };
        var thermalPaste = new InventoryItem
        {
            ItemName = "High-Performance Thermal Paste",
            SKU = "HW-THERM-001",
            Category = "Hardware",
            StockQuantity = 18,
            UnitCost = 250,
            UnitPrice = 400,
            ReorderLevel = 5
        };
        var memoryKit = new InventoryItem
        {
            ItemName = "16GB DDR4 Memory Kit",
            SKU = "HW-DDR4-16G",
            Category = "Hardware",
            StockQuantity = 9,
            UnitCost = 1800,
            UnitPrice = 2300,
            ReorderLevel = 3
        };
        context.InventoryItems.AddRange(networkCable, solidStateDrive, thermalPaste, memoryKit);
        await context.SaveChangesAsync();

        var completedRequest = new ServiceRequest
        {
            CustomerID = customer.CustomerID,
            TechID = technician.TechID,
            IssueDescription = SeedMarker,
            ScheduledDate = now.AddDays(-18),
            EstimatedDeadline = now.AddDays(-17),
            Status = "Completed",
            Priority = "Normal",
            CompletedDate = now.AddDays(-17),
            ServiceCategory = "Hardware Repair",
            Check1_Diagnostic = true,
            Check2_Hardware = true,
            Check3_Firmware = true,
            Check4_QA = true,
            Check5_Handover = true
        };
        var activeRequest = new ServiceRequest
        {
            CustomerID = customer.CustomerID,
            TechID = technician.TechID,
            IssueDescription = "Office network drops connection several times each morning.",
            ScheduledDate = now.AddHours(-2),
            EstimatedDeadline = now.AddHours(3),
            Status = "In Progress",
            Priority = "Urgent",
            ServiceCategory = "Network Setup"
        };
        var quotationRequest = new ServiceRequest
        {
            CustomerID = customer.CustomerID,
            TechID = technician.TechID,
            IssueDescription = "Laptop storage upgrade and performance tune-up.",
            ScheduledDate = now.AddDays(3),
            EstimatedDeadline = now.AddDays(4),
            Status = "PendingAdminApproval",
            Priority = "Normal",
            ServiceCategory = "Hardware Repair"
        };
        var pendingRequest = new ServiceRequest
        {
            CustomerID = customer.CustomerID,
            IssueDescription = "Need help setting up a secure backup routine for family photos.",
            ScheduledDate = now.AddDays(6),
            Status = "Pending",
            Priority = "Low",
            ServiceCategory = "Data Recovery"
        };
        context.ServiceRequests.AddRange(completedRequest, activeRequest, quotationRequest, pendingRequest);
        await context.SaveChangesAsync();

        context.Invoices.AddRange(
            new Invoice
            {
                RequestID = completedRequest.RequestID,
                TotalAmount = 4200,
                PaymentStatus = "Paid",
                DateIssued = now.AddDays(-16),
                BreakdownDetails = "Diagnostics ₱500; SSD replacement ₱3,500; installation and testing ₱200"
            },
            new Invoice
            {
                RequestID = activeRequest.RequestID,
                TotalAmount = 1800,
                PaymentStatus = "Paid",
                IsQuotation = true,
                QuotationStatus = "ApprovedByAdmin",
                DateIssued = now.AddDays(-1),
                BreakdownDetails = "Network diagnosis ₱600; cable replacement ₱360; configuration and testing ₱840"
            },
            new Invoice
            {
                RequestID = quotationRequest.RequestID,
                TotalAmount = 5800,
                PaymentStatus = "Unpaid",
                IsQuotation = true,
                QuotationStatus = "PendingAdmin",
                DateIssued = now,
                BreakdownDetails = "SSD upgrade ₱3,500; installation ₱800; performance tune-up ₱1,500"
            });

        context.JobInventoryUsages.AddRange(
            new JobInventoryUsage { RequestID = completedRequest.RequestID, ItemID = networkCable.ItemID, Quantity = 3, UnitPrice = networkCable.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = completedRequest.RequestID, ItemID = solidStateDrive.ItemID, Quantity = 1, UnitPrice = solidStateDrive.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = completedRequest.RequestID, ItemID = thermalPaste.ItemID, Quantity = 1, UnitPrice = thermalPaste.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = activeRequest.RequestID, ItemID = networkCable.ItemID, Quantity = 2, UnitPrice = networkCable.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = activeRequest.RequestID, ItemID = thermalPaste.ItemID, Quantity = 1, UnitPrice = thermalPaste.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = activeRequest.RequestID, ItemID = memoryKit.ItemID, Quantity = 1, UnitPrice = memoryKit.UnitPrice, IsDeducted = true },
            new JobInventoryUsage { RequestID = quotationRequest.RequestID, ItemID = solidStateDrive.ItemID, Quantity = 1, UnitPrice = solidStateDrive.UnitPrice, IsDeducted = false });

        context.StockMovements.AddRange(
            InitialStock(networkCable, 40, now.AddDays(-25)),
            InitialStock(solidStateDrive, 12, now.AddDays(-25)),
            InitialStock(thermalPaste, 20, now.AddDays(-25)),
            InitialStock(memoryKit, 10, now.AddDays(-25)),
            JobUsage(networkCable, completedRequest, -3, now.AddDays(-17), "Marco Santos"),
            JobUsage(solidStateDrive, completedRequest, -1, now.AddDays(-17), "Marco Santos"),
            JobUsage(thermalPaste, completedRequest, -1, now.AddDays(-17), "Marco Santos"),
            JobUsage(networkCable, activeRequest, -2, now.AddHours(-1), "Marco Santos"),
            JobUsage(thermalPaste, activeRequest, -1, now.AddHours(-1), "Marco Santos"),
            JobUsage(memoryKit, activeRequest, -1, now.AddHours(-1), "Marco Santos"));

        context.ServiceMessages.AddRange(
            new ServiceMessage
            {
                RequestID = completedRequest.RequestID,
                SenderID = technicianUser.Id,
                Content = "The SSD was replaced and the workstation passed the full hardware and stability checks.",
                Timestamp = now.AddDays(-17),
                IsRead = true
            },
            new ServiceMessage
            {
                RequestID = activeRequest.RequestID,
                SenderID = customerUser.Id,
                Content = "The connection has been stable since your last adjustment. I will monitor it during the afternoon.",
                Timestamp = now.AddMinutes(-40),
                IsRead = false
            });

        context.JobDeliverables.Add(new JobDeliverable
        {
            RequestID = completedRequest.RequestID,
            Phase = "FinalProof",
            Description = "Completed workstation repair: storage upgraded, thermal service completed, and QA checklist passed.",
            CreatedAt = now.AddDays(-17)
        });
        context.SupportTickets.Add(new SupportTicket
        {
            CustomerID = customer.CustomerID,
            Subject = "Question about service warranty",
            Message = "Please confirm how long the replacement SSD is covered after the completed repair.",
            Status = "Open",
            CreatedAt = now.AddDays(-2)
        });

        context.UserNotifications.AddRange(
            new UserNotification
            {
                RecipientUserID = customerUser.Id,
                AudienceRole = Roles.Customer,
                Category = "Jobs",
                Icon = "wrench",
                Title = "Technician is working on your network request",
                Message = "Marco Santos is currently handling your office network issue.",
                ActionUrl = $"/Customer/ServiceRequests?jobId={activeRequest.RequestID}",
                SourceKey = $"operations:customer:active-job:{activeRequest.RequestID}",
                CreatedAt = now.AddHours(-1)
            },
            new UserNotification
            {
                RecipientUserID = administrator.Id,
                AudienceRole = Roles.Administrator,
                Category = "Quotations",
                Icon = "file-text",
                Title = "Quotation awaiting review",
                Message = "A hardware-upgrade quotation is ready for administrator review.",
                ActionUrl = $"/Admin/Finance/Quotations#{quotationRequest.RequestID}",
                SourceKey = $"operations:admin:quotation:{quotationRequest.RequestID}",
                CreatedAt = now
            });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine("Company data seeded successfully.");
        Console.WriteLine($"Administrator: {administrator.Email}");
        Console.WriteLine($"Technician: {technicianUser.Email}");
        Console.WriteLine($"Customer: {customerUser.Email}");
        Console.WriteLine("Passwords were supplied through BusinessData__Password and were not printed.");
    }

    internal static async Task<ApplicationUser> EnsureAccountAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> users,
        AccountProfileService profiles,
        string password,
        string email,
        string fullName,
        string phoneNumber,
        string address,
        string role,
        string specialty = "General service")
    {
        var user = await users.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                StreetAddress = address,
                BarangayCity = "Davao City"
            };
            var result = await profiles.CreateAsync(user, role, password, specialty);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create company {role.ToLowerInvariant()} account: "
                    + string.Join("; ", result.Errors.Select(error => error.Description)));
            return user;
        }

        if (user.IsArchived || user.LockoutEnd > DateTimeOffset.UtcNow)
            throw new InvalidOperationException($"Company account {email} is archived or locked out.");

        var roles = await users.GetRolesAsync(user);
        if (roles.Count != 1 || roles[0] != role)
            throw new InvalidOperationException($"Company account {email} must have only the {role} role.");

        await using var transaction = await context.Database.BeginTransactionAsync();
        var passwordReset = await users.GeneratePasswordResetTokenAsync(user);
        var passwordResult = await users.ResetPasswordAsync(user, passwordReset, password);
        if (!passwordResult.Succeeded)
            throw new InvalidOperationException(
                $"Could not update company account {email}: "
                + string.Join("; ", passwordResult.Errors.Select(error => error.Description)));
        await profiles.EnsureDomainProfileAsync(user, role, specialty);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return user;
    }

    internal static StockMovement InitialStock(InventoryItem item, int quantity, DateTime timestamp) => new()
    {
        ItemID = item.ItemID,
        MovementType = "Initial Stock",
        Quantity = quantity,
        UnitCost = item.UnitCost,
        UnitPrice = item.UnitPrice,
        Timestamp = timestamp,
        PerformedBy = "HomeServe Administrator",
        DestinationOrSource = "Warehouse receiving",
        Notes = "Opening stock received into the Davao service warehouse."
    };

    internal static StockMovement JobUsage(InventoryItem item, ServiceRequest request, int quantity,
        DateTime timestamp, string performedBy) => new()
    {
        ItemID = item.ItemID,
        RequestID = request.RequestID,
        MovementType = "Job Usage",
        Quantity = quantity,
        UnitCost = item.UnitCost,
        UnitPrice = item.UnitPrice,
        Timestamp = timestamp,
        PerformedBy = performedBy,
        DestinationOrSource = $"Job #JOB-{request.RequestID:D4}",
        Notes = "Material consumed during a completed service operation."
    };
}
