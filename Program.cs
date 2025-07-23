using Hangfire;
using Hangfire.MySql;
using HangfireDemo.Filter;
using HangfireDemo.Models;
using HangfireDemo.Services.ServiceImp;
using HangfireDemo.Services;
using SqlSugar;


var builder = WebApplication.CreateBuilder(args);

// ����SqlSugar����
builder.Services.AddScoped<ISqlSugarClient>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlSugarScope(new ConnectionConfig()
    {
        ConnectionString = connectionString,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true,
        InitKeyType = InitKeyType.Attribute
    });
});

var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection");
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(
        new MySqlStorage(
            hangfireConnection,
            new MySqlStorageOptions
            {
                TablesPrefix = "hf_", // ��ǰ׺
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true, // �Զ�������
                TransactionTimeout = TimeSpan.FromMinutes(1)
            })));

builder.Services.AddHangfireServer();

builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ��ʼ�����ݿ�
InitializeDatabase(app.Services);

// �����м��
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Hangfire Dashboard",
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseAuthorization();
app.MapControllers();

app.Run();

void InitializeDatabase(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

    // ����ʾ����
    db.CodeFirst.InitTables<User>();
}
