using ElectronicQueue.Application.Interfaces;
using ElectronicQueue.Core.Entities;

namespace ElectronicQueue.Api.Specializations
{
    internal static class SpecializationsEndpoint
    {
        public static string ApiRoute = "api/specializations";

        public static IEndpointRouteBuilder MapSpecializations(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(ApiRoute, async context =>
            {
                var repo = context.RequestServices.GetRequiredService<IRepository<Specialization>>();
                var specializations = await repo.GetAllAsync();

                await context.Response.WriteAsJsonAsync(specializations);
            });

            return endpoints;
        }
    }
}
