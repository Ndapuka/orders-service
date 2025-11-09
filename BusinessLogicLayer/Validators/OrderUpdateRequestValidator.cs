using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : FluentValidation.AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        //OrderID
        RuleFor(temp => temp.OrderID)
            .NotEmpty().WithMessage("Order ID can't be blank");

        //UserID
        RuleFor(temp => temp.UserID)
            .NotEmpty().WithMessage("User ID can't be blank");

        //OrderDate
        RuleFor(temp => temp.OrderDate)
            .NotEmpty().WithMessage("Order Date can't be blank");
        
        //OrderItems
        RuleFor(temp => temp.OrderItems)
            .NotEmpty().WithMessage("Order Items can't be blank");

    }
}
