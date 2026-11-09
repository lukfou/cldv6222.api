using System.Text;
using CLDV6212POE1.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register controllers.
builder.Services.AddControllers();

// Register application services.
builder.Services.AddSingleton<UserTableService>();
builder.Services.AddSingleton<JwtTokenService>();

// Register the backend HTTP client.
builder.Services.AddHttpClient();

// Read JWT settings.
string jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is missing.");

string jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing.");

string jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing.");

// Configure JWT authentication.
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                NameClaimType = "name",
                RoleClaimType = "role",

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

// Register authorization for Member 2 to extend.
builder.Services.AddAuthorization();

// Configure Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string xmlFile =
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";

    string xmlPath =
        Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Create the Users table during startup.
using (var scope = app.Services.CreateScope())
{
    var userTableService =
        scope.ServiceProvider.GetRequiredService<UserTableService>();

    await userTableService.InitialiseAsync();
}

// Enable Swagger in the development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP to HTTPS where the local launch profile supports it.
app.UseHttpsRedirection();

// Authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints.
app.MapControllers();

app.Run();