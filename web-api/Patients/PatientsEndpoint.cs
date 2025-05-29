using ElectronicQueue.Application.Interfaces;
using ElectronicQueue.Core.Entities;

namespace ElectronicQueue.Api.Patients
{
    internal static class PatientsEndpoint
    {
        public static string ApiRoute = "api/patients";

        public static IEndpointRouteBuilder MapPatients(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(ApiRoute, async context =>
            {
                var repo = context.RequestServices.GetRequiredService<IRepository<Patient>>();
                var patients = await repo.GetAllAsync();

                // Возвращаем JSON (нужно добавить using Microsoft.AspNetCore.Http; для Response.WriteAsJsonAsync)
                await context.Response.WriteAsJsonAsync(patients);
            });

            return endpoints;
        }
    }
}
