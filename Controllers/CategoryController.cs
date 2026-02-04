using ExpenseManager.Areas.Identity.Data;
using ExpenseManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace ExpenseManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDBContext _dBContext;

        public CategoryController(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _dBContext.Categories
                                                 .OrderBy(e => e.Name)
                                                 .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An occurred while fetching Category data !!!");
                return View(new List<Category>());
            }
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if(category == null)
            {
                ModelState.AddModelError("", "Category data is required.");
                return View();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool catExist = await _dBContext.Categories.AnyAsync(x => x.Name == category.Name);

                    if(catExist)
                    {
                        ModelState.AddModelError("", "This category already exist.");
                        return View(category);
                    }

                    await _dBContext.Categories.AddAsync(category);
                    await _dBContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                } catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error while add new Category record !!!");
                    return View(category);
                }
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var catData = await _dBContext.Categories.FindAsync(id);

                if(catData != null)
                {
                    return View(catData);
                }

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex) 
            {
                ModelState.AddModelError("", "An error occurred  while fetching category data...");
                return View();
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Category category)
        {
            if(id != category.Id)
            {
                return View("NotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate category name (excluding current record)
                    bool duplicateExist = await _dBContext.Categories
                        .AnyAsync(x => x.Name == category.Name && x.Id != category.Id);

                    if (duplicateExist)
                    {
                        ModelState.AddModelError("", "Another category with the same name already exists.");
                        return View(category);
                    }

                    _dBContext.Categories.Update(category);
                    await _dBContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "Error while updating Category data!!!");
                    return View(category);
                }
            }

            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            try
            {
                var catData = await _dBContext.Categories.FindAsync(id);

                if(catData != null)
                {
                    return View(catData);
                }

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Error while fetching Category data!!!");
                return View();
            }

        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            try
            {
                var catData = await _dBContext.Categories.FindAsync(id);

                if(catData == null)
                {
                    return View("NotFound");
                }

                _dBContext.Categories.Remove(catData);
                await _dBContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error while Deleting Category Data!!!");
                return View();
            }

        }

    }
}
