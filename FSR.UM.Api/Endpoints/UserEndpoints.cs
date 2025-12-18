using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dummy.Iam.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", async ([FromServices] IUserService userService) =>
        {
            return Results.Ok(await userService.GetAllAsync());
        });

        app.MapGet("/properties", async ([FromServices] IPropertyRepository repo) =>
        {
            return Results.Ok(await repo.GetAllAsync());
        });

        app.MapPost("/properties", async ([FromBody]Property property, [FromServices] IPropertyRepository repo) =>
        {
            var created = await repo.AddAsync(property);
            return Results.Created($"/properties/{created.Id}", created);
        });
    }
}
