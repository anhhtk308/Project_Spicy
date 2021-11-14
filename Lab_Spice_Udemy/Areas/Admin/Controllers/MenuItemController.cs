using Lab_Spice_Udemy.Data;
using Lab_Spice_Udemy.Models.ViewModels;
using Lab_Spice_Udemy.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lab_Spice_Udemy.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;
        [BindProperty]
        public MenuItemViewModel MenuIetmVM { get; set; }
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostEnvironment;
            MenuIetmVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem(),
                SubCategory = _db.SubCategory
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //get-create
        public IActionResult Create()
        {
            return View(MenuIetmVM);
        }
        
        //post-create
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuIetmVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuIetmVM);
            }
            _db.MenuItem.Add(MenuIetmVM.MenuItem);
            await _db.SaveChangesAsync();

            //image
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDB = await _db.MenuItem.FindAsync(MenuIetmVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //file has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuIetmVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDB.Image = @"\images\" + MenuIetmVM.MenuItem.Id + extension;
            }
            else
            {
                //no file was uploaded, use default
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuIetmVM.MenuItem.Id + ".png");
                menuItemFromDB.Image = @"\images\" + MenuIetmVM.MenuItem.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //get-edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuIetmVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuIetmVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuIetmVM.MenuItem.Id).ToListAsync();
            if(MenuIetmVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuIetmVM);
        }

        //post-edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            MenuIetmVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                MenuIetmVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuIetmVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuIetmVM);
            }
           
            //image
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDB = await _db.MenuItem.FindAsync(MenuIetmVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //new file has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //delete original file
                var imagePath = Path.Combine(webRootPath, menuItemFromDB.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                //upload new file
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuIetmVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDB.Image = @"\images\" + MenuIetmVM.MenuItem.Id + extension_new;
            }
            menuItemFromDB.Name = MenuIetmVM.MenuItem.Name;
            menuItemFromDB.Description = MenuIetmVM.MenuItem.Description;
            menuItemFromDB.Price = MenuIetmVM.MenuItem.Price;
            menuItemFromDB.Spicyness = MenuIetmVM.MenuItem.Spicyness;
            menuItemFromDB.CategoryId = MenuIetmVM.MenuItem.CategoryId;
            menuItemFromDB.SubCategoryId = MenuIetmVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
