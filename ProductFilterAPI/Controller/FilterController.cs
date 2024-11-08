// Controllers/FilterController.cs
using Microsoft.AspNetCore.Mvc;
using ProductFilterAPI.Models;
using ProductFilterAPI.Services;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ProductFilterAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FilterController : ControllerBase
	{
		private readonly ProductService _productService;
		private readonly ILogger<FilterController> _logger;
		private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"the", "is", "and", "of", "a", "in", "to", "for", "with", "on", "that", "this", "it", "by", "an", "as", "at"
	};
		public FilterController(ProductService productService,ILogger<FilterController> logger)
		{
			_productService = productService;
			_logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> GetFilteredProducts(
			[FromQuery] decimal? minprice,
			[FromQuery] decimal? maxprice,
			[FromQuery] string? size,
			[FromQuery] string? highlight)
		{
			_logger.LogInformation("Received filter request with parameters: minprice={MinPrice}, maxprice={MaxPrice}, size={Size}, highlight={Highlight}",
				minprice, maxprice, size, highlight);

			var productsList = await _productService.GetProductsAsync();
			var products = productsList.Products;

			// Filter products
			if (minprice.HasValue)
				products = products.Where(p => p.Price >= minprice.Value).ToList();
			if (maxprice.HasValue)
				products = products.Where(p => p.Price <= maxprice.Value).ToList();
			if (!string.IsNullOrEmpty(size))
				products = products.Where(p => p.Sizes.Contains(size, StringComparer.OrdinalIgnoreCase)).ToList();

			_logger.LogInformation("Filtered products count: {Count}", products.Count);

			// Generate filter info
			var filterInfo = new FilterInfo();
			if (products.Count > 0)
			{
				var minPrice = products.Min(p => p.Price);
				var maxPrice = products.Max(p => p.Price);
				var sizes = products.SelectMany(p => p.Sizes).Distinct().ToList();
				var commonWords = GetMostCommonWords(products);


				filterInfo.MinPrice = minPrice;
				filterInfo.MaxPrice = maxPrice;
				filterInfo.Sizes = sizes;
				filterInfo.CommonWords = commonWords;
				
			}

			_logger.LogInformation("Generated filter object with minPrice={MinPrice}, maxPrice={MaxPrice}, sizes={Sizes}",
					filterInfo.MinPrice, filterInfo.MaxPrice, string.Join(", ", filterInfo.Sizes));
			// Highlight words
			if (!string.IsNullOrEmpty(highlight))
			{
				var wordsToHighlight = highlight.Split(',');
				foreach (var word in wordsToHighlight)
				{
					products.ForEach(p =>
					{
						if (!string.IsNullOrWhiteSpace(p.Description))
						{
							p.Description = Regex.Replace(p.Description, $"\\b{Regex.Escape(word)}\\b", $"<em>{word}</em>", RegexOptions.IgnoreCase);
						}
					});
				}
			}

			return Ok(new { Product=products,FilterOptions=filterInfo});
		}

		private static List<string> GetMostCommonWords(IEnumerable<Product> products, int count = 10)
		{
			var wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var product in products)
			{
				// Split the description into words and filter out non-alphabetic characters
				var words = Regex.Split(product.Description, @"\W+");

				foreach (var word in words)
				{
					if (string.IsNullOrWhiteSpace(word)) continue;

					// Exclude stop words
					if (StopWords.Contains(word)) continue;

					// Count the frequency of each word
					if (wordFrequency.ContainsKey(word))
						wordFrequency[word]++;
					else
						wordFrequency[word] = 1;
				}
			}

			// Order words by frequency and select the top `count` words
			return wordFrequency
				.OrderByDescending(kv => kv.Value)
				.Take(count)
				.Select(kv => kv.Key)
				.ToList();
		}
	}
}
