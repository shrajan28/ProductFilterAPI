using System.Collections.ObjectModel;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProductFilterAPI.Controllers;
using ProductFilterAPI.Models;
using ProductFilterAPI.Services;
using Xunit;

namespace ProductFilterAPITests
{
	public class FilterControllerTests
	{
		private readonly Mock<IProductService> _mockProductService;
		private readonly Mock<ILogger<FilterController>> _mockLogger;
		private readonly FilterController _controller;

		public FilterControllerTests()
		{
			_mockProductService = new Mock<IProductService>();
			_mockLogger = new Mock<ILogger<FilterController>>();
			_controller = new FilterController(_mockProductService.Object, _mockLogger.Object);
		}

		private static ProductList GetSampleProductList() => new ProductList
		{
			Products = new Collection<Product>
			{
				new Product
				{
					Title = "Red Trouser",
					Price = 10,
					Sizes = new List<string> { "small", "medium", "large" },
					Description = "This trouser pairs with a green shirt."
				},
				new Product
				{
					Title = "Blue Shirt",
					Price = 15,
					Sizes = new List<string> { "medium", "large" },
					Description = "Ideal for a formal event."
				},
				new Product
				{
					Title = "Green Jacket",
					Price = 25,
					Sizes = new List<string> { "large", "x-large" },
					Description = "Perfect for winter and cold weather."
				}
			}
		};

		[Fact]
		public async Task GetFilteredProducts_NoParameters_ReturnsAllProducts()
		{
			// Arrange
			var sampleData = GetSampleProductList();
			_mockProductService.Setup(service => service.GetProductsAsync()).ReturnsAsync(sampleData);

			// Act
			var result = await _controller.GetFilteredProducts(null, null, null, null);
			var okResult = Assert.IsType<OkObjectResult>(result);
			var response = Assert.IsType<FilteredProductResponse>(okResult.Value);

			// Assert
			Assert.NotNull(response);
			Assert.Equal(3, response.Product.Count); // All products returned
		}

		[Fact]
		public async Task GetFilteredProducts_WithMinAndMaxPrice_ReturnsFilteredProducts()
		{
			// Arrange
			var sampleData = GetSampleProductList();
			_mockProductService.Setup(service => service.GetProductsAsync()).ReturnsAsync(sampleData);

			// Act
			var result = await _controller.GetFilteredProducts(10, 20, null, null);
			var okResult = Assert.IsType<OkObjectResult>(result);
			var response = Assert.IsType<FilteredProductResponse>(okResult.Value);

			// Assert
			Assert.NotNull(response);
			Assert.Equal(2, response.Product.Count); // Two products with price between 10 and 20
			Assert.All(response.Product, p => Assert.InRange(p.Price, 10, 20));
		}

		[Fact]
		public async Task GetFilteredProducts_WithSize_ReturnsFilteredProductsBySize()
		{
			// Arrange
			var sampleData = GetSampleProductList();
			_mockProductService.Setup(service => service.GetProductsAsync()).ReturnsAsync(sampleData);

			// Act
			var result = await _controller.GetFilteredProducts(null, null, "medium", null);
			var okResult = Assert.IsType<OkObjectResult>(result);
			var response = Assert.IsType<FilteredProductResponse>(okResult.Value);

			// Assert
			Assert.NotNull(response);
			Assert.All(response.Product, p => Assert.Contains("medium", p.Sizes));
		}

		[Fact]
		public async Task GetFilteredProducts_WithHighlight_ReturnsHighlightedDescriptions()
		{
			// Arrange
			var sampleData = GetSampleProductList();
			_mockProductService.Setup(service => service.GetProductsAsync()).ReturnsAsync(sampleData);

			// Act
			var result = await _controller.GetFilteredProducts(null, null, null, "green");
			var okResult = Assert.IsType<OkObjectResult>(result);
			var response = Assert.IsType<FilteredProductResponse>(okResult.Value);

			// Assert
			Assert.NotNull(response);
			Assert.Contains(response.Product, p => p.Description.Contains("<em>green</em>"));
		}

		[Fact]
		public async Task GetFilteredProducts_ValidatesFilterInfoObject()
		{
			// Arrange
			var sampleData = GetSampleProductList();
			_mockProductService.Setup(service => service.GetProductsAsync()).ReturnsAsync(sampleData);

			// Act
			var result = await _controller.GetFilteredProducts(null, null, null, null);
			var okResult = Assert.IsType<OkObjectResult>(result);
			var response = Assert.IsType<FilteredProductResponse>(okResult.Value);

			// Assert
			Assert.NotNull(response);
			var filterInfo = response.FilterOptions;

			Assert.Equal(10, filterInfo.MinPrice);
			Assert.Equal(25, filterInfo.MaxPrice);
			Assert.Contains("small", filterInfo.Sizes);
			Assert.Contains("large", filterInfo.Sizes);
			Assert.True(filterInfo.CommonWords.Count <= 10);
		}


	}
}

