using API.Extensions;
using API.Helpers;
using API.Middleware;
using Core.Entities.Identity;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);






// Add services to the container.
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityConnection")));


builder.AddIdentityServices();

//Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
    return ConnectionMultiplexer.Connect(configuration);
});


// uruchamianie migracji na starcie--->
await using var provider = builder.Services.BuildServiceProvider();

using (var scope = provider.CreateScope())
{
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        await context.Database.MigrateAsync();
        //zasilenie bazy z plikow
        await StoreContextSeed.SeedAsync(context, loggerFactory);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var identityContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        await identityContext.Database.MigrateAsync();
        await AppIdentityDbContextSeed.SeedUsersAsync(userManager);

    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured during migration");
    }
}
// <--- uruchamianie migracji na starcie

builder.Services.AddCors(opt =>
    opt.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200")));

builder.Services.AddAutoMapper(typeof(MappingProfiles));

builder.Services.AddControllers();

builder.AddApplicationServices();


builder.AddSwaggerDocumentation();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    //mamy inna obsluge bledow
    // app.UseDeveloperExceptionPage();


    // app.UseSwagger();
    // app.UseSwaggerUI();
    
}

app.UseSwaggerDocumentation();


//strona dla errorow nieobsluzonych
app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers();

app.Run();