// Models/Product.cs




// Models/FilterInfo.cs
using System.Text.Json.Serialization;

namespace ProductFilterAPI.Models
{


	// Models/Product.cs

	[JsonSerializable(typeof(List<Product>))]
	public class ProductList
	{
		public List<Product> Products { get; set; }
	}
	[JsonSerializable(typeof(Product))]
	public class Product
		{
			public string Title { get; set; }
			public decimal Price { get; set; }
			public List<string> Sizes { get; set; } // Changed to a list to hold multiple sizes
			public string Description { get; set; }
		}

	public class FilterInfo
	{
		public decimal MinPrice { get; set; }
		public decimal MaxPrice { get; set; }
		public List<string> Sizes { get; set; }
		public List<string> CommonWords { get; set; }
	}
}
