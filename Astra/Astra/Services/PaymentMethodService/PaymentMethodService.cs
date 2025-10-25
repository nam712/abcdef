// Implementation of IPaymentMethodService using IPaymentMethodRepository and AutoMapper
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{


    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IPaymentMethodRepository _repository;
        private readonly IMapper _mapper;

        public PaymentMethodService(IPaymentMethodRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaymentMethodDto> GetByIdAsync(int paymentMethodId)
        {
            var method = await _repository.GetByIdAsync(paymentMethodId);
            return method == null ? null : _mapper.Map<PaymentMethodDto>(method);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetAllAsync()
        {
            var methods = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
        }

        public async Task<PaymentMethodDto> AddAsync(PaymentMethodDto paymentMethodDto)
        {
            var method = _mapper.Map<PaymentMethod>(paymentMethodDto);
            var added = await _repository.AddAsync(method);
            return _mapper.Map<PaymentMethodDto>(added);
        }

        public async Task UpdateAsync(PaymentMethodDto paymentMethodDto)
        {
            var method = _mapper.Map<PaymentMethod>(paymentMethodDto);
            await _repository.UpdateAsync(method);
        }

        public async Task DeleteAsync(int paymentMethodId)
        {
            await _repository.DeleteAsync(paymentMethodId);
        }

        public async Task<IEnumerable<PaymentMethodDto>> SearchAsync(string keyword)
        {
            var methods = await _repository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetActiveAsync()
        {
            var methods = await _repository.GetActiveAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
        }
    }


}
