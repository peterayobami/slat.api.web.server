using Microsoft.EntityFrameworkCore;
using Slat.Core;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add and configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        migration => migration.MigrationsAssembly("Slat.Api.Web.Server"));
});

// Add and cofigure Authentication
//builder.Services.AddAuthentication()
//    .AddIdentityServerJwt()
//    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
//    {
//        options.Authority = builder.Configuration["Jwt:Authority"];
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = false,
//            ValidTypes = new[] { "at+jwt" }
//        };
//    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientAppsAccess", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Create a scope
var scope = app.Services.CreateScope();

// Migrate the database with the scope
scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ClientAppsAccess");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
