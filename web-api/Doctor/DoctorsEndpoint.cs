using ElectronicQueue.Application.Doctors;

namespace ElectronicQueue.Api.Doctors
{
    internal static class DoctorsEndpoint
    {
        public static string ApiRoute = "api/doctors";

        // Метод расширения для IEndpointRouteBuilder, а не IApplicationBuilder
        public static IEndpointRouteBuilder MapDoctors(this IEndpointRouteBuilder endpoints)
        {
            // Получаем сервис из DI контейнера
            var doctorCommand = endpoints.ServiceProvider.GetService<GetAllDoctorsCommand>();

            // Регистрируем GET эндпоинт
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
            endpoints.MapGet(ApiRoute, () => doctorCommand.ExecuteAsync(new()));
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.

            return endpoints;
        }
    }
}
