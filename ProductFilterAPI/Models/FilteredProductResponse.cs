using System.Collections.ObjectModel;

namespace ProductFilterAPI.Models
{
	public class FilteredProductResponse
	{
		public Collection<Product> Product { get; set; }
		public FilterInfo FilterOptions { get; set; }
	}
}
