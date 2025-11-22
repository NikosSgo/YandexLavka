using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record ClearCartCommand(string CustomerId) : IRequest<Unit>;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearCartCommandHandler> _logger;

    public ClearCartCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ClearCartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Clearing cart for customer {CustomerId}", request.CustomerId);

            await _unitOfWork.Cart.DeleteByCustomerIdAsync(request.CustomerId);
            await _unitOfWork.CommitAsync();

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error clearing cart");
            throw;
        }
    }
}

