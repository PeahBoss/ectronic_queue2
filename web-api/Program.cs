using ElectronicQueue.Infrastructure;       
using ElectronicQueue.Api.Patients;         
using ElectronicQueue.Api.Specializations;   


var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация сервисов вашего проекта (репозитории, сервисы и т.д.)
builder.Services.AddFullElectronicQueue();

var app = builder.Build();

// Включение Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// Регистрируем эндпоинты через IEndpointRouteBuilder
app.UseRouting();
#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapPatients();
    endpoints.MapSpecializations();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.Run();
