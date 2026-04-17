using System.Collections.Concurrent;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    // Simple file-backed user repo for demo purposes
    public class FileUserRepository : IUserRepository
    {
        private readonly string _path;
        private readonly ConcurrentDictionary<string, User> _users = new();

        public FileUserRepository(IWebHostEnvironment env)
        {
            _path = Path.Combine(env.ContentRootPath, "users.json");
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return;

                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                foreach (var u in list)
                {
                    _users[u.Username] = u;
                }
            }
            catch
            {
                // ignore for demo
            }
        }

        private void Save()
        {
            try
            {
                var list = _users.Values.OrderBy(u => u.Username).ToList();
                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
            catch
            {
                // ignore for demo
            }
        }

        public IEnumerable<User> GetAll() => _users.Values.OrderBy(u => u.Username);

        public User? GetByUsername(string username) => _users.TryGetValue(username, out var u) ? u : null;

        public User? GetByEmail(string email) => _users.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        public User Add(User user)
        {
            _users[user.Username] = user;
            Save();
            return user;
        }

        public User? Update(User user, string? oldUsername = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(oldUsername) && oldUsername != user.Username)
                {
                    // remove old entry if username changed
                    _users.TryRemove(oldUsername, out _);
                }

                _users[user.Username] = user;
                Save();
                return user;
            }
            catch
            {
                return null;
            }
        }
    }
}
