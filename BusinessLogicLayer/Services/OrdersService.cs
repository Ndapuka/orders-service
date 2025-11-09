using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
//using System.ComponentModel.DataAnnotations;

namespace eCommerce.OrdersMicroservice. BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMapper _mapper;
    private UsersMicroserviceClient _usersMicroserviceClient;
    private ProductsMicroserviceClient _productsMicroserviceClient;


    public OrdersService(
        IOrdersRepository ordersRepository,
        IMapper mapper,
        IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _ordersRepository = ordersRepository;
        _mapper = mapper;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }


    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            Console.WriteLine("[ERROR] OrderAddRequest is null");
            throw new ArgumentNullException(nameof(orderAddRequest));
        }

        // Validate OrderAddRequest
        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(e => e.ErrorMessage));
            Console.WriteLine($"[ERROR] OrderAddRequest validation failed: {errors}");
            throw new ArgumentException(errors);
        }

        List<ProductDTO?> products = new();

        // Validate order items
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult itemValidation = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
            if (!itemValidation.IsValid)
            {
                string errors = string.Join(", ", itemValidation.Errors.Select(e => e.ErrorMessage));
                Console.WriteLine($"[ERROR] OrderItemAddRequest validation failed: {errors}");
                throw new ArgumentException(errors);
            }

            // Check product existence
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);
            if (product == null)
            {
                Console.WriteLine($"[ERROR] Product not found: ProductID={orderItemAddRequest.ProductID}");
                throw new ArgumentException($"Invalid Product ID: {orderItemAddRequest.ProductID}");
            }

            Console.WriteLine($"[INFO] Product loaded: ID={product.ProductID}, Name={product.ProductName}, Category={product.Category}");
            products.Add(product);
        }

        // Check user existence
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user == null)
        {
            
            Console.WriteLine($"[ERROR] User not found: UserID={orderAddRequest.UserID}");
            throw new ArgumentException("Invalid User ID");
        }

        // Map to Order entity
        Order orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }

        orderInput.Totalbill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        // Add to DB
        Order? addedOrder = await _ordersRepository.AddOrder(orderInput);
        if (addedOrder == null)
        {
            Console.WriteLine("[ERROR] Failed to add order to database.");
            return null;
        }

        // Map to response
        OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);

        // Inject ProductName and Category
        for (int i = 0; i < addedOrderResponse.OrderItems.Count; i++)
        {
            var orderItem = addedOrderResponse.OrderItems[i];

            var productDTO = products.FirstOrDefault(p => p?.ProductID == orderItem.ProductID);
            if (productDTO == null)
            {
                Console.WriteLine($"[WARN] ProductDTO not found in list for ProductID={orderItem.ProductID}");
                continue;
            }

            Console.WriteLine($"[INFO] Updating OrderItem: ProductID={orderItem.ProductID}, Name={productDTO.ProductName}, Category={productDTO.Category}");

            // Atualiza com `
            // ` (já que OrderItemResponse é um record)
            addedOrderResponse.OrderItems[i] = orderItem with
            {
                ProductName = productDTO.ProductName ?? string.Empty,
                Category = productDTO.Category ?? string.Empty
            };
        }
        //TO DO: Load PersonName and Email from UsersMicroservice
        if (addedOrderResponse != null)
        {
            _mapper.Map<UserDTO, OrderResponse>(user, addedOrderResponse);
        }
        
        return addedOrderResponse;
    }


    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        //Check for null parameter
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }


        //Validate OrderAddRequest using Fluent Validations
        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO> products = new List<ProductDTO>();

        //Validate order items using Fluent Validation
        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }


            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            products.Add(product);
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }


        //Convert data from OrderUpdateRequest to Order
        Order orderInput = _mapper.Map<Order>(orderUpdateRequest); //Map OrderUpdateRequest to 'Order' type (it invokes OrderUpdateRequestToOrderMappingProfile class)

        //Generate values
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        orderInput.Totalbill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);


        //Invoke repository
        Order? updatedOrder = await _ordersRepository.UpdateOrder(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder); //Map updatedOrder ('Order' type) into 'OrderResponse' type (it invokes OrderToOrderResponseMappingProfile).


        //TO DO: Load ProductName and Category in OrderItem
        if (updatedOrderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(temp => temp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }
        //TO DO: Load PersonName and Email from UsersMicroservice

        if (updatedOrderResponse != null)
        {
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, updatedOrderResponse);
            }
        }



        return updatedOrderResponse;
    }


    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
        {
            return false;
        }


        bool isDeleted = await _ordersRepository.DeleteOrder(orderID);
        return isDeleted;
    }


    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);
        if (order == null)
            return null;

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);


        //TO DO: Load ProductName and Category in OrderItem
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
        }
        // TO DO: Load PersonName and Eamil from UsersMicroservice
        if (orderResponse != null)
        {
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }
        
    
        return orderResponse;
    }


    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrdersByCondition(filter);


        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);


        //TO DO: Load ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }


            // TO DO: Load PersonName and Eamil from UsersMicroservice
            
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }
    

        return orderResponses.ToList();
    }


    public async Task<List<OrderResponse?>> GetOrders(Guid? userID = null)
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();

        if (userID.HasValue)
        {
            orders = orders.Where(o => o != null && o.UserID == userID);
        }

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);


        //TO DO: Load ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
            }
            // TO DO: Load PersonName and Eamil from UsersMicroservice
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        
        return orderResponses.ToList();
    }
}
