using CitiesManager.Core.Identity;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using CitiesManager.Core.ServiceContracts;
using CitiesManager.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This is because we are not going to create views in this project
builder.Services.AddControllers(options => {
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new ConsumesAttribute("application/json"));

    //Authorization policy
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddXmlSerializerFormatters(); //This XML is because we added a XML in the controller

//Adding jwt service
builder.Services.AddTransient<IJwtService, JwtService>();

//Adding versioning
builder.Services.AddApiVersioning(config =>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader(); //Reads version number from request url at "apiVersion" constraint
    //config.ApiVersionReader = new QueryStringApiVersionReader(); //Reads version number from request query string called "api-version". Eg: api-version=1.0
    //config.ApiVersionReader = new HeaderApiVersionReader("api-version"); //Reads version number from request header called "api-version". Eg: api-version: 1.0
 
     config.DefaultApiVersion = new ApiVersion(1, 0); // This is to set the default version
     config.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

//Swagger
builder.Services.AddEndpointsApiExplorer(); // Generates description for all endpoints
builder.Services.AddSwaggerGen(options => {
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "1.0" });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Cities Web API", Version = "2.0" });
}); //generates OpenAPI specification

builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV"; //v1
    options.SubstituteApiVersionInUrl = true;
});

//Add the CORS for Angular app CORS: localhost:4200 port number of angular application
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept", "content-type")
        .WithMethods("GET", "POST", "PUT", "DELETE"); //Operations allowed

    });

    options.AddPolicy("4100Client", policyBuilder =>
    {
        policyBuilder
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins2").Get<string[]>())
        .WithHeaders("Authorization", "origin", "accept")
        .WithMethods("GET"); //Operations allowed

    });
});

//Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
})
 .AddEntityFrameworkStores<ApplicationDbContext>()
 .AddDefaultTokenProviders()
 .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
 .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

//JWT
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
 .AddJwtBearer(options => {
     options.TokenValidationParameters = new TokenValidationParameters()
     {
         ValidateAudience = true,
         ValidAudience = builder.Configuration["Jwt:Audience"],
         ValidateIssuer = true,
         ValidIssuer = builder.Configuration["Jwt:Issuer"],
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
     };
 });

builder.Services.AddAuthorization(options => {
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger(); // creates endpoint for swagger.json
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
}); // creates swagger UI for testing all Web API endpoints / action methods

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
