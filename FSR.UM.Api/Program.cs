using Dummy.Iam.Api.Endpoints;
using FSR.UM.Infrastructure.SqlServer;
using FSR.UM.Infrastructure.SqlServer.Db.AuthDb;
using FSR.UM.Infrastructure.SqlServer.Db.PropertyDb;
using Microsoft.EntityFrameworkCore;
using FSR.UM.Infrastructure.SqlServer.Seed;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlServerInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var propertyDb = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
    propertyDb.Database.Migrate();
    PropertyDbSeeder.Seed(propertyDb);

    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    authDb.Database.Migrate();
    AuthDbSeeder.Seed(authDb);
}

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.MapUserEndpoints();

app.Run();
