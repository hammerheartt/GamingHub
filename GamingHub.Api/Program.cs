using GamingHub.Api.Data;
using GamingHub.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        o.AllowInputFormatterExceptionMessages = false;
    });
builder.Services.AddAutoMapper(typeof(Program));

var connectionString = builder.Configuration.GetConnectionString("GamingHub");
builder.Services.AddDbContext<GamingHubDbContext>(o => o.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IStationsRepository, StationsRepository>();
builder.Services.AddScoped<IReservationsRepository, ReservationsRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<GamingHubDbContext>().Database.Migrate();
}

app.MapControllers();

app.Run();


