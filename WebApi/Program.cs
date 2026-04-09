using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Domain.Entities;
using WebApi.Domain.Enums;
using WebApi.Domain.Services;
using WebApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi().AddWebApi().AddPersistence().AddOutbox();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost(
    "api/v1/orders",
    async (
        CreateOrderRequest request,
        AppDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken
    ) =>
    {
        var order = Order.Create(request.Description, dateTimeProvider.UtcNow);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/v1/orders/{order.Id}", new CreateOrderResponse(order.Id));
    }
);

app.MapGet(
    "api/v1/orders/{id:guid}",
    async (Guid id, AppDbContext dbContext, CancellationToken cancellationToken) =>
    {
        var order = await dbContext
            .Orders.Where(order => order.Id == id)
            .Select(order => new OrderDto(
                order.Id,
                order.Description,
                order.Status,
                order.StatusAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return order is not null ? Results.Ok(new OrderResponse(order)) : Results.NotFound();
    }
);

app.MapGet(
    "api/v1/orders",
    async (AppDbContext dbContext, CancellationToken cancellationToken) =>
    {
        var orders = await dbContext
            .Orders.Select(order => new OrderDto(
                order.Id,
                order.Description,
                order.Status,
                order.StatusAt
            ))
            .ToListAsync(cancellationToken);
        return orders is not null ? Results.Ok(new OrderListResponse(orders)) : Results.NotFound();
    }
);

app.Run();

public record CreateOrderRequest(string Description);

public record CreateOrderResponse(Guid Id);

public record OrderListResponse(List<OrderDto> Orders);

public record OrderResponse(OrderDto Order);

public record OrderDto(Guid Id, string Description, OrderStatus Status, DateTime StatusAt);
