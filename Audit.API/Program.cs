using Audit.Services;
using Audit.Services.Interfaces;
using AuditPilot.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IShareHolderService, ShareHolderService>();
builder.Services.AddScoped<ITrialBalanceService, TrialBalanceService>();
builder.Services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
builder.Services.AddScoped<IAdjustmentService, AdjustmentService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Audit", Version = "v1" });
    // Avoid schema collisions and normalize namespace brand to "Audit"
    c.CustomSchemaIds(type =>
    {
        static string BuildTypeSegment(Type current)
        {
            if (current.IsArray)
            {
                return $"ArrayOf{BuildTypeSegment(current.GetElementType()!)}";
            }

            if (current.IsGenericType)
            {
                var genericRoot = current.Name[..current.Name.IndexOf('`')];
                var builder = new StringBuilder(genericRoot);
                foreach (var argument in current.GetGenericArguments())
                {
                    builder.Append(BuildTypeSegment(argument));
                }

                return builder.ToString();
            }

            return current.Name;
        }

        var ns = type.Namespace ?? string.Empty;
        ns = ns.Replace("AuditPilot", "Audit").Replace("AuthPilot", "Audit");
        var parts = ns.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var reduced = parts.Length >= 2 ? string.Join('.', parts[^2], parts[^1]) : ns;
        var core = BuildTypeSegment(type);
        var id = string.IsNullOrWhiteSpace(reduced) ? core : $"{reduced}.{core}";
        return id.Replace('+', '.');
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token prefixed with Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
    c.OperationFilter<AuditPilot.API.Swagger.TrialBalanceExamplesFilter>();
    c.OperationFilter<AuditPilot.API.Swagger.FiscalPeriodExamplesFilter>();
    c.OperationFilter<AuditPilot.API.Swagger.AdjustmentExamplesFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit v1");
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}


app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Minimal seeding for smoketest
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Ensure latest migrations are applied
    db.Database.Migrate();
    if (!db.Accounts.Any())
    {
        db.Accounts.AddRange(
            new AuditPilot.Data.Entities.Account { Code = "10140", Name = "Cash", IsActive = true },
            new AuditPilot.Data.Entities.Account { Code = "10150", Name = "Bank", IsActive = true },
            new AuditPilot.Data.Entities.Account { Code = "20100", Name = "Payables", IsActive = true }
        );
        db.SaveChanges();
    }
    if (!db.FiscalPeriods.Any())
    {
        var clientId = db.Clients.Select(c => c.Id).FirstOrDefault();
        if (clientId == Guid.Empty)
        {
            var c = new AuditPilot.Data.Entities.Client { Name = "Acme", Email = "acme@example.com", CompanyType = Audit.Common.CompanyType.Private };
            db.Clients.Add(c);
            db.SaveChanges();
            clientId = c.Id;
        }
        db.FiscalPeriods.Add(new AuditPilot.Data.Entities.FiscalPeriod
        {
            ClientId = clientId,
            Name = "FY 2024",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            IsLocked = false,
            CreatedAtUtc = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

app.Run();

