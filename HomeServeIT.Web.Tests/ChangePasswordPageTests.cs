using System.Security.Claims;
using HomeServeIT.Web.Areas.Identity.Pages.Account.Manage;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace HomeServeIT.Web.Tests;

public sealed class ChangePasswordPageTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LocalPageActivatesAndLoadsForPasswordAccount(bool hasPassword)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication();
        services.AddScoped(_ => database.CreateContext());
        services.AddIdentityCore<ApplicationUser>().AddEntityFrameworkStores<ApplicationDbContext>().AddSignInManager();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "password-page@test.invalid" };
        var result = hasPassword ? await users.CreateAsync(user, "FixtureOnly!123") : await users.CreateAsync(user);
        Assert.True(result.Succeeded);

        // Exercise local PageModel activation; the former internal Identity model could not be activated.
        var page = ActivatorUtilities.CreateInstance<ChangePasswordModel>(scope.ServiceProvider);
        page.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "Test"))
            }
        };
        var response = await page.OnGetAsync();
        if (hasPassword) Assert.IsType<PageResult>(response);
        else Assert.Equal("./SetPassword", Assert.IsType<RedirectToPageResult>(response).PageName);

        page.Input = new() { OldPassword = "Incorrect!123", NewPassword = "Replacement!456", ConfirmPassword = "Replacement!456" };
        if (hasPassword)
        {
            Assert.IsType<PageResult>(await page.OnPostAsync());
            Assert.False(page.ModelState.IsValid);
            Assert.True(await users.CheckPasswordAsync(user, "FixtureOnly!123"));
        }
        page.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        Assert.IsType<ChallengeResult>(await page.OnGetAsync());
        Assert.IsType<ChallengeResult>(await page.OnPostAsync());
    }
}
