using Application.Common.Models;
using MediatR;

namespace Application.Features.Reports.Queries;

public record FinancialReportQuery(
    Guid StoreId,
    DateTime DateFrom,
    DateTime DateTo
) : IRequest<Result<FinancialReportDto>>;

public record FinancialReportDto(
    Guid StoreId,
    DateTime DateFrom,
    DateTime DateTo,
    decimal TotalRevenue,
    decimal TotalCash,
    decimal TotalCard,
    decimal TotalEWallet,
    decimal TotalRefunds,
    decimal NetRevenue,
    List<PaymentMethodBreakdown> PaymentBreakdown
);

public record PaymentMethodBreakdown(
    string PaymentMethod,
    decimal Amount,
    int TransactionCount
);
