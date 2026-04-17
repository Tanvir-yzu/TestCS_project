using WebApplication1.Models;

namespace WebApplication1.Data
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        User Add(User user);
        User? Update(User user, string? oldUsername = null);
    }
}
