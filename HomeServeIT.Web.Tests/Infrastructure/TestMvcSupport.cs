using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.FileProviders;

namespace HomeServeIT.Web.Tests.Infrastructure;

internal sealed class TestTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment, IDisposable
{
    public TestWebHostEnvironment()
    {
        ContentRootPath = Directory.CreateTempSubdirectory("homeserve-test-").FullName;
    }

    public string ApplicationName { get; set; } = "HomeServeIT.Web";
    public string EnvironmentName { get; set; } = "Testing";
    public string WebRootPath { get; set; } = "";
    public string ContentRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    public void Dispose()
    {
        if (Directory.Exists(ContentRootPath))
            Directory.Delete(ContentRootPath, recursive: true);
    }
}
