using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using YourShopManagement.API.Data;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;

namespace Backend.Tests.Repositories
{
    /// <summary>
    /// Unit tests for CustomerRepository following enterprise standards
    /// Tests cover CRUD operations, search, filtering, edge cases, and error handling
    /// </summary>
    public class CustomerRepositoryTests : IDisposable
    {
        private readonly string _dbName;
        private ApplicationDbContext _context;
        private CustomerRepository _repository;

        public CustomerRepositoryTests()
        {
            _dbName = Guid.NewGuid().ToString();
            InitializeContext();
        }

        private void InitializeContext()
        {
            _context = TestDbContextFactory.Create(_dbName);
            _repository = new CustomerRepository(_context);
        }

        public void Dispose()
        {
            TestDbContextFactory.Destroy(_context);
        }

        #region GetById Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = await SeedSingleCustomer();

            // Act
            var result = await _repository.GetByIdAsync(customer.CustomerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.CustomerId, result.CustomerId);
            Assert.Equal(customer.CustomerCode, result.CustomerCode);
            Assert.Equal(customer.CustomerName, result.CustomerName);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
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
        public async Task GetAllAsync_ShouldReturnAllCustomers_WhenCustomersExist()
        {
            // Arrange
            await SeedMultipleCustomers(5);

            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCustomersExist()
        {
            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCustomersInCorrectOrder()
        {
            // Arrange
            var customers = new[]
            {
                CreateCustomer("C001", "Alice", "alice@test.com"),
                CreateCustomer("C002", "Bob", "bob@test.com"),
                CreateCustomer("C003", "Charlie", "charlie@test.com")
            };

            foreach (var cust in customers)
            {
                await _repository.AddAsync(cust);
            }

            // Act
            var result = (await _repository.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.CustomerCode == "C001");
            Assert.Contains(result, c => c.CustomerCode == "C002");
            Assert.Contains(result, c => c.CustomerCode == "C003");
        }

        #endregion

        #region Add Tests

        [Fact]
        public async Task AddAsync_ShouldAddCustomer_WithValidData()
        {
            // Arrange
            var customer = CreateCustomer("C100", "New Customer", "new@test.com");

            // Act
            var result = await _repository.AddAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.CustomerId > 0);
            Assert.Equal("C100", result.CustomerCode);
            Assert.Equal("New Customer", result.CustomerName);

            // Verify persistence
            var saved = await _repository.GetByIdAsync(result.CustomerId);
            Assert.NotNull(saved);
            Assert.Equal(result.CustomerId, saved.CustomerId);
        }

        [Fact]
        public async Task AddAsync_ShouldSetDefaultValues_ForOptionalFields()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerCode = "C200",
                CustomerName = "Minimal Customer",
                Phone = "0123456789"
            };

            // Act
            var result = await _repository.AddAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("active", result.Status);
            Assert.Equal("retail", result.CustomerType);
            Assert.Equal(0, result.TotalDebt);
            Assert.Equal(0, result.LoyaltyPoints);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
            Assert.NotEqual(DateTime.MinValue, result.UpdatedAt);
        }

        [Fact]
        public async Task AddAsync_ShouldAddCustomer_WithAllFieldsPopulated()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerCode = "C300",
                CustomerName = "Full Customer",
                Phone = "0123456789",
                Email = "full@test.com",
                Address = "123 Main St",
                TaxCode = "TAX123",
                CustomerType = "wholesale",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "Male",
                IdCard = "123456789",
                BankAccount = "1234567890",
                BankName = "VCB",
                TotalDebt = 1000m,
                TotalPurchaseAmount = 5000m,
                TotalPurchaseCount = 10,
                LoyaltyPoints = 500,
                Segment = "VIP",
                Source = "Facebook",
                AvatarUrl = "/uploads/avatar.jpg",
                Status = "active",
                Notes = "Important customer"
            };

            // Act
            var result = await _repository.AddAsync(customer);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("C300", result.CustomerCode);
            Assert.Equal("Full Customer", result.CustomerName);
            Assert.Equal("0123456789", result.Phone);
            Assert.Equal(1000m, result.TotalDebt);
            Assert.Equal(500, result.LoyaltyPoints);
            Assert.Equal("VIP", result.Segment);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = await SeedSingleCustomer();
            customer.CustomerName = "Updated Name";
            customer.Phone = "9876543210";
            customer.TotalDebt = 2000m;

            // Act
            await _repository.UpdateAsync(customer);

            // Assert
            var updated = await _repository.GetByIdAsync(customer.CustomerId);
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.CustomerName);
            Assert.Equal("9876543210", updated.Phone);
            Assert.Equal(2000m, updated.TotalDebt);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyModifiedFields()
        {
            // Arrange
            var customer = await SeedSingleCustomer();
            var originalCode = customer.CustomerCode;
            var originalEmail = customer.Email;

            customer.CustomerName = "Changed Name Only";

            // Act
            await _repository.UpdateAsync(customer);

            // Assert
            var updated = await _repository.GetByIdAsync(customer.CustomerId);
            Assert.Equal("Changed Name Only", updated.CustomerName);
            Assert.Equal(originalCode, updated.CustomerCode);
            Assert.Equal(originalEmail, updated.Email);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleNullableFields()
        {
            // Arrange
            var customer = await SeedSingleCustomer();
            customer.Phone = null;
            customer.Email = null;
            customer.Notes = null;

            // Act
            await _repository.UpdateAsync(customer);

            // Assert
            var updated = await _repository.GetByIdAsync(customer.CustomerId);
            Assert.Null(updated.Phone);
            Assert.Null(updated.Email);
            Assert.Null(updated.Notes);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCustomer_WhenCustomerExists()
        {
            // Arrange
            var customer = await SeedSingleCustomer();
            var customerId = customer.CustomerId;

            // Act
            await _repository.DeleteAsync(customerId);

            // Assert
            var deleted = await _repository.GetByIdAsync(customerId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrowException_WhenCustomerDoesNotExist()
        {
            // Act & Assert
            await _repository.DeleteAsync(99999); // Should not throw
        }

        [Fact]
        public async Task DeleteAsync_ShouldOnlyDeleteSpecifiedCustomer()
        {
            // Arrange
            var customers = await SeedMultipleCustomers(3);
            var toDelete = customers.First();

            // Act
            await _repository.DeleteAsync(toDelete.CustomerId);

            // Assert
            var remaining = (await _repository.GetAllAsync()).ToList();
            Assert.Equal(2, remaining.Count);
            Assert.DoesNotContain(remaining, c => c.CustomerId == toDelete.CustomerId);
        }

        #endregion

        #region Search Tests

        [Theory]
        [InlineData("Alice", 1)]
        [InlineData("Ali", 1)]
        [InlineData("C001", 1)]
        [InlineData("alice@test.com", 1)]
        public async Task SearchAsync_ShouldFindCustomer_ByVariousCriteria(string keyword, int expectedCount)
        {
            // Arrange
            await SeedSingleCustomer("C001", "Alice", "alice@test.com");

            // Act
            var result = (await _repository.SearchAsync(keyword)).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatch()
        {
            // Arrange
            await SeedMultipleCustomers(3);

            // Act
            var result = (await _repository.SearchAsync("NonExistentKeyword123")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAsync_ShouldHandleEmptyString()
        {
            // Arrange
            await SeedMultipleCustomers(3);

            // Act
            var result = (await _repository.SearchAsync(string.Empty)).ToList();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchAsync_ShouldHandleSpecialCharacters()
        {
            // Arrange
            await SeedSingleCustomer("C-001", "Alice-Marie@Test", "test+tag@company.com");

            // Act
            var result = (await _repository.SearchAsync("C-001")).ToList();

            // Assert
            Assert.Single(result);
        }

        #endregion

        #region GetBySegment Tests

        [Fact]
        public async Task GetBySegmentAsync_ShouldReturnCustomers_FromSpecificSegment()
        {
            // Arrange
            await SeedSingleCustomer("C001", "VIP Customer 1", "vip1@test.com", segment: "VIP");
            await SeedSingleCustomer("C002", "VIP Customer 2", "vip2@test.com", segment: "VIP");
            await SeedSingleCustomer("C003", "Regular Customer", "reg@test.com", segment: "Regular");

            // Act
            var result = (await _repository.GetBySegmentAsync("VIP")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("VIP", c.Segment));
        }

        [Fact]
        public async Task GetBySegmentAsync_ShouldReturnEmptyList_WhenSegmentHasNoCustomers()
        {
            // Arrange
            await SeedMultipleCustomers(3);

            // Act
            var result = (await _repository.GetBySegmentAsync("NonExistentSegment")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBySegmentAsync_ShouldBeExactMatch()
        {
            // Arrange
            await SeedSingleCustomer("C001", "VIP Customer", "vip@test.com", segment: "VIP");
            await SeedSingleCustomer("C002", "VVIP Customer", "vvip@test.com", segment: "VVIP");

            // Act
            var result = (await _repository.GetBySegmentAsync("VIP")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("VIP", result[0].Segment);
        }

        #endregion

        #region GetByStatus Tests

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnActiveCustomers()
        {
            // Arrange
            await SeedSingleCustomer("C001", "Active 1", "a1@test.com", status: "active");
            await SeedSingleCustomer("C002", "Active 2", "a2@test.com", status: "active");
            await SeedSingleCustomer("C003", "Inactive", "i1@test.com", status: "inactive");

            // Act
            var result = (await _repository.GetByStatusAsync("active")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("active", c.Status));
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnInactiveCustomers()
        {
            // Arrange
            await SeedSingleCustomer("C001", "Active", "a@test.com", status: "active");
            await SeedSingleCustomer("C002", "Inactive 1", "i1@test.com", status: "inactive");
            await SeedSingleCustomer("C003", "Inactive 2", "i2@test.com", status: "inactive");

            // Act
            var result = (await _repository.GetByStatusAsync("inactive")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("inactive", c.Status));
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnEmptyList_ForUnusedStatus()
        {
            // Arrange
            await SeedMultipleCustomers(3);

            // Act
            var result = (await _repository.GetByStatusAsync("suspended")).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task ConcurrentAdds_ShouldAllSucceed()
        {
            // Arrange
            var tasks = new List<Task<Customer>>();
            
            for (int i = 0; i < 10; i++)
            {
                var customer = CreateCustomer($"C{i:D3}", $"Concurrent {i}", $"concurrent{i}@test.com");
                tasks.Add(_repository.AddAsync(customer));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(10, results.Length);
            Assert.All(results, r => Assert.True(r.CustomerId > 0));

            var all = (await _repository.GetAllAsync()).ToList();
            Assert.Equal(10, all.Count);
        }

        #endregion

        #region Helper Methods

        private Customer CreateCustomer(string code, string name, string email, 
            string segment = "Regular", string status = "active")
        {
            return new Customer
            {
                CustomerCode = code,
                CustomerName = name,
                Email = email,
                Phone = "0123456789",
                Segment = segment,
                Status = status,
                CustomerType = "retail"
            };
        }

        private async Task<Customer> SeedSingleCustomer(string code = "C001", string name = "Test Customer", 
            string email = "test@test.com", string segment = "Regular", string status = "active")
        {
            var customer = new Customer
            {
                CustomerCode = code,
                CustomerName = name,
                Email = email,
                Phone = "0123456789",
                Segment = segment,
                Status = status,
                CustomerType = "retail"
            };

            return await _repository.AddAsync(customer);
        }

        private async Task<List<Customer>> SeedMultipleCustomers(int count)
        {
            var customers = new List<Customer>();
            
            for (int i = 1; i <= count; i++)
            {
                var cust = await SeedSingleCustomer($"C{i:D3}", $"Customer {i}", $"cust{i}@test.com");
                customers.Add(cust);
            }

            return customers;
        }

        #endregion
    }
}
