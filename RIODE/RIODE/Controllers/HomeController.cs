using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIODE.DAL;
using RIODE.Models;
using RIODE.ViewModels;
//using RIODE.Models;

namespace RIODE.Controllers;

public class HomeController : Controller
{
    private readonly RiodeContext _context;
    public HomeController(RiodeContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        HomeVM home = new HomeVM
        {
            Sliders = _context.Sliders.ToList(),
            Categories = _context.Categories.ToList(),
            Products = _context.Products.Include(p => p.ProductImages).Include(p => p.Brand)
           .Include(p => p.Badge).Include(p=>p.Subcategory).Include(pc => pc.Category).ToList(),
            Services = _context.Services.ToList(),
            Subcategories=_context.Subcategories.Include(s=>s.Products).ToList(),
           ProductImages=_context.ProductImages.ToList(),
        };
        return View(home);
    }


}

