using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Services;

namespace HomeServeIT.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = Roles.Customer)]
public class QuotationsController : Controller
{
    private static readonly string[] SupportedPaymentMethods = ["Cash", "Card", "GCash"];
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JobInventoryService _jobInventoryService;

    public QuotationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        JobInventoryService jobInventoryService)
    {
        _context = context;
        _userManager = userManager;
        _jobInventoryService = jobInventoryService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == user!.Id);

        if (customer == null)
            return View(new List<Invoice>());

        var invoices = await _context.Invoices
            .Include(i => i.ServiceRequest)
            .Where(i => i.ServiceRequest.CustomerID == customer.CustomerID
                        && i.IsQuotation
                        && (i.QuotationStatus == "ApprovedByAdmin" || i.QuotationStatus == "Approved"))
            .OrderByDescending(i => i.DateIssued)
            .ToListAsync();

        return View(invoices);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == user!.Id);

        var quotation = await _context.Invoices
            .Include(i => i.ServiceRequest)
            .FirstOrDefaultAsync(i => i.InvoiceID == id 
                                      && i.ServiceRequest.CustomerID == customer!.CustomerID 
                                      && i.IsQuotation);

        if (quotation == null) return NotFound();

        return View(quotation);
    }

    public async Task<IActionResult> Checkout(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == user!.Id);

        var quotation = await _context.Invoices
            .Include(i => i.ServiceRequest)
            .FirstOrDefaultAsync(i => i.InvoiceID == id 
                                      && i.ServiceRequest.CustomerID == customer!.CustomerID 
                                      && i.IsQuotation
                                      && (i.QuotationStatus == "ApprovedByAdmin" || i.QuotationStatus == "Approved")
                                      && i.PaymentStatus == "Unpaid");

        if (quotation == null) return NotFound();

        return View(quotation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int id, string paymentMethod)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Forbid();

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == user.Id);

        if (customer == null) return Forbid();
        if (!SupportedPaymentMethods.Contains(paymentMethod, StringComparer.Ordinal))
        {
            TempData["ErrorMessage"] = "Choose one of the available payment methods before continuing.";
            return RedirectToAction(nameof(Index));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var quotation = await _context.Invoices
            .Include(i => i.ServiceRequest)
            .FirstOrDefaultAsync(i => i.InvoiceID == id
                                      && i.ServiceRequest.CustomerID == customer.CustomerID
                                      && i.IsQuotation
                                      && i.ServiceRequest.Status != "Cancelled");

        if (quotation == null) return NotFound();

        if (quotation.PaymentStatus == "Paid")
        {
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = $"Payment for Quotation #{quotation.InvoiceID:D4} was already recorded.";
            return RedirectToAction(nameof(Index));
        }

        if (quotation.PaymentStatus != "Unpaid"
            || quotation.QuotationStatus is not ("ApprovedByAdmin" or "Approved")
            || quotation.ServiceRequest.Status != "Pending")
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "This quotation must be approved by an administrator before payment.";
            return RedirectToAction(nameof(Index));
        }

        var inventoryResult = await _jobInventoryService.DeductForJobStartAsync(
            quotation.RequestID,
            user.FullName ?? user.Email ?? "Customer");
        if (!inventoryResult.Succeeded)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = inventoryResult.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        quotation.PaymentStatus = "Paid";
        quotation.QuotationStatus = "Approved";
        if (quotation.ServiceRequest.Status is not ("Completed" or "Cancelled"))
            quotation.ServiceRequest.Status = "In Progress";

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["SuccessMessage"] = $"Payment request for Quotation #{quotation.InvoiceID:D4} was recorded locally using {paymentMethod}. No external funds were captured.";
        return RedirectToAction(nameof(Index));
    }
}
