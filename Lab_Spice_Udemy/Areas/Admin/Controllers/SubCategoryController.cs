using Lab_Spice_Udemy.Data;
using Lab_Spice_Udemy.Models;
using Lab_Spice_Udemy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab_Spice_Udemy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        //get-index
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subCategories);
        }

        //get-create
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryModel model = new SubCategoryAndCategoryModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //post-create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if(doesSubCategoryExists.Count() > 0)
                {
                    //error
                    StatusMessage = "Error: Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another name";

                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryModel modelVM = new SubCategoryAndCategoryModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await (from SubCategory in _db.SubCategory
                             where SubCategory.CategoryId == id
                             select SubCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //get-edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if(subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryModel model = new SubCategoryAndCategoryModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //post-edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    //error
                    StatusMessage = "Error: Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another name";

                }
                else
                {
                    var subCatFromDb = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    subCatFromDb.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryModel modelVM = new SubCategoryAndCategoryModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };
            //modelVM.SubCategory.Id = id;
            return View(modelVM);
        }



    }
}
