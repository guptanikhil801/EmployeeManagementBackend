using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string? Department { get; set; } 
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;          // soft-delete flag

        // Navigation property (1-to-many)
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public EmployeeProfile? Profile { get; set; }
    }

    public class EmployeeProfile
    {
        public int Id { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LinkedInUrl { get; set; }

        // FK — profile owns the FK, not Employee
        public int EmployeeId { get; set; }

        // Navigation back
        public Employee Employee { get; set; } = null!;
    }
}
