using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ExpenseManager.Controllers
{
    [Authorize(Roles ="User")]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpenseController(ApplicationDBContext dBContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dBContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userID = _userManager.GetUserId(User);

                var expenses = await _dbContext.Expenses
                                             .Include(e => e.Category)
                                             .Where(e => e.UserId == userID)
                                             .ToListAsync();

                return View(expenses);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching Expense data!!!");
                return View();
            }
        }

        public IActionResult Create()
        {
            // Get all categories from DB and store in ViewBag
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if(expense == null)
            {
                return View("NotFound");
            }

            if(ModelState.IsValid)
            {
                try
                {
                    //  Get currently logged-in user ID
                    var userID = _userManager.GetUserId(User);

                    if(userID == null) // in case no one is logged in
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    //  Assign values automatically
                    expense.UserId = userID;

                    // Use entered date if provided, otherwise set now
                    if (expense.DateAndTime == default)
                    {
                        expense.DateAndTime = DateTime.Now;
                    }

                    await _dbContext.Expenses.AddAsync(expense);
                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }catch(Exception ex)
                {
                    ModelState.AddModelError("", "Error while add new expense!!!");

                    // Re-populate dropdown (otherwise it will be empty on error page)
                    ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expense.CategoryId);

                    return View(expense);
                }
            }

            // Also re-populate dropdown if ModelState invalid
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var userID = _userManager.GetUserId(User);

                if(userID == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var expenseData = await _dbContext.Expenses
                                                  .Include(e => e.Category)
                                                  .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userID);
                
                ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expenseData.CategoryId);
                
                return View(expenseData);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching Expense data!!!");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Expense expense)
        {
            if(id != expense.Id)
            {
                return View("NotFound");
            }

            if(ModelState.IsValid)
            {
                try
                {
                    var userID = _userManager.GetUserId(User);

                    if(userID == null)
                    {
                        return View("NotFound");
                    }

                    expense.UserId = userID;

                    _dbContext.Expenses.Update(expense);
                    await _dbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));

                }catch(Exception ex)
                {
                    ModelState.AddModelError("", "Error while Update Expense data!!!");
                    return View();
                }
            }

            // If validation fails, reload dropdown again
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name", expense.CategoryId);

            return View(expense);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            try
            {
                // get current login user id
                var userID = _userManager.GetUserId(User);

                // check same user or not
                var expenseData = await _dbContext
                                        .Expenses
                                        .Include(e => e.Category) // include category 
                                        .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userID);

                if (expenseData == null)
                {
                    return View("NotFound");
                }

                return View(expenseData);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error while Fetching Expense data!!!");
                return View();
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var userID = _userManager.GetUserId(User);

                if(userID == null)
                {
                    return View("NotFound");
                }

                var expenseData = await _dbContext .Expenses
                                             .Include(e => e.Category)
                                             .FirstOrDefaultAsync(e => e.Id ==id && e.UserId==userID);

                return View(expenseData);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching Expense data!!!");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var userID = _userManager.GetUserId(User);

                if(userID == null)
                {
                    return View("NotFound");
                }

                var expenseData = await _dbContext
                                        .Expenses
                                        .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userID);

                if(expenseData == null)
                {
                    return View("NotFound");
                }

                _dbContext.Expenses.Remove(expenseData);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while delete Expense data!!!");
                return View();
            }
        }

    }
}
