﻿using Hangfire;
using HangfireDemo.Models;
using SqlSugar;

namespace HangfireDemo.Services.ServiceImp
{
    public class UserService : IUserService
    {
        private readonly ISqlSugarClient _db;
        private readonly ILogger<UserService> _logger;
        private readonly IBackgroundJobClient _backgroundJob;

        public UserService(IBackgroundJobClient backgroundJob, ISqlSugarClient db, ILogger<UserService> logger)
        {
            _backgroundJob = backgroundJob;
            _db = db;
            _logger = logger;
        }

        public async Task<int> CreateUserAsync(string name, string email)
        {
            var user = new User { Name = name, Email = email};
            var userId = await _db.Insertable(user).ExecuteReturnIdentityAsync();
            _logger.LogInformation("User created: {Name} ({Id})", name, user.Id);
            return userId;
        }

        public async Task<User> GetUserAsync(string email)
        {
            var user = await _db.Queryable<User>().FirstAsync(u => u.Email == email);
            return user != null ? user : null;
        }

        public async Task SendWelcomeEmailAsync(int userId)
        {
            var user = await _db.Queryable<User>().FirstAsync(u => u.Id == userId);
            _logger.LogInformation("Sending welcome email to {Email}...", user.Email);

            // 模拟邮件发送延迟
            await Task.Delay(2000);

            _logger.LogInformation("Welcome email sent to {Email}", user.Email);
        }
    }
}
