using eCommerce.OrdersMicroservice.BusinessLogicLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrderMicroservice.API.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        //Get: api/Orders

    
    [HttpGet]
        public async Task<IEnumerable<OrderResponse?>> GetOrders() 
        {
            //retrive userID role tokenJWT
           var userIDClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID" || c.Type == "sub")?.Value;
           var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            Guid? userID = null;
            if (userIDClaim != null && Guid.TryParse(userIDClaim, out Guid parsedUserId))
            {
                userID = parsedUserId;
            }
            bool isAdmin = roleClaim != null && roleClaim.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            
            if (!isAdmin && userID != null)
            {
                FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserID, userID);
                List<OrderResponse?> filteredOrders = await _ordersService.GetOrdersByCondition(filter);
            
                return filteredOrders;
            }

            List<OrderResponse?> orders = await _ordersService.GetOrders();
            return orders;
        }

        //GET: api/Orders/search/orderid/{orderID}
        [HttpGet("search/orderid/{orderID}")]

        public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID) 
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);

            OrderResponse? order = await _ordersService.GetOrderByCondition(filter);
            return order;
            
        }

        //GET: api/Orders/search/productid/{productID}
        [HttpGet("search/productid/{productID}")]

        public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems, Builders< OrderItem>.Filter.Eq(tempProduct=>tempProduct.ProductID, productID));

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;

        }

        //GET: api/Orders/search/userid/{userID}
        [HttpGet("search/userid/{userid}")]

        public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserID(Guid userid)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserID, userid );

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;

        }


        //GET: api/Orders/search/orderDate/{orderDate}
        [HttpGet("search/orderdate/{orderDate}")]

        public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));

            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return orders;

        }

        //POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> Post(OrderAddRequest orderAddRequest)
        {
            if (orderAddRequest == null)
            {
                return BadRequest("Order data is null");
            }

            OrderResponse? orderResponse = await _ordersService.AddOrder(orderAddRequest);
            if (orderResponse == null)
            {
                return Problem("An error occurred while adding the order.");
            }

            return CreatedAtAction(nameof(GetOrderByOrderID), new { orderID = orderResponse.OrderID }, orderResponse);
        }
        //PUT: api/Orders/{orderID}
        
        [HttpPut("{orderID}")]
        public async Task<IActionResult> Put(Guid orderID, OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                return BadRequest("Order data is null");
            }

            if (orderUpdateRequest.OrderID != Guid.Empty && orderUpdateRequest.OrderID != orderID)
            {
                return BadRequest("Order ID in the URL does not match Order ID in the request body.");
            }
            orderUpdateRequest = orderUpdateRequest with { OrderID = orderID }; // Ensure the OrderID in the request matches the route parameter
            

            OrderResponse? orderResponse = await _ordersService.UpdateOrder(orderUpdateRequest);
            if (orderResponse == null)
            {
                return NotFound($"Order with ID {orderID} not found.");
            }

            return Ok(orderResponse);
        }
        //DELETE api/Orders/{ordersID}
        [HttpDelete("{orderID}")]
        public async Task<IActionResult> Delete(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                return BadRequest("Invalid order ID");
            }
            bool isDeleted = await _ordersService.DeleteOrder(orderID);
            if (!isDeleted) 
            {
                return NotFound($"Order with ID {orderID} was not found.");
            }
            return Ok(isDeleted);
        }





    }
}
