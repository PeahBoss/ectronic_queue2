using ElectronicQueue.Infrastructure;       
using ElectronicQueue.Api.Patients;         
using ElectronicQueue.Api.Specializations;   


var builder = WebApplication.CreateBuilder(args);

// ����������� �������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ����������� �������� ������ ������� (�����������, ������� � �.�.)
builder.Services.AddFullElectronicQueue();

var app = builder.Build();

// ��������� Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

// ������������ ��������� ����� IEndpointRouteBuilder
app.UseRouting();
#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapPatients();
    endpoints.MapSpecializations();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.Run();
