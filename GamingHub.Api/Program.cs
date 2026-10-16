using GamingHub.Api.Repositories;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        o.AllowInputFormatterExceptionMessages = false;
    });
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IRoomsRepository, RoomsRepository>();

var app = builder.Build();

app.MapControllers();

app.Run();


