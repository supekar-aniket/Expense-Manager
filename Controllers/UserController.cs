using Microsoft.AspNetCore.Authorization;
using ExpenseManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUsersInRoleAsync("User");

                return View(user);
            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching users Data!!!");
                return View(new List<IdentityUser>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) // use when id is a string
            {
                return View("NotFound");
            }

            try
            {
                var userData = await _userManager.FindByIdAsync(id);

                if(userData == null)
                {
                    return View("NotFound");
                }

                return View(userData);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching user data!!!");
                return View();
            }

        }


        public async Task<IActionResult> Delete(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) // use when id is a string
            {
                return View("NotFound");
            }

            try
            {
                var userData = await _userManager.FindByIdAsync(id);

                if(userData == null)
                {
                    return View("NotFound");
                }

                return View(userData);

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching user data!!!");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) // use when id is a string
            {
                return View("NotFound");
            }

            try
            {
                var userData = await _userManager.FindByIdAsync(id);

                if(userData == null)
                {
                    return View("NotFound");
                }

                await _userManager.DeleteAsync(userData);

                return RedirectToAction(nameof(Index));

            }catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while delete user data!!!");
                return View();
            }
        }

    }
}
