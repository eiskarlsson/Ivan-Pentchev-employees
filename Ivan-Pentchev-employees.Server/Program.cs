using Ivan_Pentchev_employees.Server;
using Ivan_Pentchev_employees.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IUniversalDateParserService, UniversalDateParserService>();
builder.Services.AddSingleton<IUniversalEmployeeCsvParserService, UniversalEmployeeCsvParserService>();
builder.Services.AddSingleton<IEmployeeAlgorithm, EmployeeAlgorithm>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.AllowAnyOrigin() // Angular dev server
                .AllowAnyHeader()
                .AllowAnyMethod();

        });
});

var app = builder.Build();

app.UseCors("AllowAngularApp");

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
