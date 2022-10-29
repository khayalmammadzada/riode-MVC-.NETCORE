using System;
namespace RIODE.ViewModels
{
	public class ShopAndFilterVM
	{
        public List<int> CategoryIds { get; set; }
        public List<int> BrandIds { get; set; }
        //public List<int> ColorIds { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinPrice { get; set; }
    }
}

