using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    /// <summary>
    /// DTO cho Employee tự cập nhật thông tin
    /// - Hiển thị: TẤT CẢ thông tin (để employee xem)
    /// - Cho phép sửa: CHỈ các trường được phép
    /// </summary>
    public class EmployeeUpdateByEmployeeDto
    {
        [Required]
        public int EmployeeId { get; set; }

        // ❌ CHỈ HIỂN THỊ - KHÔNG ĐƯỢC SỬA
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }
        public string? SalaryType { get; set; }
        public string? Username { get; set; }
        public string? Permissions { get; set; }
        public string? WorkStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ CÁC TRƯỜNG EMPLOYEE ĐƯỢC PHÉP SỬA
        [MaxLength(20)]
        public string? Phone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? IdCard { get; set; }

        [MaxLength(100)]
        public string? BankAccount { get; set; }

        [MaxLength(255)]
        public string? BankName { get; set; }

        // Có thể đổi mật khẩu
        [MaxLength(255)]
        public string? Password { get; set; }

        // Avatar
        [MaxLength(255)]
        public string? AvatarUrl { get; set; }
        
        public IFormFile? AvatarFile { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
