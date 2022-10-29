using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RIODE.DAL;
using RIODE.Models;
using RIODE.Utilities;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SubcategoryController : Controller
    {
        private readonly RiodeContext _context;
        private readonly IWebHostEnvironment _env;


        public SubcategoryController(RiodeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Subcategory> model = _context.Subcategories.ToList();
            return View(model);
        }

        //CREATE SECTION
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Subcategory subcategory)
        {
            ViewBag.Categories = _context.Categories.ToList();
            if (!ModelState.IsValid) return View();
            Subcategory existed = _context.Subcategories.FirstOrDefault(c => c.Name.ToLower().Trim() == subcategory.Name.ToLower().Trim());
            if (existed != null)
            {
                ModelState.AddModelError("Name", "You can not duplicate subcategory name");
                return View();
            }
            if (subcategory.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "Choose at least one image");
                return View();
            }

            if (!subcategory.ImageFile.ImageIsOkay(2))
            {
                ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                return View();
            }

            subcategory.IsDeleted = false;
            subcategory.ImageName = await subcategory.ImageFile.FileCreate(_env.WebRootPath, "assets/images/categories");
            await _context.Subcategories.AddAsync(subcategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //UPDATE SECTION
        public IActionResult Update(int? id)
        {
            ViewBag.Categories = _context.Categories.ToList();
            if (id is null || id == 0) return NotFound();
            Subcategory subcategory = _context.Subcategories.FirstOrDefault(c => c.Id == id);
            if (subcategory is null) return NotFound();
            return View(subcategory);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Update(int? id, Subcategory subcategory)
        {
            ViewBag.Categories = _context.Categories.ToList();
            Subcategory existed = await _context.Subcategories.FindAsync(id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid) return View(existed);


            if (subcategory.ImageFile == null)
            {
                string filename = existed.ImageName;
                _context.Entry(existed).CurrentValues.SetValues(subcategory);
                existed.ImageName = filename;
            }
            else
            {
                if (!subcategory.ImageFile.ImageIsOkay(2))
                {
                    ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                    return View(existed);
                }

                FileValidator.FileDelete(_env.WebRootPath, "assets/images/categories", existed.ImageName);
                _context.Entry(existed).CurrentValues.SetValues(subcategory);
                existed.ImageName = await subcategory.ImageFile.FileCreate(_env.WebRootPath, "assets/images/categories");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //DELETE SECTION
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Subcategory subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory is null) return NotFound();

            _context.Subcategories.Remove(subcategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}

