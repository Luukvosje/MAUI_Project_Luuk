using Microsoft.EntityFrameworkCore;
using TimeOn.Api.DependencyInjection;
using TimeOn.Api.Services;
using TimeOn.Application.DependencyInjection;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Infrastructure.DependencyInjection;
using TimeOn.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000", "https://0.0.0.0:5001");
}

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin =>
                string.IsNullOrWhiteSpace(origin) ||
                origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                origin.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                origin.Contains("10.0.2.2", StringComparison.OrdinalIgnoreCase))
            .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
