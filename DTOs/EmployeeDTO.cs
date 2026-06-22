using EmployeeManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.DTOs
{
    // ──────────────────────────────────────────────────────────────
    // CREATE — what the client sends when creating a new employee
    // Id, HireDate, IsActive are NOT here — server controls those
    // ──────────────────────────────────────────────────────────────
    public record CreateEmployeeRequest(
        [Required]
        [MaxLength(100)]
    string Name,

        [Required]
        [EmailAddress]
        [MaxLength(200)]
    string Email,

        [MaxLength(50)]
    string? Department,

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive.")]
    decimal Salary
    );

    // ──────────────────────────────────────────────────────────────
    // UPDATE — full replace (PUT). All fields required.
    // ──────────────────────────────────────────────────────────────
    public record UpdateEmployeeRequest(
        [Required]
    [MaxLength(100)]
    string Name,

        [Required]
    [EmailAddress]
    [MaxLength(200)]
    string Email,

        [MaxLength(50)]
    string? Department,

        [Range(0, double.MaxValue)]
    decimal Salary
    );

    // ──────────────────────────────────────────────────────────────
    // PATCH — partial update (PATCH). All fields optional.
    // Only non-null fields will be applied.
    // ──────────────────────────────────────────────────────────────
    public record PatchEmployeeRequest(
        [MaxLength(100)]
    string? Name,

        [EmailAddress]
    [MaxLength(200)]
    string? Email,

        [MaxLength(50)]
    string? Department,

        [Range(0, double.MaxValue)]
    decimal? Salary
    );

    // ──────────────────────────────────────────────────────────────
    // RESPONSE — what the API returns. Never expose the entity directly.
    // Salary excluded — shown only in detail response (see below)
    // ──────────────────────────────────────────────────────────────
    public record EmployeeResponse(
        int Id,
        string Name,
        string Email,
        string? Department,
        DateTime HireDate,
        bool IsActive
    );

    // ──────────────────────────────────────────────────────────────
    // DETAIL RESPONSE — richer version for GET /employees/{id}
    // Includes salary and a summary of their orders
    // ──────────────────────────────────────────────────────────────
    public record EmployeeDetailResponse(
        int Id,
        string Name,
        string Email,
        string? Department,
        decimal Salary,
        DateTime HireDate,
        bool IsActive,
        List<OrderResponse> Orders,
        int TotalOrders,
        decimal TotalOrderValue
    );

    // ──────────────────────────────────────────────────────────────
    // LIST ITEM — lightweight version for GET /employees (list view)
    // No salary, no order details — just enough to show a table row
    // ──────────────────────────────────────────────────────────────
    public record EmployeeListItem(
        int Id,
        string Name,
        string? Department,
        bool IsActive
    );

    public record EmployeeProfileDTO
    {
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LinkedInUrl { get; set; }

    }
}
