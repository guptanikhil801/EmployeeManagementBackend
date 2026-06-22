using EmployeeManagement.DTOs;
using EmployeeManagement.Models;

namespace EmployeeManagement.Extensions
{
    public static class EmployeeExtensions
    {
        public static Employee ToEmployee(this CreateEmployeeRequest request)
        {
            return new Employee
            {
                Name = request.Name,
                Email = request.Email,
                Department = request.Department,
                Salary = request.Salary,
                HireDate = DateTime.UtcNow,
                IsActive = true
            };
        }

        public static Employee ToEmployee(this UpdateEmployeeRequest request, int id, ICollection<Order> orders = null)
        {
            return new Employee
            {
                Name = request.Name,
                Email = request.Email,
                Department = request.Department,
                Salary = request.Salary,
                HireDate = DateTime.UtcNow,
                IsActive = true,
                Id = id,
                Orders = orders
            };
        }

    }
}
