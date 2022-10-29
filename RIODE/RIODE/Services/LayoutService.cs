using System;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RIODE.DAL;
using RIODE.Models;
using RIODE.ViewModels;
using static System.Net.WebRequestMethods;

namespace RIODE.Services
{
    public class LayoutService
    {
        private readonly RiodeContext _context;
        private readonly IHttpContextAccessor _http;

        public LayoutService(RiodeContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
        }

        public Dictionary<string, string> GetSettings()
        {
            return _context.Settings.ToDictionary(p => p.Key, p => p.Value);
        }


        public LayoutBasketVM GetBasket()
        {
            //BasketVM basket = new BasketVM();

            string basketStr = _http.HttpContext.Request.Cookies["Basket"];
            if (!string.IsNullOrEmpty(basketStr))
            {
                BasketVM basket = JsonConvert.DeserializeObject<BasketVM>(basketStr);
                LayoutBasketVM layoutBasket = new LayoutBasketVM();
                layoutBasket.BasketItemVMs = new List<BasketItemVM>();
                foreach (BasketCookieItemVM cookie in basket.BasketCookieItemVMs)
                {
                    Product existed = _context.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == cookie.Id);
                    if (existed == null)
                    {
                        basket.BasketCookieItemVMs.Remove(cookie);
                        continue;
                    }
                    BasketItemVM basketItem = new BasketItemVM
                    {
                        Product = existed,
                        Quantity = cookie.Quantity
                    };
                    layoutBasket.BasketItemVMs.Add(basketItem);
                }
                layoutBasket.TotalPrice = basket.TotalPrice;
                return layoutBasket;
            }

            return null;

        }



    }
}

