using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Order
    {
        public int Id { get; set; }
        public decimal Total { get; set; }

        [AllowedValues("Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Returned")]
        public string Status { get; set; } = "Pending";
        public int EmployeeId { get; set; }        
    }                            
}
