using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminExpenseController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminExpenseController(ApplicationDBContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // ✅ Show all expenses with filters
        public async Task<IActionResult> Index(string userName, int? categoryId, DateTime? startDate, DateTime? endDate)
        {
            ModelState.Remove("userName");

            try
            {
                var expenses = _dbContext.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .AsQueryable();

                // User filter
                if (!string.IsNullOrEmpty(userName))
                    expenses = expenses.Where(e => e.User.UserName.Contains(userName));

                // ✅ Category filter (int?)
                if (categoryId.HasValue)
                    expenses = expenses.Where(e => e.CategoryId == categoryId.Value);

                // Date filters
                if (startDate.HasValue)
                    expenses = expenses.Where(e => e.DateAndTime >= startDate.Value);

                if (endDate.HasValue)
                    expenses = expenses.Where(e => e.DateAndTime <= endDate.Value);

                ViewBag.Users = await _userManager.Users.ToListAsync();
                ViewBag.Categories = await _dbContext.Categories.ToListAsync();

                return View(await expenses.OrderByDescending(e => e.DateAndTime).ToListAsync());
            }
            catch (Exception ex)
            {
                // Log the error if you have logging setup (e.g., _logger.LogError(ex, "Error fetching expenses"));
                ModelState.AddModelError("", "Something went wrong while fetching expenses.");

                // Fallback: load empty lists so View won’t crash
                ViewBag.Users = await _userManager.Users.ToListAsync();
                ViewBag.Categories = await _dbContext.Categories.ToListAsync();

                return View(new List<Expense>()); // return empty expenses list
            }
        }


        // ✅ Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var expense = await _dbContext.Expenses
                                    .Include(e => e.Category)
                                    .Include(e => e.User)
                                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                {
                    return NotFound();
                }

                return View(expense);

            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching user data!!!");
                return View();
            }
            
        }

        // Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return View("NotFound");

            try
            {
                var expenseData = await _dbContext.Expenses
                    .Include(e => e.Category)
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expenseData == null)
                    return View("NotFound");

                ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expenseData.CategoryId);

                return View(expenseData);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error while fetching expense data!");
                return View("Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Expense expense)
        {
            if (id != expense.Id)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var expenseData = await _dbContext.Expenses.FindAsync(id);

                    if(expenseData == null)
                    {
                        return View("NotFound");
                    }

                    // Update only allowed fields
                    expenseData.ItemName = expense.ItemName;
                    expenseData.Amount = expense.Amount;
                    expenseData.DateAndTime = expense.DateAndTime;
                    expenseData.Description = expense.Description;
                    expenseData.CategoryId = expense.CategoryId;

                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error while Edit user data!!!");
                    return View(expense);
                }
            }

            // If valiation then reload the dropdown list
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }

        //  Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var expense = await _dbContext.Expenses
                                    .Include(e => e.Category)
                                    .Include(e => e.User)
                                    .FirstOrDefaultAsync(m => m.Id == id);

                if (expense == null)
                {
                    return View("NotFound");
                }

                return View(expense);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching user data!!!");
                return View();
            }

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var expense = await _dbContext.Expenses.FindAsync(id);

                if(expense == null)
                {
                    return View("NotFound");
                }

                _dbContext.Expenses.Remove(expense);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch
            {
                ModelState.AddModelError("", "Error while Delete user data!!!");
                return View();
            }

        }
    }
}
