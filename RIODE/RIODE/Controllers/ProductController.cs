using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RIODE.DAL;
using RIODE.Models;
using RIODE.ViewModels;
using Z.EntityFramework.Plus;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RIODE.Controllers
{
    public class ProductController : Controller
    {
        private readonly RiodeContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ProductController(RiodeContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: /<controller>/
        public IActionResult Index(int? page = 1)
        {
            if (page < 0) page = 1;
            var products = _context.Products.Include(p => p.ProductImages).Include(p => p.Category)
                .Include(p=>p.Brand).Include(p=>p.Badge).Include(p=>p.Subcategory);
            ViewBag.ActivePage = page;
            ViewBag.PageCount = Math.Ceiling((double)products.Count() / 2);
            ShopIndexVM shopindex = new ShopIndexVM
            {
                Categories = _context.Categories.Include(c => c.Products).ToList(),
                Products = products.Skip(((int)page - 1) * 8).Take(8).ToList(),
                Subcategories = _context.Subcategories.Include(p => p.Products).ToList(),
                Brands = _context.Brands.Include(p => p.Products).ToList(),
                ProductColors = _context.ProductColors.Include(pc=>pc.Color).ToList(),
                Colors=_context.Colors.Include(c=>c.ProductColors).ThenInclude(pc=>pc.Product).ToList(),

            };
            return View(shopindex);
        }

        [HttpPost]
        public IActionResult Index(ShopAndFilterVM items) //List<int> p,List<int> VendorIds, int MaxPrice...
        {
            if (items is null) return NotFound();
            List<Product> products = _context.Products.Include(p => p.ProductImages).IncludeOptimized(p => p.Brand)
                .Include(p => p.Badge).IncludeOptimized(p => p.Category).ToList();

            if (items.CategoryIds != null && items.BrandIds != null)
                products = products.Where(p=> items.CategoryIds.Contains(p.CategoryId) || items.BrandIds.Contains(p.BrandId)).ToList();
            else
            {
                if (items.CategoryIds != null)
                    products = products.Where(p=> items.CategoryIds.Contains(p.CategoryId)).ToList();
                if (items.BrandIds != null)
                    products = products.Where(p => items.BrandIds.Contains(p.BrandId)).ToList();
            }
            if (items.MinPrice >= 0 && items.MinPrice < items.MaxPrice)
                products = products.Where(p => p.SellPrice >= items.MinPrice && p.SellPrice <= items.MaxPrice).ToList();
            else if (items.MinPrice >= 0) products = products.Where(p => p.SellPrice >= items.MinPrice).ToList();
            else if (items.MaxPrice >= 0) products = products.Where(p => p.SellPrice <= items.MaxPrice).ToList();
            return PartialView("_ProductGridPartialView", products);
        }

        public IActionResult Detail(int? id)
        {
            if (id is null) return BadRequest();
            var product = _context.Products
                .Include(p => p.Subcategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Badge)
                .Include(p => p.Brand)
                .Include(p => p.ProductComments)
                .Include(p=>p.ProductColors).ThenInclude(pc=>pc.Color)
                .Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product is null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public IActionResult PostComment(ProductComment comment)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("ReturnUrl", "/Product/Detail/" + comment.ProductId);
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account", routeValueDictionary);
            Product p = _context.Products.FirstOrDefault(p => p.Id == comment.ProductId);
            if (p is null) return NotFound();
            if (!(comment.Rating >= 0 && comment.Rating <= 100) || comment is null) return RedirectToAction(nameof(Detail), comment.ProductId);
            AppUser user = _context.AppUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user is null) return NotFound();
            p.ReviewSum += comment.Rating;
            p.ReviewCount++;
            comment.AppUser = user;
            comment.Product = p;
            comment.CreatedTime = DateTime.UtcNow;
            _context.ProductComments.Add(comment);
            _context.SaveChanges();
            return RedirectToAction(nameof(Detail), new { id = p.Id });
        }




        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null || id == 0) return NotFound();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
            {
                AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null) return NotFound();
                BasketItem existed = await _context.BasketItems.FirstOrDefaultAsync(b => b.AppUserId == user.Id && b.ProductId == product.Id);
                if (existed == null)
                {
                    existed = new BasketItem
                    {
                        Product = product,
                        AppUser = user,
                        Quantity = 1,
                        Price = (decimal)product.SellPrice
                    };
                    _context.BasketItems.Add(existed);
                }
                else
                {
                    existed.Quantity++;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                string basketStr = HttpContext.Request.Cookies["Basket"];

                BasketVM basket;

                if (string.IsNullOrEmpty(basketStr))
                {
                    basket = new BasketVM();
                    BasketCookieItemVM cookieItem = new BasketCookieItemVM
                    {
                        Id = product.Id,
                        Quantity = 1
                    };
                    basket.BasketCookieItemVMs = new List<BasketCookieItemVM>();
                    basket.BasketCookieItemVMs.Add(cookieItem);
                    basket.TotalPrice = (decimal)product.SellPrice;

                }
                else
                {
                    basket = JsonConvert.DeserializeObject<BasketVM>(basketStr);
                    BasketCookieItemVM existed = basket.BasketCookieItemVMs.Find(p => p.Id == id);
                    if (existed == null)
                    {
                        BasketCookieItemVM cookieItem = new BasketCookieItemVM
                        {
                            Id = product.Id,
                            Quantity = 1
                        };
                        basket.BasketCookieItemVMs.Add(cookieItem);
                        basket.TotalPrice += (decimal)product.SellPrice;
                    }
                    else
                    {
                        basket.TotalPrice += (decimal)product.SellPrice;
                        existed.Quantity++;
                    }
                }
                basketStr = JsonConvert.SerializeObject(basket);

                HttpContext.Response.Cookies.Append("Basket", basketStr);
            }

            return RedirectToAction("Index", "Product");
        }


        //public async Task<IActionResult> RemoveBasket(int? id)
        //{
        //    if (id is null || id == 0) return NotFound();
        //    Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        //    if (product == null) return NotFound();
        //    if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
        //    {
        //        AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
        //        if (user == null) return NotFound();
        //        BasketItem existed = await _context.BasketItems.FirstOrDefaultAsync(b => b.AppUserId == user.Id && b.ProductId == product.Id);
        //        if (existed != null)
        //        {

        //            _context.BasketItems.Remove(existed);
        //        }

        //        await _context.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        string basketStr = HttpContext.Request.Cookies["Basket"];

        //        BasketVM basket;

        //        if (!string.IsNullOrEmpty(basketStr))
        //        {

        //            basket = JsonConvert.DeserializeObject<BasketVM>(basketStr);
        //            BasketCookieItemVM existed = basket.BasketCookieItemVMs.Find(p => p.Id == id);
        //            if (existed != null)
        //            {

        //                basket.BasketCookieItemVMs.Remove(existed);

        //            }

        //        }

        //    }

        //    return RedirectToAction("Index", "Product");
        //}



        public IActionResult ShowBasket()
        {
            if (HttpContext.Request.Cookies["Basket"] == null) return NotFound();
            BasketVM basket = JsonConvert.DeserializeObject<BasketVM>(HttpContext.Request.Cookies["Basket"]);
            return Json(basket);
        }




    }
}

