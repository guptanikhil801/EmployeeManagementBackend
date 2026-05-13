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
         Ok(await db.Employees.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await db.Employees
                .FirstOrDefaultAsync(e => e.Id == id);
            return employee is null ? NotFound() : Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            db.Employees.Add(employee);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
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
    }
}
