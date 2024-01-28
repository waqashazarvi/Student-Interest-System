using Students_Interest_System.Models;
var builder = WebApplication.CreateBuilder(args);
// Configure logging
builder.Logging.AddConsole(); // Add console logging
builder.Logging.AddDebug(); // Add debug logging
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register LoggerService as a singleton
builder.Services.AddSingleton<LoggerService>(provider =>
{
    // Specify the path to your log file
    string logFilePath = "LoggerFile.txt";
    return new LoggerService(logFilePath);
});
var app = builder.Build();
app.UseSession();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Student}/{action=Login}/{id?}");
app.MapFallback(context =>
{
    context.Response.Redirect("/Student/Login");
    return Task.CompletedTask;
});
app.Run();
