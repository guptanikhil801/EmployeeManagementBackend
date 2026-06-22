using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EmployeeManagement.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }                 // FK — EF convention: {NavProp}Id

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Owned entity — no separate table by default
        public Address ShippingAddress { get; set; } = new();

        // Shadow property (set by EF, not here) — "CreatedAt"

        // Navigation back to employee
        public Employee Employee { get; set; } = null!;

        // Navigation — many-to-many to Product
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
