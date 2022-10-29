using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RIODE.DAL;
using RIODE.Models;

namespace RIODE.ViewComponents
{
    public class FooterViewComponent:ViewComponent
    {
        private readonly RiodeContext _context;

        public FooterViewComponent(RiodeContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Setting> model = await _context.Settings.ToListAsync();
            return View(model);
        }


    }
}

