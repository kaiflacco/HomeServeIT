using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Services;

namespace HomeServeIT.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Administrator)]
    public class CrmController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportingModule _reporting;

        public CrmController(ApplicationDbContext context, ReportingModule reporting)
        {
            _context = context;
            _reporting = reporting;
        }

        public async Task<IActionResult> Customers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .ToListAsync();
            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer([FromServices] UserManager<ApplicationUser> userManager, string firstName, string lastName, string email, string phone, string address, [FromServices] HomeServeIT.Web.Services.AccountProfileService profiles)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                TempData["ErrorMessage"] = "Name and email are required.";
                return RedirectToAction(nameof(Customers));
            }

            var user = new ApplicationUser { UserName = email.Trim(), Email = email.Trim(), FullName = $"{firstName} {lastName}", PhoneNumber = phone, StreetAddress = address };
            var result = await profiles.CreateAsync(user, Roles.Customer);
            if (result.Succeeded)
            {
                return await HomeServeIT.Web.Services.InvitationResult.ShowAsync(this, userManager, user);
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Customers));
        }

        public async Task<IActionResult> CustomerDetail(int id)
        {
            if (id == 0)
            {
                return RedirectToAction(nameof(Customers));
            }

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.ServiceRequests = await _context.ServiceRequests
                .Include(r => r.Technician)
                .Where(r => r.CustomerID == id)
                .OrderByDescending(r => r.ScheduledDate)
                .ToListAsync();

            ViewBag.Invoices = await _context.Invoices
                .Include(i => i.ServiceRequest)
                .Where(i => i.ServiceRequest.CustomerID == id)
                .OrderByDescending(i => i.DateIssued)
                .ToListAsync();

            ViewBag.Devices = await _context.Devices
                .Where(d => d.CustomerID == id)
                .OrderByDescending(d => d.DateAdded)
                .ToListAsync();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer([FromServices] HomeServeIT.Web.Services.AccountProfileService profiles, int customerId, string firstName, string lastName, string phone, string address)
        {
            var customer = await _context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.CustomerID == customerId);
            if (customer?.User == null) return NotFound();
            var model = ProfileViewModel.FromUser(customer.User);
            model.FullName = $"{firstName} {lastName}";
            model.Mobile = phone;
            model.Address = address;
            model.City = null; // The CRM address input contains the complete address.
            var result = await profiles.UpdateAsync(customer.User, model);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded
                ? "Customer updated successfully." : string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(CustomerDetail), new { id = customerId });
        }

        public async Task<IActionResult> ServiceHistory()
        {
            var history = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Technician)
                .Where(r => r.Status == "Completed")
                .OrderByDescending(r => r.CompletedDate ?? r.ScheduledDate)
                .ToListAsync();

            var requestIds = history.Select(h => h.RequestID).ToList();
            var invoices = await _context.Invoices
                .Where(i => requestIds.Contains(i.RequestID))
                .ToDictionaryAsync(i => i.RequestID, i => i.TotalAmount);

            ViewBag.InvoiceAmounts = invoices;
            return View(history);
        }

        public async Task<IActionResult> Reports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(await _reporting.GetOverviewAsync(userId, DateTime.UtcNow));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadReport(string type = "Full Summary", string? dateRange = "30", DateTime? startDate = null, DateTime? endDate = null)
        {
            var result = await _reporting.GenerateAsync(
                new ReportGenerationRequest(type, dateRange, startDate, endDate),
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                DateTime.UtcNow);
            if (result.RequiresAuthorization) return Unauthorized();
            if (!result.Succeeded) return BadRequest(result.ErrorMessage);

            Response.Headers.CacheControl = "no-store";
            return File(result.PdfContent!, "application/pdf", result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadRecentReport(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var report = await _reporting.GetRecentReportAsync(id, userId);
            if (report == null) return NotFound();

            Response.Headers.CacheControl = "no-store";
            return File(report.PdfContent, "application/pdf", report.FileName);
        }
    }
}
