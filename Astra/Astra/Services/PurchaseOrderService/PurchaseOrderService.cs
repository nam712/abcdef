using AutoMapper;
using YourShopManagement.API.DTOs.PurchaseOrder;
using YourShopManagement.API.Models;
using YourShopManagement.API.Repositories;

namespace YourShopManagement.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repo;
        private readonly IMapper _mapper;

        public PurchaseOrderService(IPurchaseOrderRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<(bool, string, PurchaseOrderDto?)> CreateAsync(CreatePurchaseOrderDto dto)
        {
            if (await _repo.ExistsByCodeAsync(dto.PoCode))
                return (false, "Mã phiếu nhập đã tồn tại", null);

            var entity = _mapper.Map<PurchaseOrder>(dto);
            entity.PurchaseOrderDetails = dto.Details.Select(d => new PurchaseOrderDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                ImportPrice = d.ImportPrice,
                FinalAmount = d.Quantity * d.ImportPrice
            }).ToList();

            entity.TotalAmount = entity.PurchaseOrderDetails.Sum(x => x.FinalAmount);

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var result = _mapper.Map<PurchaseOrderDto>(entity);
            return (true, "Tạo phiếu nhập thành công", result);
        }

        public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseOrderDto>>(list);
        }

        public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<PurchaseOrderDto>(entity);
        }

        public async Task<(bool, string)> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                return (false, "Không tìm thấy phiếu nhập");

            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
            return (true, "Đã xóa phiếu nhập");
        }
    }
}
