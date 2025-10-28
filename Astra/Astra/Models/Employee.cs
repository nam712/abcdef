// Employee entity class for the employees table
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourShopManagement.API.Models;

namespace Backend.Models
{
    /// <summary>
    /// Bảng nhân viên
    /// Constraint: Nếu có username thì bắt buộc phải có password
    /// </summary>
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("employee_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [MaxLength(50)]
        [Column("employee_code")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên nhân viên không được để trống")]
        [MaxLength(255)]
        [Column("employee_name")]
        public string EmployeeName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        [Column("gender")]
        public string? Gender { get; set; }

        [MaxLength(20)]
        [Column("id_card")]
        public string? IdCard { get; set; }

        [MaxLength(100)]
        [Column("position")]
        public string? Position { get; set; }

        [MaxLength(100)]
        [Column("department")]
        public string? Department { get; set; }

        [Required]
        [Column("hire_date")]
        public DateTime HireDate { get; set; }

        [Column("salary", TypeName = "decimal(18,2)")]
        public decimal? Salary { get; set; }

        [MaxLength(20)]
        [Column("salary_type")]
        public string? SalaryType { get; set; }

        [MaxLength(100)]
        [Column("bank_account")]
        public string? BankAccount { get; set; }

        [MaxLength(255)]
        [Column("bank_name")]
        public string? BankName { get; set; }

        [MaxLength(100)]
        [Column("username")]
        public string? Username { get; set; }

        [MaxLength(255)]
        [Column("password")]
        public string? Password { get; set; }

        [MaxLength(255)]
        [Column("permissions")]
        public string? Permissions { get; set; }

        [MaxLength(255)]
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("work_status")]
        public string WorkStatus { get; set; } = "active";

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Invoice>? Invoices { get; set; }
        /// <summary>
        /// Validation: Nếu có Username thì bắt buộc phải có Password
        /// </summary>
        public bool IsValid()
        {
            if (!string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password))
            {
                return false;
            }
            return true;
        }
    }


}
