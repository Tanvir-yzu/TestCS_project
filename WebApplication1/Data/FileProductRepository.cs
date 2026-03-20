using System.Collections.Concurrent;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    // Very small file-backed repository for demo purposes. Not suitable for production.
    public class FileProductRepository : IProductRepository
    {
        private readonly string _path;
        private readonly ConcurrentDictionary<int, Product> _products = new();
        private int _nextId = 1;

        public FileProductRepository(IWebHostEnvironment env)
        {
            _path = Path.Combine(env.ContentRootPath, "products.json");
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return;

                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                foreach (var p in list)
                {
                    _products[p.Id] = p;
                    if (p.Id >= _nextId) _nextId = p.Id + 1;
                }
            }
            catch
            {
                // ignore errors for demo
            }
        }

        private void Save()
        {
            try
            {
                var list = _products.Values.OrderBy(p => p.Id).ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
            catch
            {
                // ignore errors for demo
            }
        }

        public IEnumerable<Product> GetAll() => _products.Values.OrderBy(p => p.Id);

        public Product? Get(int id) => _products.TryGetValue(id, out var p) ? p : null;

        public Product Add(Product product)
        {
            var id = System.Threading.Interlocked.Increment(ref _nextId) - 1;
            product.Id = id;
            _products[id] = product;
            Save();
            return product;
        }
    }
}
