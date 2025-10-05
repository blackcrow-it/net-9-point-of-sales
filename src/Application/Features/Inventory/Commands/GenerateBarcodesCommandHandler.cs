using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Inventory.Commands;

public class GenerateBarcodesCommandHandler : IRequestHandler<GenerateBarcodesCommand, Result<List<BarcodeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GenerateBarcodesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BarcodeDto>>> Handle(GenerateBarcodesCommand request, CancellationToken cancellationToken)
    {
        if (request.ProductVariantIds == null || !request.ProductVariantIds.Any())
            return Result<List<BarcodeDto>>.Failure("No product variants specified");

        var variants = await _context.ProductVariants
            .Include(pv => pv.Product)
            .Where(pv => request.ProductVariantIds.Contains(pv.Id))
            .ToListAsync(cancellationToken);

        if (variants.Count != request.ProductVariantIds.Count)
            return Result<List<BarcodeDto>>.Failure("Some product variants not found");

        var barcodeDtos = new List<BarcodeDto>();

        foreach (var variant in variants)
        {
            // Generate EAN-13 barcode
            var barcode = await GenerateUniqueEAN13(cancellationToken);
            
            variant.Barcode = barcode;

            barcodeDtos.Add(new BarcodeDto(
                variant.Id,
                variant.Product?.Name ?? "Unknown",
                variant.Name,
                barcode,
                variant.Product?.SKU ?? "Unknown"
            ));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<List<BarcodeDto>>.Success(barcodeDtos);
    }

    /// <summary>
    /// Generates a unique EAN-13 barcode
    /// Format: 12 digits + 1 check digit
    /// </summary>
    private async Task<string> GenerateUniqueEAN13(CancellationToken cancellationToken)
    {
        string barcode;
        bool isUnique;

        do
        {
            // Generate 12 random digits
            var random = new Random();
            var digits = new int[12];
            for (int i = 0; i < 12; i++)
            {
                digits[i] = random.Next(0, 10);
            }

            // Calculate check digit using EAN-13 algorithm
            int oddSum = 0;
            int evenSum = 0;
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 0)
                    oddSum += digits[i];
                else
                    evenSum += digits[i];
            }

            int total = oddSum + (evenSum * 3);
            int checkDigit = (10 - (total % 10)) % 10;

            // Construct barcode
            barcode = string.Join("", digits) + checkDigit;

            // Check if barcode already exists
            isUnique = !await _context.ProductVariants
                .AnyAsync(pv => pv.Barcode == barcode, cancellationToken);

        } while (!isUnique);

        return barcode;
    }
}
