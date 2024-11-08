// Services/ProductService.cs
using ProductFilterAPI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProductFilterAPI.Services
{
	public class ProductService
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
			_logger.LogInformation("Fetching products from external API: {ProductUrl}", ProductUrl);

			try
			{
				var response = await _httpClient.GetAsync(ProductUrl);
				if (response.IsSuccessStatusCode)
				{
					_logger.LogInformation("Received successful response from {ProductUrl}", ProductUrl);
					// Read the content as a string
					var jsonString = await response.Content.ReadAsStringAsync();

					// Deserialize the JSON string into a list of Product objects
					var products = JsonSerializer.Deserialize<ProductList>(jsonString, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
					_logger.LogInformation($"Deserialized {products.Products.Count} products");

					// Return the deserialized list
					return products ?? new ProductList();
				}
				else
				{
					_logger.LogWarning("Failed to fetch products. Status code: {StatusCode}", response.StatusCode);
					return new ProductList();
				}
				// Return an empty list if the response is not successful
				
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching products from {ProductUrl}", ProductUrl);
				

				return new ProductList();
			}
		}
	}
}
