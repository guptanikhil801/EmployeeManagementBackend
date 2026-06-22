using EmployeeManagement.Data;
using EmployeeManagement.DTOs;
using EmployeeManagement.Extensions;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController(AppDBContext db) : ControllerBase
    {

        [HttpGet("{employeeId}/getall-orders")]
        public async Task<IActionResult> GetEmployeeOrders(int employeeId)
        {
            var exists = await db.Employees.AnyAsync(e => e.Id == employeeId);
            if (!exists) return NotFound("Employee not found.");

            var orders = await db.Orders
                .AsNoTracking()
                .Where(o => o.EmployeeId == employeeId)
                .Select(o => new OrderResponse(
                    o.Id,
                    o.ProductName,
                    o.Quantity,
                    o.TotalAmount,
                    o.Status,
                    o.OrderDate,
                    o.Employee.Name,
                    o.ShippingAddress.City))
                .ToListAsync();

            return Ok(orders);
        }

        // GET employee with all their orders
        [HttpGet("{id}/employee-details-with-orders")]
        public async Task<IActionResult> GetWithOrders(int id)
        {
            var employee = await db.Employees
                .AsNoTracking()
                .Include(e => e.Orders)             // JOIN to Orders table
                .Where(e => e.Id == id)
                .Select(e => new EmployeeDetailResponse(
                    e.Id,
                    e.Name,
                    e.Email,
                    e.Department,
                    e.Salary,
                    e.HireDate,
                    e.IsActive,
                    e.Orders.Select(o => new OrderResponse(
                        o.Id,
                        o.ProductName,
                        o.Quantity,
                        o.TotalAmount,
                        o.Status,
                        o.OrderDate,
                        o.Employee.Name,
                        o.ShippingAddress.City)
                         ).ToList(),
                    e.Orders.Count,
                    e.Orders.Sum(x => x.TotalAmount))
                         )
                .FirstOrDefaultAsync();

            return employee is null ? NotFound() : Ok(employee);
        }

        // GET employee with profile
        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var result = await db.Employees
                .AsNoTracking()
                .Include(e => e.Profile)                // JOIN to EmployeeProfiles
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Email,
                    Profile = e.Profile == null ? null : new
                    {
                        e.Profile.Phone,
                        e.Profile.Bio,
                        e.Profile.PhotoUrl,
                        e.Profile.LinkedInUrl
                    }
                })
                .FirstOrDefaultAsync();

            return result is null ? NotFound() : Ok(result);
        }

        // POST — create or update profile
        [HttpPost("{employeeId}/profile")]
        public async Task<IActionResult> UpsertProfile(int employeeId, EmployeeProfileDTO req)
        {
            var employee = await db.Employees
                .Include(e => e.Profile)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee is null) return NotFound();

            if (employee.Profile is null)
            {
                // create — first time
                employee.Profile = new EmployeeProfile
                {
                    EmployeeId = employeeId,
                    Phone = req.Phone,
                    Bio = req.Bio,
                    PhotoUrl = req.PhotoUrl,
                    LinkedInUrl = req.LinkedInUrl
                };
            }
            else
            {
                // update — already exists
                employee.Profile.Phone = req.Phone;
                employee.Profile.Bio = req.Bio;
                employee.Profile.PhotoUrl = req.PhotoUrl;
                employee.Profile.LinkedInUrl = req.LinkedInUrl;
            }

            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeRequest employee)
        {
            db.Employees.Add(employee.ToEmployee());
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateEmployeeRequest updatedEmployee)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee is null) return NotFound();
            db.Employees.Update(updatedEmployee.ToEmployee(id, employee.Orders));
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            db.Employees.Remove(employee);
            await db.SaveChangesAsync();
            return NoContent();
        }

        #region Private Methods
        private async Task<List<Employee>> GetEmployeesByWithOrder()
        {
            return await db.Employees.GroupJoin(
                db.Orders,
                employee => employee.Id,
                orders => orders.EmployeeId,
             (employee, customerOrder) => new Employee
             {
                 Id = employee.Id,
                 Email = employee.Email,
                 Name = employee.Name,
                 //CreatedAt = employee.CreatedAt,
                 Orders = customerOrder.ToList()
             }).ToListAsync();
        }

        private async Task<Employee> GetEmployeeByIDWithOrder(int empID)
        {
            return await db.Employees
                .Where(x => x.Id == empID)
                .GroupJoin(db.Orders,
                employee => employee.Id,
                orders => orders.EmployeeId,
                    (employee, orders) => new Employee
                    {
                        Id = employee.Id,
                        Email = employee.Email ?? string.Empty,
                     //   CreatedAt = employee.CreatedAt,
                        Name = employee.Name ?? string.Empty,
                        Orders = orders != null ? orders.ToList() : new List<Order>()
                    })
                .FirstOrDefaultAsync() ?? new Employee();
        }
        #endregion
    }
}
