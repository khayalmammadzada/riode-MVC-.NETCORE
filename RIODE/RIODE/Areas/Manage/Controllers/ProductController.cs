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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Areas.Manage.Controllers
{

    [Area("Manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ProductController : Controller
    {
        private readonly RiodeContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(RiodeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        // GET: /<controller>/
        public IActionResult Index()
        {
            List<Product> model = _context.Products.
                Include(pc => pc.Category).
                Include(p => p.Badge).Include(p =>p.Brand).Include(p=>p.Subcategory).
                Include(p => p.ProductImages).ToList();
            return View(model);
        }


        //Create section
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subcategories = _context.Subcategories.ToList();
            ViewBag.Badges = _context.Badges.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            { 
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Subcategories = _context.Subcategories.ToList();
                ViewBag.Badges = _context.Badges.ToList();
                ViewBag.Brands = _context.Brands.ToList();
                ViewBag.Colors = _context.Colors.ToList();
                return View();
            }
            if (product.MainImage == null || product.HoverImage == null || product.Images == null)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Subcategories = _context.Subcategories.ToList();
                ViewBag.Badges = _context.Badges.ToList();
                ViewBag.Brands = _context.Brands.ToList();
                ViewBag.Colors = _context.Colors.ToList();
                ModelState.AddModelError(string.Empty, "You must choose 1 main photo and 1 hover photo and 1 another photo");
                return View();
            }

            if (!product.MainImage.ImageIsOkay(2) || !product.HoverImage.ImageIsOkay(2))
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Subcategories = _context.Subcategories.ToList();
                ViewBag.Badges = _context.Badges.ToList();
                ViewBag.Brands = _context.Brands.ToList();
                ViewBag.Colors = _context.Colors.ToList();
                ModelState.AddModelError(string.Empty, "Please choose valid image file");
                return View();
            }

            product.ProductImages = new List<ProductImage>();
            TempData["FileName"] = "";
            List<IFormFile> removeable = new List<IFormFile>();
            foreach (var photo in product.Images.ToList())
            {
                if (!photo.ImageIsOkay(2))
                {
                    removeable.Add(photo);
                    TempData["FileName"] += photo.FileName + ",";
                    continue;
                }
                ProductImage another = new ProductImage
                {
                    ImageUrl = await photo.FileCreate(_env.WebRootPath, "assets/images/products"),
                    Status = false,
                    Product = product
                };
                product.ProductImages.Add(another);
            }

            product.Images.RemoveAll(p => removeable.Any(r => r.FileName == p.FileName));

            ProductImage main = new ProductImage
            {
                ImageUrl = await product.MainImage.FileCreate(_env.WebRootPath, "assets/images/products"),
                Status = true,
                Product = product
            };
            ProductImage hover = new ProductImage
            {
                ImageUrl = await product.HoverImage.FileCreate(_env.WebRootPath, "assets/images/products"),
                Status = null,
                Product = product
            };

            product.ProductImages.Add(main);
            product.ProductImages.Add(hover);
            if (product.ColorIds != null)
            {
                product.ProductColors = new List<ProductColor>();
                foreach (var item in product.ColorIds)
                {
                    product.ProductColors.Add(new() { Product = product, ColorId = item });
                }
                //foreach (var item in _context.Tags.Where(p => prod.TagIds.Contains(p.Id)))
                //{
                //    prod.ProductTags.Add(new() { Product = prod, Tag = item });
                //}
            }


            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        //Edit section
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subcategories = _context.Subcategories.ToList();
            ViewBag.Badges = _context.Badges.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            if (id is null || id == 0) return NotFound();
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Badge)
                .Include(p => p.Category).
                Include(p => p.Brand).Include(p=>p.Subcategory).SingleOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ActionName("Update")]
        public async Task<IActionResult> Edit(int? id, Product product)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Subcategories = _context.Subcategories.ToList();
            ViewBag.Badges = _context.Badges.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            if (id is null || id == 0) return NotFound();
            Product existed = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Badge)
                .Include(p => p.Category).
                Include(p => p.Brand).Include(p => p.Subcategory)
                .Include(p=>p.ProductColors).ThenInclude(pc=>pc.Color)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (existed == null) return NotFound();

            List<ProductImage> removeable = existed.ProductImages.Where(p => p.Status == false && !product.ImagesId.Contains(p.Id)).ToList();
            //existed.ProductImages.RemoveAll(p => removeable.Any(r => p.Id == r.Id));
            _context.Entry(existed).CurrentValues.SetValues(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}

