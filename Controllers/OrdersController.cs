using EmployeeManagement.Data;
using EmployeeManagement.DTOs;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace orderManagement.Controllers
{
    [ApiController]
    [Route("api/orders/{employeeId}")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDBContext _db;

        public OrdersController(AppDBContext db) => _db = db;

        // GET all orders for an employee — with projection, pagination, no-tracking
        [HttpGet]
        public async Task<IActionResult> GetOrders(
            int employeeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var orders = await _db.Orders
                .AsNoTracking()                                  // read-only, no tracking overhead
                .Where(o => o.EmployeeId == employeeId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderResponse(                  // projection — only needed columns
                    o.Id,
                    o.ProductName,
                    o.Quantity,
                    o.TotalAmount,
                    o.Status,
                    o.OrderDate,
                    o.Employee.Name,                             // EF translates this to a JOIN
                    o.ShippingAddress.City
                ))
                .ToListAsync();

            return Ok(orders);
        }

        // GET single order with eager loading
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int employeeId, int orderId)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Employee)                        // eager load
                .FirstOrDefaultAsync(o => o.Id == orderId && o.EmployeeId == employeeId);

            if (order is null) return NotFound();

            return Ok(new OrderResponse(
                order.Id, order.ProductName, order.Quantity,
                order.TotalAmount, order.Status, order.OrderDate,
                order.Employee.Name, order.ShippingAddress.City));
        }

        // POST — create order (change tracking + SaveChanges)
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int employeeId, CreateOrderRequest req)
        {
            var employeeExists = await _db.Employees
                .AnyAsync(e => e.Id == employeeId);              // cheap existence check

            if (!employeeExists) return NotFound("Employee not found.");

            var order = new Order
            {
                EmployeeId = employeeId,
                ProductName = req.ProductName,
                Quantity = req.Quantity,
                TotalAmount = req.TotalAmount,
                ShippingAddress = new Address
                {
                    Street = req.Street,
                    City = req.City,
                    State = req.State,
                    PostalCode = req.PostalCode
                }
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder),
                new { employeeId, orderId = order.Id },
                new { order.Id });
        }

        // PATCH — update status
        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int employeeId, int orderId, [FromBody] string status)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.EmployeeId == employeeId);

            if (order is null) return NotFound();

            order.Status = status;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE (soft delete via Employee.IsActive filter)
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int employeeId, int orderId)
        {
            // EF 7+ ExecuteDelete — no entity load required (bulk op)
            var deleted = await _db.Orders
                .Where(o => o.Id == orderId && o.EmployeeId == employeeId)
                .ExecuteDeleteAsync();

            return deleted > 0 ? NoContent() : NotFound();
        }

        // GET order with all its products
        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetOrderProducts(int id)
        {
            var order = await _db.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.OrderDate,
                    Products = _db.Set<OrderProduct>()
                        .Where(op => op.OrderId == id)
                        .Select(op => new
                        {
                            op.Product.Id,
                            op.Product.Name,
                            op.Quantity,
                            op.UnitPrice,
                            LineTotal = op.Quantity * op.UnitPrice
                        })
                })
                .FirstOrDefaultAsync();

            return order is null ? NotFound() : Ok(order);
        }

        // POST — add a product to an order
        [HttpPost("{orderId}/products/{productId}")]
        public async Task<IActionResult> AddProductToOrder(
            int orderId, int productId, [FromBody] int quantity)
        {
            var order = await _db.Orders.FindAsync(orderId);
            var product = await _db.Products.FindAsync(productId);

            if (order is null || product is null) return NotFound();

            var alreadyAdded = await _db.Set<OrderProduct>()
                .AnyAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (alreadyAdded)
                return Conflict("Product already in this order.");

            _db.Set<OrderProduct>().Add(new OrderProduct
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price          // snapshot price at time of order
            });

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE — remove a product from an order
        [HttpDelete("{orderId}/products/{productId}")]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            var deleted = await _db.Set<OrderProduct>()
                .Where(op => op.OrderId == orderId && op.ProductId == productId)
                .ExecuteDeleteAsync();

            return deleted > 0 ? NoContent() : NotFound();
        }
    }
}
