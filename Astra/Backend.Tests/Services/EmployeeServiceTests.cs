using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Backend.Mappings;
using Backend.Repositories;
using Backend.Services;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Tests.Services
{
    /// <summary>
    /// Comprehensive unit tests for EmployeeService following enterprise standards
    /// Tests cover all service methods, validation, error handling, and file operations
    /// </summary>
    public class EmployeeServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IEmployeeRepository> _mockRepository;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly EmployeeService _service;

        public EmployeeServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>());
            _mapper = config.CreateMapper();
            
            _mockRepository = new Mock<IEmployeeRepository>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            
            // Setup default WebRootPath
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            
            _service = new EmployeeService(_mockRepository.Object, _mapper, _mockEnvironment.Object);
        }

        #region GetById Tests

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedDto_WhenEmployeeExists()
        {
            // Arrange
            var employee = CreateEmployee(1, "E001", "John Doe");
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(employee);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.EmployeeId);
            Assert.Equal("E001", result.EmployeeCode);
            Assert.Equal("John Doe", result.EmployeeName);
            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEmployeeDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee)null);

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
            _mockRepository.Setup(r => r.GetByIdAsync(invalidId)).ReturnsAsync((Employee)null);

            // Act
            var result = await _service.GetByIdAsync(invalidId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedDtoList_WhenEmployeesExist()
        {
            // Arrange
            var employees = new List<Employee>
            {
                CreateEmployee(1, "E001", "Alice"),
                CreateEmployee(2, "E002", "Bob"),
                CreateEmployee(3, "E003", "Charlie")
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, e => e.EmployeeCode == "E001");
            Assert.Contains(result, e => e.EmployeeCode == "E002");
            Assert.Contains(result, e => e.EmployeeCode == "E003");
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoEmployeesExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Employee>());

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
            var dto = CreateEmployeeDto("E100", "New Employee");
            var employee = _mapper.Map<Employee>(dto);
            employee.EmployeeId = 10;

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .ReturnsAsync((Employee e) => { e.EmployeeId = 10; return e; });

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.EmployeeId);
            Assert.Equal("E100", result.EmployeeCode);
            Assert.Equal("New Employee", result.EmployeeName);
            _mockRepository.Verify(r => r.AddAsync(It.Is<Employee>(e => 
                e.EmployeeCode == "E100" && 
                e.EmployeeName == "New Employee")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldMapAllProperties()
        {
            // Arrange
            var dto = new EmployeeDto
            {
                EmployeeCode = "E200",
                EmployeeName = "Full Employee",
                Phone = "0123456789",
                Email = "full@company.com",
                Department = "IT",
                Position = "Senior Dev",
                Salary = 50000m,
                WorkStatus = "active"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .ReturnsAsync((Employee e) => { e.EmployeeId = 20; return e; });

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal("E200", result.EmployeeCode);
            Assert.Equal("Full Employee", result.EmployeeName);
            Assert.Equal("0123456789", result.Phone);
            Assert.Equal("full@company.com", result.Email);
            Assert.Equal(50000m, result.Salary);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task UpdateAsync_ShouldMapAndCallRepository()
        {
            // Arrange
            var dto = CreateEmployeeDto("E300", "Updated Employee");
            dto.EmployeeId = 30;

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(dto);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e =>
                e.EmployeeId == 30 &&
                e.EmployeeCode == "E300" &&
                e.EmployeeName == "Updated Employee")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllModifiedFields()
        {
            // Arrange
            var dto = new EmployeeDto
            {
                EmployeeId = 40,
                EmployeeCode = "E400",
                EmployeeName = "Modified",
                Position = "Manager",
                Salary = 75000m,
                WorkStatus = "active"
            };

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(dto);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e =>
                e.EmployeeId == 40 &&
                e.Position == "Manager" &&
                e.Salary == 75000m)), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task DeleteAsync_ShouldDeleteEmployee_WhenEmployeeExists()
        {
            // Arrange
            var employee = CreateEmployee(50, "E500", "To Delete");
            _mockRepository.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(employee);
            _mockRepository.Setup(r => r.DeleteAsync(50)).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(50);

            // Assert
            // Note: GetByIdAsync is called twice - once in DeleteAsync and once in DeleteAvatarAsync
            _mockRepository.Verify(r => r.GetByIdAsync(50), Times.Exactly(2));
            _mockRepository.Verify(r => r.DeleteAsync(50), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenEmployeeNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Employee)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.DeleteAsync(999));
            Assert.Contains("Không tìm thấy nhân viên", exception.Message);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAvatar_WhenEmployeeHasAvatar()
        {
            // Arrange
            var employee = CreateEmployee(60, "E600", "With Avatar");
            employee.AvatarUrl = "/uploads/avatars/test.jpg";
            _mockRepository.Setup(r => r.GetByIdAsync(60)).ReturnsAsync(employee);
            _mockRepository.Setup(r => r.DeleteAsync(60)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(60);

            // Assert
            _mockRepository.Verify(r => r.GetByIdAsync(60), Times.Exactly(2)); // Once for delete check, once for avatar deletion
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e => e.AvatarUrl == null)), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(60), Times.Once);
        }

        #endregion

        #region Search Tests

        [Fact]
        public async Task SearchAsync_ShouldReturnMappedResults()
        {
            // Arrange
            var employees = new List<Employee>
            {
                CreateEmployee(1, "E001", "Search Result 1"),
                CreateEmployee(2, "E002", "Search Result 2")
            };
            _mockRepository.Setup(r => r.SearchAsync("Search")).ReturnsAsync(employees);

            // Act
            var result = (await _service.SearchAsync("Search")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Contains("Search Result", e.EmployeeName));
            _mockRepository.Verify(r => r.SearchAsync("Search"), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            // Arrange
            _mockRepository.Setup(r => r.SearchAsync("NoMatch")).ReturnsAsync(new List<Employee>());

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
                .ReturnsAsync(new List<Employee>());

            // Act
            var result = await _service.SearchAsync(keyword);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetByDepartment Tests

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturnEmployeesFromDepartment()
        {
            // Arrange
            var employees = new List<Employee>
            {
                CreateEmployee(1, "E001", "IT Employee 1", "IT"),
                CreateEmployee(2, "E002", "IT Employee 2", "IT")
            };
            _mockRepository.Setup(r => r.GetByDepartmentAsync("IT")).ReturnsAsync(employees);

            // Act
            var result = (await _service.GetByDepartmentAsync("IT")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal("IT", e.Department));
            _mockRepository.Verify(r => r.GetByDepartmentAsync("IT"), Times.Once);
        }

        [Fact]
        public async Task GetByDepartmentAsync_ShouldReturnEmpty_ForNonExistentDepartment()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByDepartmentAsync("NonExistent"))
                .ReturnsAsync(new List<Employee>());

            // Act
            var result = (await _service.GetByDepartmentAsync("NonExistent")).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByWorkStatus Tests

        [Fact]
        public async Task GetByWorkStatusAsync_ShouldReturnActiveEmployees()
        {
            // Arrange
            var employees = new List<Employee>
            {
                CreateEmployee(1, "E001", "Active 1", workStatus: "active"),
                CreateEmployee(2, "E002", "Active 2", workStatus: "active")
            };
            _mockRepository.Setup(r => r.GetByWorkStatusAsync("active")).ReturnsAsync(employees);

            // Act
            var result = (await _service.GetByWorkStatusAsync("active")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal("active", e.WorkStatus));
        }

        [Fact]
        public async Task GetByWorkStatusAsync_ShouldReturnInactiveEmployees()
        {
            // Arrange
            var employees = new List<Employee>
            {
                CreateEmployee(1, "E001", "Inactive", workStatus: "inactive")
            };
            _mockRepository.Setup(r => r.GetByWorkStatusAsync("inactive")).ReturnsAsync(employees);

            // Act
            var result = (await _service.GetByWorkStatusAsync("inactive")).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("inactive", result[0].WorkStatus);
        }

        #endregion

        #region AddOrUpdateWithAvatar Tests

        [Fact]
        public async Task AddOrUpdateWithAvatarAsync_ShouldAddNewEmployee_WhenIdIsZero()
        {
            // Arrange
            var dto = CreateEmployeeDto("E700", "With Avatar");
            dto.EmployeeId = 0;

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Employee>()))
                .ReturnsAsync((Employee e) => { e.EmployeeId = 70; return e; });

            // Act
            var result = await _service.AddOrUpdateWithAvatarAsync(dto);

            // Assert
            Assert.Equal(70, result.EmployeeId);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        }

        [Fact]
        public async Task AddOrUpdateWithAvatarAsync_ShouldUpdateExistingEmployee()
        {
            // Arrange
            var existingEmployee = CreateEmployee(80, "E800", "Existing");
            var dto = CreateEmployeeDto("E800", "Updated");
            dto.EmployeeId = 80;

            _mockRepository.Setup(r => r.GetByIdAsync(80)).ReturnsAsync(existingEmployee);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddOrUpdateWithAvatarAsync(dto);

            // Assert
            Assert.Equal(80, result.EmployeeId);
            _mockRepository.Verify(r => r.GetByIdAsync(80), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateWithAvatarAsync_ShouldThrowException_WhenEmployeeNotFoundForUpdate()
        {
            // Arrange
            var dto = CreateEmployeeDto("E900", "Not Found");
            dto.EmployeeId = 900;

            _mockRepository.Setup(r => r.GetByIdAsync(900)).ReturnsAsync((Employee)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _service.AddOrUpdateWithAvatarAsync(dto));
            Assert.Contains("Không tìm thấy nhân viên", exception.Message);
        }

        [Fact]
        public async Task AddOrUpdateWithAvatarAsync_ShouldRejectInvalidFileExtension()
        {
            // Arrange
            var dto = CreateEmployeeDto("E1000", "Invalid File");
            dto.EmployeeId = 0;
            dto.AvatarFile = CreateMockFormFile("test.txt", "text/plain");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _service.AddOrUpdateWithAvatarAsync(dto));
            Assert.Contains("Chỉ hỗ trợ định dạng", exception.Message);
        }

        [Fact]
        public async Task AddOrUpdateWithAvatarAsync_ShouldRejectOversizedFile()
        {
            // Arrange
            var dto = CreateEmployeeDto("E1100", "Large File");
            dto.EmployeeId = 0;
            dto.AvatarFile = CreateMockFormFile("large.jpg", "image/jpeg", 6 * 1024 * 1024); // 6MB

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _service.AddOrUpdateWithAvatarAsync(dto));
            Assert.Contains("Kích thước ảnh tối đa 5MB", exception.Message);
        }

        #endregion

        #region DeleteAvatar Tests

        [Fact]
        public async Task DeleteAvatarAsync_ShouldClearAvatarUrl_WhenEmployeeExists()
        {
            // Arrange
            var employee = CreateEmployee(1200, "E1200", "With Avatar");
            employee.AvatarUrl = "/uploads/avatars/test.jpg";

            _mockRepository.Setup(r => r.GetByIdAsync(1200)).ReturnsAsync(employee);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAvatarAsync(1200);

            // Assert
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e => 
                e.EmployeeId == 1200 && e.AvatarUrl == null)), Times.Once);
        }

        [Fact]
        public async Task DeleteAvatarAsync_ShouldThrowException_WhenEmployeeNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(9999)).ReturnsAsync((Employee)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _service.DeleteAvatarAsync(9999));
            Assert.Contains("Không tìm thấy nhân viên", exception.Message);
        }

        #endregion

        #region Helper Methods

        private Employee CreateEmployee(int id, string code, string name, 
            string department = "IT", string workStatus = "active")
        {
            return new Employee
            {
                EmployeeId = id,
                EmployeeCode = code,
                EmployeeName = name,
                Department = department,
                WorkStatus = workStatus,
                HireDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private EmployeeDto CreateEmployeeDto(string code, string name)
        {
            return new EmployeeDto
            {
                EmployeeCode = code,
                EmployeeName = name,
                HireDate = DateTime.Now,
                WorkStatus = "active"
            };
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType, long size = 1024)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(size);
            
            var stream = new MemoryStream(new byte[size]);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mockFile.Object;
        }

        #endregion
    }
}
