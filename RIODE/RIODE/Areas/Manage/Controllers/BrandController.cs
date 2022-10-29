using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIODE.DAL;
using RIODE.Models;
using RIODE.Utilities;
using RIODE.Utilities.Enums;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class BrandController : Controller
    {


        private readonly RiodeContext _context;
        private readonly IWebHostEnvironment _env;


        public BrandController(RiodeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Brand> model = _context.Brands.ToList();
            return View(model);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid) return View();
            Brand existed = _context.Brands.FirstOrDefault(c => c.Name.ToLower().Trim() == brand.Name.ToLower().Trim());
            if (existed != null)
            {
                ModelState.AddModelError("Name", "You can not duplicate brand name");
                return View();
            }


            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Brand brand = await _context.Brands.FirstOrDefaultAsync(c => c.Id == id);
            if (brand is null) return NotFound();
            return View(brand);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Update(int? id, Brand brand)
        {
            Brand existed = await _context.Brands.FindAsync(id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid) return View(existed);

                _context.Entry(existed).CurrentValues.SetValues(brand);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Brand brand = await _context.Brands.FindAsync(id);
            if (brand is null) return NotFound();

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}

