using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeServeIT.Web.Constants;
using HomeServeIT.Web.Data;
using HomeServeIT.Web.Models;
using HomeServeIT.Web.Areas.Technician.Models;
using HomeServeIT.Web.Services;

namespace HomeServeIT.Web.Areas.Technician.Controllers
{
    [Area("Technician")]
    [Authorize(Roles = Roles.Technician)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly WeatherService _weatherService;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            WeatherService weatherService)
        {
            _context = context;
            _userManager = userManager;
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var tech = await _context.Technicians.FirstOrDefaultAsync(t => t.UserID == user!.Id);

            if (tech == null)
                return View(new TechnicianDashboardViewModel());

            var allJobs = await _context.ServiceRequests
                .Include(r => r.Customer)
                    .ThenInclude(customer => customer.User)
                .VisibleTechnicianAssignments(tech.TechID)
                .ToListAsync();

            var today = DateTime.Today;
            var actionableJobs = allJobs
                .Where(job => !job.Customer.User.IsArchived)
                .ToList();
            var vm = new TechnicianDashboardViewModel
            {
                TotalAssigned = allJobs.Count,
                TotalPending  = actionableJobs.Count(j => j.Status == "Pending"),
                TotalCompleted = actionableJobs.Count(j => j.Status == "Completed"),
                TodaysJobs    = actionableJobs.Where(j => j.ScheduledDate.Date == today || j.Status == "In Progress").ToList(),
                UpcomingJobs  = actionableJobs
                                    .Where(j => j.ScheduledDate.Date > today && j.Status != "Completed" && j.Status != "In Progress")
                                    .OrderBy(j => j.ScheduledDate)
                                    .Take(5)
                                    .ToList()
            };

            var nextJob = vm.TodaysJobs.FirstOrDefault(j => j.Status == "In Progress")
                          ?? vm.TodaysJobs.FirstOrDefault(j => j.Status == "Pending")
                          ?? vm.UpcomingJobs.FirstOrDefault();
            // Use the appointment city when a job exists; otherwise show live weather
            // for the technician's city so the dashboard remains useful on quiet days.
            // Only city-level profile data is sent to the provider.
            var weatherCity = nextJob?.Customer?.User?.BarangayCity ?? user?.BarangayCity;
            vm.NextJobWeather = await _weatherService.GetWeatherAsync(
                weatherCity,
                nextJob?.ScheduledDate ?? DateTime.Today,
                HttpContext.RequestAborted);

            return View(vm);
        }
    }
}
