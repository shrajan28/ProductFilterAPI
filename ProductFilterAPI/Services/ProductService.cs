using ProductFilterAPI.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace ProductFilterAPI.Services
{
	public class ProductService:IProductService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ProductService> _logger;
		private const string ProductUrl = "https://pastebin.com/raw/JucRNpWs";

		public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<ProductList> GetProductsAsync()
		{
			var response = await _httpClient.GetAsync(ProductUrl);
			response.EnsureSuccessStatusCode();

			// Read the response content as a string for logging
			var jsonResponse = await response.Content.ReadAsStringAsync();
			_logger.LogInformation("Full API response from {Url}: {Response}", ProductUrl, jsonResponse);

			// Deserialize the JSON content to ProductList
			var productList = JsonSerializer.Deserialize<ProductList>(jsonResponse, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			// Ensure Products is initialized as a Collection if null
			if (productList?.Products == null)
			{
				productList.Products = new Collection<Product>();
			}

			return productList;
		}
	}
}
