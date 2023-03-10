using Serilog;
using Distributed.Logging.Utilities;
using Distributed.Logging.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddEnvironmentVariables();
}).UseSerilog(SeriLoggerUtility.Configure);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapPost("/Create", (int id) =>
{
    Error();
    return id;
})
.WithName("GetWeatherForecast");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.Run();

void Error()
{
    throw new ArgumentNullException();

}
