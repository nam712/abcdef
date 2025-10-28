// Data Transfer Object for Employee
using System;
namespace Backend.DTOs
{

    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdCard { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }
        public string? SalaryType { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Permissions { get; set; }
        public string? AvatarUrl { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public string? WorkStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


}
