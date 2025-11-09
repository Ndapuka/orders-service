using AutoMapper;
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;

  public class OrderAddRequestToOrderMappingProfile :Profile
    {
      public OrderAddRequestToOrderMappingProfile()
    {
        CreateMap<OrderAddRequest, Order>()
            .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.UserID))
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.OrderDate))
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

    }
}

