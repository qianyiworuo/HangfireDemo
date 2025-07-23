using HangfireDemo.Models;

namespace HangfireDemo.Services
{
    public interface IUserService
    {
        Task<int> CreateUserAsync(string name, string email);
        Task SendWelcomeEmailAsync(int userId);
        Task<User> GetUserAsync(string email);
    }
}
