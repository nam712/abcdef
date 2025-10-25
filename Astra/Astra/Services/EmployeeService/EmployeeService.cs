// Implementation of IEmployeeService using IEmployeeRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{


    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EmployeeDto> GetByIdAsync(int employeeId)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> AddAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            var added = await _repository.AddAsync(employee);
            return _mapper.Map<EmployeeDto>(added);
        }

        public async Task UpdateAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            await _repository.UpdateAsync(employee);
        }

        public async Task DeleteAsync(int employeeId)
        {
            await _repository.DeleteAsync(employeeId);
        }

        public async Task<IEnumerable<EmployeeDto>> SearchAsync(string keyword)
        {
            var employees = await _repository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<IEnumerable<EmployeeDto>> GetByDepartmentAsync(string department)
        {
            var employees = await _repository.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<IEnumerable<EmployeeDto>> GetByWorkStatusAsync(string workStatus)
        {
            var employees = await _repository.GetByWorkStatusAsync(workStatus);
            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }
    }


}
