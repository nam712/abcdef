using Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllAsync(int? userId = null, bool? isRead = null, int limit = 50);
        Task<Notification?> GetByIdAsync(int id);
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification?> UpdateAsync(int id, Notification notification);
        Task<bool> DeleteAsync(int id);
        Task<int> GetUnreadCountAsync(int? userId = null);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadAsync(int? userId = null);
        Task<bool> DeleteAllAsync(int? userId = null);
    }
}
