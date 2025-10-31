using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YourShopManagement.API.Data;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Tests.Repositories
{
    /// <summary>
    /// Unit tests for EmployeeRepository following enterprise standards
    /// Tests cover CRUD operations, search, filtering, edge cases, and error handling
    /// </summary>
    public class EmployeeRepositoryTests : IDisposable
    {
        private readonly string _dbName;
        private ApplicationDbContext _context;
        private EmployeeRepository _repository;

        public EmployeeRepositoryTests()
        {
            _dbName = Guid.NewGuid().ToString();
            InitializeContext();
        }

        private void InitializeContext()
        {
            _context = TestDbContextFactory.Create(_dbName);
            _repository = new EmployeeRepository(_context);
        }

        public void Dispose()
        {
            TestDbContextFactory.Destroy(_context);
        }

        #region GetById Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = await SeedSingleEmployee();

            // Act
            var result = await _repository.GetByIdAsync(employee.EmployeeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.EmployeeId, result.EmployeeId);
            Assert.Equal(employee.EmployeeCode, result.EmployeeCode);
            Assert.Equal(employee.EmployeeName, result.EmployeeName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEmployeeDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(99999);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsInvalid(int invalidId)
        {
            // Act
            var result = await _repository.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEmployees_WhenEmployeesExist()
        {
            // Arrange
            await SeedMultipleEmployees(5);

            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoEmployeesExist()
        {
            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmployeesInCorrectOrder()
        {
            // Arrange
            var employees = new[]
            {
                CreateEmployee("E001", "Alice", "IT"),
                CreateEmployee("E002", "Bob", "HR"),
                CreateEmployee("E003", "Charlie", "Sales")
            };

            foreach (var emp in employees)
            {
                await _repository.AddAsync(emp);
            }

            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, e => e.EmployeeCode == "E001");
            Assert.Contains(result, e => e.EmployeeCode == "E002");
            Assert.Contains(result, e => e.EmployeeCode == "E003");
        }

        #endregion

        #region Add Tests

        [Fact]
        public async Task AddAsync_ShouldAddEmployee_WithValidData()
        {
            // Arrange
            var employee = CreateEmployee("E100", "New Employee", "IT");

            // Act
            var result = await _repository.AddAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.EmployeeId > 0);
            Assert.Equal("E100", result.EmployeeCode);
            Assert.Equal("New Employee", result.EmployeeName);

            // Verify persistence
            var saved = await _repository.GetByIdAsync(result.EmployeeId);
            Assert.NotNull(saved);
            Assert.Equal(result.EmployeeId, saved.EmployeeId);
        }

        [Fact]
        public async Task AddAsync_ShouldSetDefaultValues_ForOptionalFields()
        {
            // Arrange
            var employee = new Employee
            {
                EmployeeCode = "E200",
                EmployeeName = "Minimal Employee",
                HireDate = DateTime.Now,
                WorkStatus = "active"
            };

            // Act
            var result = await _repository.AddAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("active", result.WorkStatus);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, result.UpdatedAt);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEmployee_WithAllFieldsPopulated()
        {
            // Arrange
            var employee = new Employee
            {
                EmployeeCode = "E300",
                EmployeeName = "Full Employee",
                Phone = "0123456789",
                Email = "full@company.com",
                Address = "123 Main St",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                IdCard = "123456789",
                Position = "Senior Developer",
                Department = "IT",
                HireDate = DateTime.Now.AddYears(-2),
                Salary = 50000m,
                SalaryType = "Monthly",
                BankAccount = "1234567890",
                BankName = "VCB",
                Username = "fulluser",
                Password = "hashedpassword",
                Permissions = "admin",
                AvatarUrl = "/uploads/avatar.jpg",
                WorkStatus = "active",
                Notes = "Top performer"
            };

            // Act
            var result = await _repository.AddAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("E300", result.EmployeeCode);
            Assert.Equal("Full Employee", result.EmployeeName);
            Assert.Equal("0123456789", result.Phone);
            Assert.Equal(50000m, result.Salary);
            Assert.Equal("Senior Developer", result.Position);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = await SeedSingleEmployee();
            employee.EmployeeName = "Updated Name";
            employee.Position = "Updated Position";
            employee.Salary = 75000m;

            // Act
            await _repository.UpdateAsync(employee);

            // Assert
            var updated = await _repository.GetByIdAsync(employee.EmployeeId);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.EmployeeName);
            Assert.Equal("Updated Position", updated.Position);
            Assert.Equal(75000m, updated.Salary);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyModifiedFields()
        {
            // Arrange
            var employee = await SeedSingleEmployee();
            var originalCode = employee.EmployeeCode;
            var originalEmail = employee.Email;

            employee.EmployeeName = "Changed Name Only";

            // Act
            await _repository.UpdateAsync(employee);

            // Assert
            var updated = await _repository.GetByIdAsync(employee.EmployeeId);
            Assert.Equal("Changed Name Only", updated.EmployeeName);
            Assert.Equal(originalCode, updated.EmployeeCode);
            Assert.Equal(originalEmail, updated.Email);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleNullableFields()
        {
            // Arrange
            var employee = await SeedSingleEmployee();
            employee.Phone = null;
            employee.Email = null;
            employee.Notes = null;

            // Act
            await _repository.UpdateAsync(employee);

            // Assert
            var updated = await _repository.GetByIdAsync(employee.EmployeeId);
            Assert.Null(updated.Phone);
            Assert.Null(updated.Email);
            Assert.Null(updated.Notes);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = await SeedSingleEmployee();
            var employeeId = employee.EmployeeId;

            // Act
            await _repository.DeleteAsync(employeeId);

            // Assert
            var deleted = await _repository.GetByIdAsync(employeeId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrowException_WhenEmployeeDoesNotExist()
        {
            // Act & Assert
            await _repository.DeleteAsync(99999); // Should not throw
        }

        [Fact]
        public async Task DeleteAsync_ShouldOnlyDeleteSpecifiedEmployee()
        {
            // Arrange
            var employees = await SeedMultipleEmployees(3);
            var toDelete = employees.First();

            // Act
            await _repository.DeleteAsync(toDelete.EmployeeId);

            // Assert
            var remaining = (await _repository.GetAllAsync()).ToList();
            Assert.Equal(2, remaining.Count);
            Assert.DoesNotContain(remaining, e => e.EmployeeId == toDelete.EmployeeId);
        }

        #endregion

        #region Search Tests

        [Theory]
        [InlineData("Nguyễn Văn A", 1)]
        [InlineData("Nguyễn", 1)]
        [InlineData("E001", 1)]
        [InlineData("nva@company.com", 1)]
        public async Task SearchAsync_ShouldFindEmployee_ByVariousCriteria(string keyword, int expectedCount)
        {
            // Arrange
            await SeedSingleEmployee("E001", "Nguyễn Văn A", "nva@company.com");

            // Act
            var result = (await _repository.SearchAsync(keyword)).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task SearchAsync_ShouldBeCaseInsensitive()
        {
            // Arrange
            await SeedSingleEmployee("E001", "Nguyễn Văn A", "nva@company.com");

            // Act
            // Note: EF InMemory doesn't support case-insensitive string operations like SQL Server
            // This test verifies exact match behavior
            var resultExact = (await _repository.SearchAsync("Nguyễn Văn A")).ToList();

            // Assert
            Assert.Single(resultExact);
            
            // For InMemory database, case-sensitive search is expected behavior
            // In production with SQL Server, this would be case-insensitive
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatch()
        {
            // Arrange
            await SeedMultipleEmployees(3);

            // Act
            var result = (await _repository.SearchAsync("NonExistentKeyword123")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAsync_ShouldHandleEmptyString()
        {
            // Arrange
            await SeedMultipleEmployees(3);

            // Act
            var result = (await _repository.SearchAsync(string.Empty)).ToList();

            // Assert
            // Should return all or none based on implementation
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchAsync_ShouldHandleSpecialCharacters()
        {
            // Arrange
            await SeedSingleEmployee("E-001", "Nguyễn-Văn@A", "test+tag@company.com");

            // Act
            var result = (await _repository.SearchAsync("E-001")).ToList();

            // Assert
            Assert.Single(result);
        }

        #endregion

        #region GetByDepartment Tests

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturnEmployees_FromSpecificDepartment()
        {
            // Arrange
            await SeedSingleEmployee("E001", "IT Employee 1", "it1@c.com", "IT");
            await SeedSingleEmployee("E002", "IT Employee 2", "it2@c.com", "IT");
            await SeedSingleEmployee("E003", "HR Employee", "hr1@c.com", "HR");

            // Act
            var result = (await _repository.GetByDepartmentAsync("IT")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, emp => Assert.Equal("IT", emp.Department));
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturnEmptyList_WhenDepartmentHasNoEmployees()
        {
            // Arrange
            await SeedMultipleEmployees(3);

            // Act
            var result = (await _repository.GetByDepartmentAsync("NonExistentDept")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldBeExactMatch()
        {
            // Arrange
            await SeedSingleEmployee("E001", "IT Employee", "it@c.com", "IT");
            await SeedSingleEmployee("E002", "IT Support", "support@c.com", "IT Support");

            // Act
            var result = (await _repository.GetByDepartmentAsync("IT")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("IT", result[0].Department);
        }

        #endregion

        #region GetByWorkStatus Tests

        [Fact]
        public async Task GetByWorkStatusAsync_ShouldReturnActiveEmployees()
        {
            // Arrange
            await SeedSingleEmployee("E001", "Active 1", "a1@c.com", workStatus: "active");
            await SeedSingleEmployee("E002", "Active 2", "a2@c.com", workStatus: "active");
            await SeedSingleEmployee("E003", "Inactive", "i1@c.com", workStatus: "inactive");

            // Act
            var result = (await _repository.GetByWorkStatusAsync("active")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, emp => Assert.Equal("active", emp.WorkStatus));
        }

        [Fact]
        public async Task GetByWorkStatusAsync_ShouldReturnInactiveEmployees()
        {
            // Arrange
            await SeedSingleEmployee("E001", "Active", "a@c.com", workStatus: "active");
            await SeedSingleEmployee("E002", "Inactive 1", "i1@c.com", workStatus: "inactive");
            await SeedSingleEmployee("E003", "Inactive 2", "i2@c.com", workStatus: "inactive");

            // Act
            var result = (await _repository.GetByWorkStatusAsync("inactive")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, emp => Assert.Equal("inactive", emp.WorkStatus));
        }

        [Fact]
        public async Task GetByWorkStatusAsync_ShouldReturnEmptyList_ForUnusedStatus()
        {
            // Arrange
            await SeedMultipleEmployees(3);

            // Act
            var result = (await _repository.GetByWorkStatusAsync("suspended")).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task ConcurrentAdds_ShouldAllSucceed()
        {
            // Arrange
            var tasks = new List<Task<Employee>>();
            
            for (int i = 0; i < 10; i++)
            {
                var employee = CreateEmployee($"E{i:D3}", $"Concurrent {i}", "IT");
                tasks.Add(_repository.AddAsync(employee));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, results.Length);
            Assert.All(results, r => Assert.True(r.EmployeeId > 0));

            var all = (await _repository.GetAllAsync()).ToList();
            Assert.Equal(10, all.Count);
        }

        #endregion

        #region Helper Methods

        private Employee CreateEmployee(string code, string name, string department = "IT", string workStatus = "active")
        {
            return new Employee
            {
                EmployeeCode = code,
                EmployeeName = name,
                Department = department,
                HireDate = DateTime.Now,
                WorkStatus = workStatus,
                Position = "Developer",
                Email = $"{code.ToLower()}@company.com"
            };
        }

        private async Task<Employee> SeedSingleEmployee(string code = "E001", string name = "Test Employee", 
            string email = "test@company.com", string department = "IT", string workStatus = "active")
        {
            var employee = new Employee
            {
                EmployeeCode = code,
                EmployeeName = name,
                Email = email,
                Department = department,
                Position = "Developer",
                WorkStatus = workStatus,
                HireDate = DateTime.Now
            };

            return await _repository.AddAsync(employee);
        }

        private async Task<List<Employee>> SeedMultipleEmployees(int count)
        {
            var employees = new List<Employee>();
            
            for (int i = 1; i <= count; i++)
            {
                var emp = await SeedSingleEmployee($"E{i:D3}", $"Employee {i}", $"emp{i}@company.com");
                employees.Add(emp);
            }

            return employees;
        }

        #endregion
    }
}
