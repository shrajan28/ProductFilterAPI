using System.Text.Json.Serialization;
using ProductFilterAPI.Models;

namespace ProductFilterAPI
{
	[JsonSerializable(typeof(object))]
	[JsonSerializable(typeof(ProductList))]
	public partial class AppJsonSerializerContext : JsonSerializerContext
	{
	}
}
