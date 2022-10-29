using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RIODE.DAL;
using RIODE.Utilities;
using RIODE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SliderController : Controller
    {
        private readonly RiodeContext _context;
        private readonly IWebHostEnvironment _env;


        public SliderController(RiodeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Slider> model = _context.Sliders.ToList();
            return View(model);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Slider slider)
        {
            if (!ModelState.IsValid) return View();
            if(slider.ImageFile is null)
            {
                ModelState.AddModelError("ImageFile", "Choose at least one image");
                return View(); 
            }

            if (!slider.ImageFile.ImageIsOkay(2))
            {
                ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                return View();
            }
            

            slider.ImageUrl = await slider.ImageFile.FileCreate(_env.WebRootPath, "assets/images/demos/demo1/slides");

            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id == 0) return NotFound();
            Slider slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider is null) return NotFound();
            return View(slider);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Update(int? id, Slider slider)
        {
            Slider existed = await _context.Sliders.FindAsync(id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid) return View(existed);
           

            if (slider.ImageFile==null)
            {
                string filename = existed.ImageUrl;
                _context.Entry(existed).CurrentValues.SetValues(slider);
                existed.ImageUrl = filename;
            }
            else
            {
                if (!slider.ImageFile.ImageIsOkay(2))
                {
                    ModelState.AddModelError("ImageFile", "Size less than 2mb and image file");
                    return View(existed);
                }

                FileValidator.FileDelete(_env.WebRootPath, "assets/images/demos/demo1/slides", existed.ImageUrl);
                _context.Entry(existed).CurrentValues.SetValues(slider);
                existed.ImageUrl = await slider.ImageFile.FileCreate(_env.WebRootPath, "assets/images/demos/demo1/slides");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}

