// Employee entity class for the employees table
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YourShopManagement.API.Models;

namespace Backend.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("employee_code")]
        [MaxLength(50)]
        public string EmployeeCode { get; set; }

        [Required]
        [Column("employee_name")]
        [MaxLength(255)]
        public string EmployeeName { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column("address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        [MaxLength(10)]
        public string Gender { get; set; }

        [Column("id_card")]
        [MaxLength(20)]
        public string IdCard { get; set; }

        [Column("position")]
        [MaxLength(100)]
        public string Position { get; set; }

        [Column("department")]
        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        [Column("hire_date")]
        public DateTime HireDate { get; set; }

        [Column("salary", TypeName = "decimal(18,2)")]
        public decimal? Salary { get; set; }

        [Column("salary_type")]
        [MaxLength(20)]
        public string SalaryType { get; set; }

        [Column("bank_account")]
        [MaxLength(100)]
        public string BankAccount { get; set; }

        [Column("bank_name")]
        [MaxLength(255)]
        public string BankName { get; set; }

        [Column("username")]
        [MaxLength(100)]
        public string Username { get; set; }

        [Column("password")]
        [MaxLength(255)]
        public string Password { get; set; }

        [Column("permissions")]
        [MaxLength(255)]
        public string Permissions { get; set; }

        [Column("avatar_url")]
        [MaxLength(255)]
        public string AvatarUrl { get; set; }

        [Required]
        [Column("work_status")]
        [MaxLength(20)]
        public string WorkStatus { get; set; } = "active";

        [Column("notes")]
        public string Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Invoice>? Invoices { get; set; }
    }


}
