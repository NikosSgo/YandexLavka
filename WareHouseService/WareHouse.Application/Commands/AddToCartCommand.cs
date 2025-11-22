using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record AddToCartCommand(string CustomerId, Guid ProductId, int Quantity) : IRequest<CartItemDto>;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToCartCommandHandler> _logger;

    public AddToCartCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<AddToCartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CartItemDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Adding product {ProductId} to cart for customer {CustomerId}", 
                request.ProductId, request.CustomerId);

            // Проверяем существование продукта
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new Exception($"Product {request.ProductId} not found");
            }

            // Проверяем, есть ли уже этот товар в корзине
            var existingItem = await _unitOfWork.Cart.GetByCustomerAndProductAsync(request.CustomerId, request.ProductId);

            CartItem cartItem;
            if (existingItem != null)
            {
                // Если товар уже есть, увеличиваем количество
                existingItem.IncreaseQuantity(request.Quantity);
                await _unitOfWork.Cart.UpdateAsync(existingItem);
                cartItem = existingItem;
            }
            else
            {
                // Создаем новый элемент корзины
                cartItem = new CartItem(Guid.NewGuid(), request.CustomerId, request.ProductId, request.Quantity);
                await _unitOfWork.Cart.AddAsync(cartItem);
            }

            await _unitOfWork.CommitAsync();

            return MapToDto(cartItem, product);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error adding product to cart");
            throw;
        }
    }

    private CartItemDto MapToDto(CartItem cartItem, Product product)
    {
        return new CartItemDto
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
    }
}

