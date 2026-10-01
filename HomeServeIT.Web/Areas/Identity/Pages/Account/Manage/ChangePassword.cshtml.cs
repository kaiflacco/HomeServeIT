using System.ComponentModel.DataAnnotations;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeServeIT.Web.Areas.Identity.Pages.Account.Manage;

[Authorize(Roles = Roles.Administrator + "," + Roles.Customer + "," + Roles.Technician)]
public sealed class ChangePasswordModel(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public sealed class InputModel
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The new password must be between {2} and {1} characters.")]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!await users.HasPasswordAsync(user)) return RedirectToPage("./SetPassword");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!await users.HasPasswordAsync(user)) return RedirectToPage("./SetPassword");
        if (!ModelState.IsValid) return Page();

        var result = await users.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await signIn.RefreshSignInAsync(user);
        StatusMessage = "Your password has been changed.";
        return RedirectToPage();
    }
}
