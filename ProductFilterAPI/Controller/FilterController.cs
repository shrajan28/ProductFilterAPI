using Microsoft.AspNetCore.Mvc;
using ProductFilterAPI.Models;
using ProductFilterAPI.Services;
using System.Collections.ObjectModel;

namespace ProductFilterAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FilterController : ControllerBase
	{
		private readonly IProductService _productService; 
		private readonly ILogger<FilterController> _logger;
		private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"the", "is", "and", "of", "a", "in", "to", "for", "with", "on", "that", "this", "it", "by", "an", "as", "at"
		};

		public FilterController(IProductService productService, ILogger<FilterController> logger)
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
			//string? username = HttpContext.Items["Username"] as string;

			//if (username != null)
			//{
			//	_logger.LogInformation("Processing request for user: {Username}", username);
			//}
			_logger.LogInformation("Received filter request with parameters: minprice={MinPrice}, maxprice={MaxPrice}, size={Size}, highlight={Highlight}",
				minprice, maxprice, size, highlight);

			var productsList = await _productService.GetProductsAsync();
			var products = productsList.Products;

			// Convert to a filtered collection
			if (minprice.HasValue)
				products = new Collection<Product>(products.Where(p => p.Price >= minprice.Value).ToList());
			if (maxprice.HasValue)
				products = new Collection<Product>(products.Where(p => p.Price <= maxprice.Value).ToList());
			if (!string.IsNullOrEmpty(size))
				products = new Collection<Product>(products.Where(p => p.Sizes.Contains(size, StringComparer.OrdinalIgnoreCase)).ToList());

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
					foreach (var product in products)
					{
						if (!string.IsNullOrWhiteSpace(product.Description))
						{
							product.Description = HighlightWordsInDescription(product.Description, word);
						}
					}
				}
			}

			return Ok(new FilteredProductResponse { Product = products, FilterOptions = filterInfo });
		}

		private static string HighlightWordsInDescription(string description, string wordToHighlight)
		{
			var words = description.Split(' ');
			for (int i = 0; i < words.Length; i++)
			{
				if (string.Equals(words[i], wordToHighlight, StringComparison.OrdinalIgnoreCase))
				{
					words[i] = $"<em>{words[i]}</em>";
				}
			}
			return string.Join(' ', words);
		}

		private static List<string> GetMostCommonWords(IEnumerable<Product> products, int count = 10)
		{
			var wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var product in products)
			{
				var words = product.Description.Split(' ');

				foreach (var word in words)
				{
					if (string.IsNullOrWhiteSpace(word)) continue;

					if (StopWords.Contains(word)) continue;

					var cleanWord = new string(word.Where(char.IsLetterOrDigit).ToArray());
					if (wordFrequency.ContainsKey(cleanWord))
						wordFrequency[cleanWord]++;
					else
						wordFrequency[cleanWord] = 1;
				}
			}

			return wordFrequency
				.OrderByDescending(kv => kv.Value)
				.Take(count)
				.Select(kv => kv.Key)
				.ToList();
		}
	}
}
