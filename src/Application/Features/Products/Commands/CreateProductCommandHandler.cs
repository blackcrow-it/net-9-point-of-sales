using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate SKU uniqueness
        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU, cancellationToken);
        if (skuExists)
            return Result<Guid>.Failure("SKU already exists");

        // Validate barcode uniqueness if provided
        if (!string.IsNullOrEmpty(request.Barcode))
        {
            var barcodeExists = await _context.ProductVariants
                .AnyAsync(pv => pv.Barcode == request.Barcode, cancellationToken);
            if (barcodeExists)
                return Result<Guid>.Failure("Barcode already exists");
        }

        // Create product
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = request.SKU,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Description = request.Description,
            Type = ProductType.Single,
            CostPrice = request.Cost,
            RetailPrice = request.Price,
            Unit = "pcs",
            TrackInventory = request.TrackInventory,
            IsActive = request.IsActive
        };

        await _context.Products.AddAsync(product, cancellationToken);

        // Create default variant
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            SKU = request.SKU,
            Name = "Default",
            Barcode = request.Barcode,
            RetailPrice = request.Price,
            CostPrice = request.Cost,
            IsDefault = true,
            IsActive = true
        };

        await _context.ProductVariants.AddAsync(variant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}
