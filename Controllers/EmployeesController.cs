using EmployeeManagement.Data;
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
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
         Ok(await GetEmployeesByWithOrder());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById( int id)
        {
            var employee = await GetEmployeeByIDWithOrder(id);
            return employee is null ? NotFound() : Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            db.Employees.Add(employee);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = employee.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee updated)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee is null) return NotFound();

            employee.Name = updated.Name;
            employee.Email = updated.Email;
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
                 CreatedAt = employee.CreatedAt,
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
                        CreatedAt = employee.CreatedAt,
                        Name = employee.Name ?? string.Empty,
                        Orders = orders != null ? orders.ToList() : new List<Order>()
                    })
                .FirstOrDefaultAsync() ?? new Employee();
        }
        #endregion
    }
}
