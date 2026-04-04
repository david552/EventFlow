using EventFlow.API.infrastructures.Extensions;
using EventFlow.API.infrastructures.JWT;
using EventFlow.API.Middlewares;
using EventFlow.Application.Users.Validators;
using EventFlow.Domain.Users;
using EventFlow.Persistence.Context;
using EventFlow.Persistence.Seed;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "RealState API", Version = "v1" });

    // 1. განვსაზღვროთ უსაფრთხოების სქემა (Security Scheme)
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "გთხოვთ შეიყვანოთ ტოკენი ფორმატში: Bearer {თქვენი_ტოკენი}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // 2. დავამატოთ მოთხოვნა, რომ Swagger-მა გამოიყენოს ეს სქემა
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddMemoryCache();
builder.Services.AddServices();
builder.Services.AddValidatorsFromAssembly(typeof(UserRegisterRequestModelValidator).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddTokenAuthentication(builder.Configuration["JWTConfiguration:Secret"]);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<User>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole<int>>() 
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.Configure<JWTConfiguration>(builder.Configuration.GetSection(nameof(JWTConfiguration)));


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

await DatabaseSeeder.InitializeAsync(app.Services);
app.Run();
