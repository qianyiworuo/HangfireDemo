using Hangfire;
using HangfireDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJob;
        private readonly IRecurringJobManager _recurringJob;
        private readonly IUserService _userService;

        public UserController(
            IBackgroundJobClient backgroundJob,
            IRecurringJobManager recurringJob,
            IUserService userService)
        {
            _backgroundJob = backgroundJob;
            _recurringJob = recurringJob;
            _userService = userService;
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
        {
            // 立即执行：创建用户
            var jobId = _backgroundJob.Enqueue(() =>
                _userService.CreateUserAsync(request.Name, request.Email));

            // 延迟执行：发送欢迎邮件（5分钟后）
            _backgroundJob.Schedule(
                () => _userService.SendWelcomeEmailAsync(1), // 实际应用中应传递用户ID
                TimeSpan.FromMinutes(5));

            return Ok(new { JobId = jobId });
        }

        [HttpPost("start-cleanup")]
        public IActionResult StartCleanupJob()
        {
            // 定时任务：每天凌晨清理（CRON表达式）
            _recurringJob.AddOrUpdate("cleanup-job",
                () => CleanupInactiveUsers(),
                Cron.Daily);

            return Ok("Cleanup job scheduled");
        }

        // Hangfire调用的方法必须是public
        public async Task CleanupInactiveUsers()
        {
            // 实际清理逻辑
            await Task.Delay(1000);
            Console.WriteLine("Performing user cleanup...");
        }
    }
    public record CreateUserRequest(string Name, string Email);
}
