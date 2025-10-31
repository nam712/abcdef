using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using YourShopManagement.API.Mappings;
using YourShopManagement.API.Repositories;
using YourShopManagement.API.Services;
using YourShopManagement.API.Models;
using YourShopManagement.API.DTOs;

namespace Backend.Tests.Services
{
    /// <summary>
    /// Comprehensive unit tests for CustomerService following enterprise standards
    /// Tests cover all service methods, validation, error handling, and mapping
    /// </summary>
    public class CustomerServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<ICustomerRepository> _mockRepository;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CustomerMappingProfile>());
            _mapper = config.CreateMapper();
            _mockRepository = new Mock<ICustomerRepository>();
            _service = new CustomerService(_mockRepository.Object, _mapper);
        }

        #region GetById Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenCustomerExists()
        {
            // Arrange
            var customer = CreateCustomer(1, "C001", "John Doe");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.CustomerId);
            Assert.Equal("C001", result.CustomerCode);
            Assert.Equal("John Doe", result.CustomerName);
            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Customer)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task GetByIdAsync_ShouldReturnNull_ForInvalidIds(int invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(invalidId)).ReturnsAsync((Customer)null);

            // Act
            var result = await _service.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtoList_WhenCustomersExist()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "C001", "Alice"),
                CreateCustomer(2, "C002", "Bob"),
                CreateCustomer(3, "C003", "Charlie")
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, c => c.CustomerCode == "C001");
            Assert.Contains(result, c => c.CustomerCode == "C002");
            Assert.Contains(result, c => c.CustomerCode == "C003");
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCustomersExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer>());

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Add Tests

        [Fact]
        public async Task AddAsync_ShouldCreateAndReturnMappedDto()
        {
            // Arrange
            var dto = CreateCustomerDto("C100", "New Customer");
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => { c.CustomerId = 10; return c; });

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.CustomerId);
            Assert.Equal("C100", result.CustomerCode);
            Assert.Equal("New Customer", result.CustomerName);
            _mockRepository.Verify(r => r.AddAsync(It.Is<Customer>(c => 
                c.CustomerCode == "C100" && 
                c.CustomerName == "New Customer")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldMapAllProperties()
        {
            // Arrange
            var dto = new CustomerDto
            {
                CustomerCode = "C200",
                CustomerName = "Full Customer",
                Phone = "0123456789",
                Email = "full@test.com",
                Address = "123 Main St",
                TaxCode = "TAX123",
                Segment = "VIP",
                Status = "active"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .ReturnsAsync((Customer c) => { c.CustomerId = 20; return c; });

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal("C200", result.CustomerCode);
            Assert.Equal("Full Customer", result.CustomerName);
            Assert.Equal("0123456789", result.Phone);
            Assert.Equal("full@test.com", result.Email);
            Assert.Equal("VIP", result.Segment);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateAsync_ShouldMapAndCallRepository()
        {
            // Arrange
            var dto = CreateCustomerDto("C300", "Updated Customer");
            dto.CustomerId = 30;

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(dto);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Customer>(c =>
                c.CustomerId == 30 &&
                c.CustomerCode == "C300" &&
                c.CustomerName == "Updated Customer")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllModifiedFields()
        {
            // Arrange
            var dto = new CustomerDto
            {
                CustomerId = 40,
                CustomerCode = "C400",
                CustomerName = "Modified",
                Phone = "9876543210",
                Segment = "VVIP",
                Status = "active"
            };

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(dto);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Customer>(c =>
                c.CustomerId == 40 &&
                c.Phone == "9876543210" &&
                c.Segment == "VVIP")), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCustomer_WhenCustomerExists()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(50)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(50);

            // Assert
            _mockRepository.Verify(r => r.DeleteAsync(50), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenCustomerDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(999)).Returns(Task.CompletedTask);

            // Act & Assert
            await _service.DeleteAsync(999); // Should not throw
            _mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
        }

        #endregion

        #region Search Tests

        [Fact]
        public async Task SearchAsync_ShouldReturnMappedResults()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "C001", "Search Result 1"),
                CreateCustomer(2, "C002", "Search Result 2")
            };
            _mockRepository.Setup(r => r.SearchAsync("Search")).ReturnsAsync(customers);

            // Act
            var result = (await _service.SearchAsync("Search")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Contains("Search Result", c.CustomerName));
            _mockRepository.Verify(r => r.SearchAsync("Search"), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            // Arrange
            _mockRepository.Setup(r => r.SearchAsync("NoMatch")).ReturnsAsync(new List<Customer>());

            // Act
            var result = (await _service.SearchAsync("NoMatch")).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SearchAsync_ShouldHandleEmptyKeyword(string keyword)
        {
            // Arrange
            _mockRepository.Setup(r => r.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Customer>());

            // Act
            var result = await _service.SearchAsync(keyword);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetBySegment Tests

        [Fact]
        public async Task GetBySegmentAsync_ShouldReturnCustomersFromSegment()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "C001", "VIP Customer 1", "VIP"),
                CreateCustomer(2, "C002", "VIP Customer 2", "VIP")
            };
            _mockRepository.Setup(r => r.GetBySegmentAsync("VIP")).ReturnsAsync(customers);

            // Act
            var result = (await _service.GetBySegmentAsync("VIP")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("VIP", c.Segment));
            _mockRepository.Verify(r => r.GetBySegmentAsync("VIP"), Times.Once);
        }

        [Fact]
        public async Task GetBySegmentAsync_ShouldReturnEmpty_ForNonExistentSegment()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetBySegmentAsync("NonExistent"))
                .ReturnsAsync(new List<Customer>());

            // Act
            var result = (await _service.GetBySegmentAsync("NonExistent")).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByStatus Tests

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnActiveCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "C001", "Active 1", status: "active"),
                CreateCustomer(2, "C002", "Active 2", status: "active")
            };
            _mockRepository.Setup(r => r.GetByStatusAsync("active")).ReturnsAsync(customers);

            // Act
            var result = (await _service.GetByStatusAsync("active")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal("active", c.Status));
        }

        [Fact]
        public async Task GetByStatusAsync_ShouldReturnInactiveCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                CreateCustomer(1, "C001", "Inactive", status: "inactive")
            };
            _mockRepository.Setup(r => r.GetByStatusAsync("inactive")).ReturnsAsync(customers);

            // Act
            var result = (await _service.GetByStatusAsync("inactive")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("inactive", result[0].Status);
        }

        #endregion

        #region Helper Methods

        private Customer CreateCustomer(int id, string code, string name, 
            string segment = "Regular", string status = "active")
        {
            return new Customer
            {
                CustomerId = id,
                CustomerCode = code,
                CustomerName = name,
                Phone = "0123456789",
                Email = $"{code.ToLower()}@test.com",
                Segment = segment,
                Status = status,
                CustomerType = "retail",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private CustomerDto CreateCustomerDto(string code, string name)
        {
            return new CustomerDto
            {
                CustomerCode = code,
                CustomerName = name,
                Phone = "0123456789",
                Status = "active"
            };
        }

        #endregion
    }
}