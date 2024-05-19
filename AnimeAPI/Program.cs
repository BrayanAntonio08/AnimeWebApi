using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Repositories;
using AnimeAPI.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


//JWT Configuration made by ChatGPT
// key initialization
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);

builder.Services.AddAuthorization();
// implementation of jwt configuration
builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
{
    var signingKey = new SymmetricSecurityKey(key);
    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);

    opt.RequireHttpsMetadata = false;

    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = signingKey,
    };
});
// add service to generate tokens in controllers
builder.Services.AddSingleton<JwtService>();


// Add DbContext configuration
builder.Services.AddDbContext<AnimeDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// Add repository interfaces and implementations
builder.Services.AddScoped<IAnimeRepository, AnimeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowOrigin");

app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
