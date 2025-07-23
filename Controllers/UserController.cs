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
            // 使用ContinueJobWith来获取前一个任务的返回值
        //    _backgroundJob.ContinueJobWith(
        //         jobId, 
        //         () => _userService.SendWelcomeEmailAsync(
        //             _userService.GetUserAsync(request.Email).Result.Id),
        //         JobContinuationOptions.OnAnyFinishedState);
            // 如果需要延迟5分钟，应该使用Schedule方法
            // 或者可以这样实现延迟：
            _backgroundJob.Schedule(
               () => _userService.SendWelcomeEmailAsync(
                   _userService.GetUserAsync(request.Email).Result.Id),
               TimeSpan.FromMinutes(5));

            return Ok(new { JobId = jobId });
        }
        [HttpGet("/{sEmail}")]
        public IActionResult GetUsers(string sEmail)
        {
            var user = _userService.GetUserAsync(sEmail);
            return Ok(new { user.Result.Name, user.Result.Email });
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
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task CleanupInactiveUsers()
        {
            // 实际清理逻辑
            await Task.Delay(1000);
            Console.WriteLine("Performing user cleanup...");
        }
    }
    public record CreateUserRequest(string Name, string Email);
}
