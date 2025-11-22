using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetCartQuery(string CustomerId) : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCartQueryHandler> _logger;

    public GetCartQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCartQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting cart for customer {CustomerId}", request.CustomerId);

        var cartItems = await _unitOfWork.Cart.GetByCustomerIdAsync(request.CustomerId);
        var itemCount = await _unitOfWork.Cart.GetItemCountAsync(request.CustomerId);

        var cartDto = new CartDto
        {
            CustomerId = request.CustomerId,
            ItemCount = itemCount,
            Items = new List<CartItemDto>()
        };

        decimal totalAmount = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for cart item {CartItemId}", 
                    cartItem.ProductId, cartItem.Id);
                continue;
            }

            var itemDto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = cartItem.Quantity,
                UnitPrice = product.UnitPrice,
                TotalPrice = product.UnitPrice * cartItem.Quantity,
                CreatedAt = cartItem.CreatedAt,
                UpdatedAt = cartItem.UpdatedAt ?? cartItem.CreatedAt
            };

            cartDto.Items.Add(itemDto);
            totalAmount += itemDto.TotalPrice;
        }

        cartDto.TotalAmount = totalAmount;

        return cartDto;
    }
}

