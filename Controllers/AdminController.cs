using ExpenseManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {

        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDBContext dBContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dBContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var users = await _userManager.GetUsersInRoleAsync("User");
                ViewBag.TotalUsers = users.Count;

                ViewBag.TotalExpenses = _dbContext.Expenses.Count();

                ViewBag.TotalCategories = _dbContext.Categories.Count();

                ViewBag.RecentActivities = _dbContext.Expenses
                                                 .OrderByDescending(e => e.DateAndTime)
                                                 .Take(5)
                                                 .Select(e => $"{e.ItemName} added Rs.-{e.Amount} in {e.Category.Name} !")
                                                 .ToList();
            }catch (Exception ex)
            {
                ModelState.AddModelError("", "An occurred while fetching Users data !!!");
                return View();
            }

            return View();
        }
    }
}
