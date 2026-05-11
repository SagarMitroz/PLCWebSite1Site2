using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Water_Filtration.Models.data;

using Water_Filtration;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure AWS S3
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

// Email service
builder.Services.AddScoped<EmailService>();

// Configure MySQL
builder.Services.AddDbContext<DbPlcOnlineContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("Defaultconnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    ));

// Add services
builder.Services.AddControllersWithViews();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <-- Important

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run();