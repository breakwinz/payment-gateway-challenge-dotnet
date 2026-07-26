using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.AcquiringBank;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Validation failures become the challenge's "Rejected" response instead of ProblemDetails.
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        new PostPaymentRejectedResponse(
            PaymentStatus.Rejected,
            context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToArray())));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPaymentsRepository, InMemoryPaymentsRepository>();

builder.Services.AddHttpClient<IBankClient, BankClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BankSimulator:BaseUrl"] ?? "http://localhost:8080");
    client.Timeout = TimeSpan.FromMilliseconds(
        builder.Configuration.GetValue("BankSimulator:TimeoutMilliseconds", 5000));
});

builder.Services.AddScoped<PaymentService>(); // scoped: typed HttpClient must not be captured by a singleton

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
