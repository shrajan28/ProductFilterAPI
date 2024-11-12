using System.Threading.Tasks;
using ProductFilterAPI.Models;

namespace ProductFilterAPI.Services
{
    public interface IProductService
    {
        Task<ProductList> GetProductsAsync();
    }
}
