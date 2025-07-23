using SqlSugar;
using System;

namespace HangfireDemo.Models
{
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn(Length = 100)]
        public string Name { get; set; }

        [SugarColumn(Length = 255)]
        public string Email { get; set; }

        [SugarColumn(InsertServerTime = true)] // 插入时自动设置数据库服务器时间
        public DateTime CreatedAt { get; set; }
    }
}
