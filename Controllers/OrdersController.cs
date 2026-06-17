using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace orderManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(AppDBContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
        Ok(await db.Orders.ToListAsync<Order>());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await db.Orders
                .FirstOrDefaultAsync(e => e.Id == id);
            return order is null ? NotFound() : Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            db.Orders.Add(order);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Order updated)
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null) return NotFound();

            order.Total = updated.Total;
            order.Status = updated.Status;
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null) return NotFound();

            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
