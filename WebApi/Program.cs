using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using WebApi.CustomExceptionMiddleware;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

//builder.Logging = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

builder.Services.CongigureAppSettings(builder.Configuration);

builder.Services.ConfigureSwagger();

builder.Services.ConfigureCors();

builder.Services.ConfigureSqlContext(builder.Configuration);

builder.Services.ConfigureRepository();
builder.Services.ConfigureBusinessServices();

//// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
}); ;

builder.Services.ConfigureAuthentication();
builder.Services.AddSwaggerGen();

//builder.Services.AddHostedService<MyBackgroundService>();  //enable to run backgrund services
var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwagger();
app.UseSwaggerUI();

//var logger = app.Services.GetRequiredService<ILoggerManager>();
//app.ConfigureExceptionHandler();
//app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.Run();
