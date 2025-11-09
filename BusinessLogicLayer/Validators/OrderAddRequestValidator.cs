
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderAddRequestValidator : FluentValidation.AbstractValidator<OrderAddRequest> 
{
    public OrderAddRequestValidator()
    {
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