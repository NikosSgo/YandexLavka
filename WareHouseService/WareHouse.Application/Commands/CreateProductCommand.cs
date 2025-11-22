using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record CreateProductCommand(
    string Name,
    string Sku,
    string Description,
    string Category,
    decimal UnitPrice,
    decimal? WeightKg,
    bool RequiresRefrigeration
) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Product name is required and must not exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("SKU is required and must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Category is required and must not exceed 100 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .When(x => x.WeightKg.HasValue)
            .WithMessage("Weight must be greater than zero if provided");
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Creating new product with SKU {Sku}", request.Sku);

            // Проверяем, не существует ли уже продукт с таким SKU
            var existingProducts = await _unitOfWork.Products.GetAllAsync();
            if (existingProducts.Any(p => p.Sku.Equals(request.Sku, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Product with SKU '{request.Sku}' already exists");
            }

            // Создаем новый продукт
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Sku = request.Sku,
                Description = request.Description ?? string.Empty,
                Category = request.Category,
                UnitPrice = request.UnitPrice,
                WeightKg = request.WeightKg,
                RequiresRefrigeration = request.RequiresRefrigeration,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Product created successfully with ID {ProductId} and SKU {Sku}", 
                product.Id, product.Sku);

            return MapToDto(product);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating product with SKU {Sku}", request.Sku);
            throw;
        }
    }

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Category = product.Category,
            UnitPrice = product.UnitPrice,
            WeightKg = product.WeightKg,
            RequiresRefrigeration = product.RequiresRefrigeration,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}

