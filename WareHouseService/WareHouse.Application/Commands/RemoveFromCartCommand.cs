using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record RemoveFromCartCommand(string CustomerId, Guid CartItemId) : IRequest<Unit>;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromCartCommandHandler> _logger;

    public RemoveFromCartCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromCartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Removing cart item {CartItemId} for customer {CustomerId}", 
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

            await _unitOfWork.Cart.DeleteAsync(cartItem);
            await _unitOfWork.CommitAsync();

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error removing cart item");
            throw;
        }
    }
}

