using WebApplication1.Models;

namespace WebApplication1.Data
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAll();
        Product? Get(int id);
        Product Add(Product product);
        Product? Update(Product product);
    }
}
