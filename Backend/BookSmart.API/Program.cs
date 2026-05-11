using System.Text;
using AppointmentSystem.API.Data;
using AppointmentSystem.API.Models;
using AppointmentSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<PredictionService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=appointment.db"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    bool adminExists = db.Users.Any(u => u.Email == "admin@booksmart.com");
    if (!adminExists)
    {
        db.Users.Add(new User
        {
            FullName = "Admin",
            Email = "admin@booksmart.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Admin
        });
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();