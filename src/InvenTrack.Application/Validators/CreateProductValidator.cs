using FluentValidation;
using InvenTrack.Application.DTOs.Products;

namespace InvenTrack.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\-]+$").WithMessage("SKU must be alphanumeric with dashes only");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0")
            .LessThan(999999999).WithMessage("Unit price is too large");

        RuleFor(x => x.CostPrice)
            .GreaterThan(0).WithMessage("Cost price must be greater than 0")
            .LessThan(999999999).WithMessage("Cost price is too large")
            .LessThanOrEqualTo(x => x.UnitPrice).WithMessage("Cost price must be less than or equal to unit price");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be 0 or greater");

        RuleFor(x => x.MinStockThreshold)
            .GreaterThanOrEqualTo(0).WithMessage("Min stock threshold must be 0 or greater");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
    }
}
