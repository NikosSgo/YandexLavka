using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record UpdateCartItemCommand(string CustomerId, Guid CartItemId, int Quantity) : IRequest<CartItemDto>;

public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CartItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCartItemCommandHandler> _logger;

    public UpdateCartItemCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCartItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CartItemDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Updating cart item {CartItemId} for customer {CustomerId}", 
                request.CartItemId, request.CustomerId);

            var cartItem = await _unitOfWork.Cart.GetByIdAsync(request.CartItemId);
            if (cartItem == null)
            {
                throw new Exception($"Cart item {request.CartItemId} not found");
            }

            if (cartItem.CustomerId != request.CustomerId)
            {
                throw new UnauthorizedAccessException("Cart item does not belong to this customer");
            }

            cartItem.UpdateQuantity(request.Quantity);
            await _unitOfWork.Cart.UpdateAsync(cartItem);

            var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                throw new Exception($"Product {cartItem.ProductId} not found");
            }

            await _unitOfWork.CommitAsync();

            return MapToDto(cartItem, product);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating cart item");
            throw;
        }
    }

    private CartItemDto MapToDto(Domain.Entities.CartItem cartItem, Domain.Entities.Product product)
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

