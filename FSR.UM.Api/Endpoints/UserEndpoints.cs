using FSR.UM.Core.Interfaces;
using FSR.UM.Core.Models;   

namespace Dummy.Iam.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", (IUserService service) =>
        {
            return Results.Ok(service.GetUsers());
        })
        .WithName("GetUsers")
        .WithTags("Users");

        //app.MapPost("/users", (User user, IUserService userService) =>
        //{
        //    return Results.Created("/users/1", user);
        //})
        //.WithTags("Users");
    }
}
