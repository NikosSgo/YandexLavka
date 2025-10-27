using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record UpdateStockCommand(Guid ProductId, int Quantity, string Location, string Operation)
    : IRequest<Unit>;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Operation).NotEmpty().Must(BeValidOperation)
            .WithMessage("Operation must be 'restock' or 'adjust'");
    }

    private bool BeValidOperation(string operation)
    {
        return operation == "restock" || operation == "adjust";
    }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Unit>
{
    private readonly IStorageUnitRepository _storageUnitRepository;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(
        IStorageUnitRepository storageUnitRepository,
        ILogger<UpdateStockCommandHandler> logger)
    {
        _storageUnitRepository = storageUnitRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating stock for product {ProductId} at {Location}. Operation: {Operation}",
            request.ProductId, request.Location, request.Operation);

        var storageUnit = await _storageUnitRepository.GetByProductAndLocationAsync(
            request.ProductId, request.Location);

        if (storageUnit == null)
            throw new NotFoundException($"Storage unit not found for product {request.ProductId} at {request.Location}");

        if (request.Operation == "restock")
        {
            storageUnit.Restock(request.Quantity);
        }
        else if (request.Operation == "adjust")
        {
            // Для корректировки устанавливаем точное количество
            // В реальном приложении здесь была бы более сложная логика
            throw new DomainException("Adjust operation not implemented yet");
        }

        await _storageUnitRepository.UpdateAsync(storageUnit);
        await _storageUnitRepository.SaveChangesAsync();

        _logger.LogInformation("Stock updated for product {ProductId}. New quantity: {Quantity}",
            request.ProductId, storageUnit.Quantity);

        return Unit.Value;
    }
}