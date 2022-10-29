using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RIODE.DAL;
using RIODE.Models;
using RIODE.Utilities;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles ="SuperAdmin,Admin")]
    public class CategoryController : Controller
    {
        private readonly RiodeContext _context;
        private readonly IWebHostEnvironment _env;


        public CategoryController(RiodeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Category> model = _context.Categories.ToList();
            return View(model);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View();
            if (category.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "Choose at least one image");
                return View();
            }

            if (!category.ImageFile.ImageIsOkay(2))
            {
                ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                return View();
            }


            category.ImageName = await category.ImageFile.FileCreate(_env.WebRootPath, "assets/images/categories");
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null) return NotFound();
            return View(category);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            Category existed = await _context.Categories.FindAsync(id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid) return View(existed);


            if (category.ImageFile == null)
            {
                string filename = existed.ImageName;
                _context.Entry(existed).CurrentValues.SetValues(category);
                existed.ImageName = filename;
            }
            else
            {
                if (!category.ImageFile.ImageIsOkay(2))
                {
                    ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                    return View(existed);
                }

                FileValidator.FileDelete(_env.WebRootPath, "assets/images/categories", existed.ImageName);
                _context.Entry(existed).CurrentValues.SetValues(category);
                existed.ImageName = await category.ImageFile.FileCreate(_env.WebRootPath, "assets/images/categories");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Category category = await _context.Categories.FindAsync(id);
            if (category is null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}

