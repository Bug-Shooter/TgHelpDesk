using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Repositories.Interface;
using TgHelpDesk.Services.Notification.Interfaces;

namespace TgHelpDesk.Repositories
{
    public class HelpRequestRepository : IRepository<HelpRequest>
    {
        private readonly TgHelpDeskDbContext _dbContext;
        private readonly INotificationService<HelpRequest> _notificationService;
        public HelpRequestRepository(TgHelpDeskDbContext tgHelpDeskDbContext, INotificationService<HelpRequest> notificationService)
        {
            _dbContext = tgHelpDeskDbContext;
            _notificationService = notificationService;
        }

        public EntityEntry<HelpRequest> Entry(HelpRequest request) => _dbContext.Entry(request);
        public IEnumerable<HelpRequest> AsEnumerable() => _dbContext.HelpRequests.Include(x => x.TelegramUser);

        public HelpRequest? Get(int id) => _dbContext.HelpRequests.Find(id);
        public async Task<HelpRequest?> GetAsync(int id) => await _dbContext.HelpRequests.FindAsync(id);


        public void Create(HelpRequest item)
        {
            _dbContext.HelpRequests.Add(item);
            if (_dbContext.SaveChanges() > 0)
                Task.Run(() => _notificationService.OnCreate(item).GetAwaiter().GetResult());
        }
        public async Task CreateAsync(HelpRequest item)
        {
            _dbContext.HelpRequests.Add(item);
            if (await _dbContext.SaveChangesAsync() > 0)
               await _notificationService.OnCreate(item);
        }

        public void Update(HelpRequest item)
        {
            var reqEntry = _dbContext.Entry(item);
            if (reqEntry.State == EntityState.Modified)
            {
                var OldItem = (HelpRequest)reqEntry.OriginalValues.ToObject();
                _dbContext.HelpRequests.Update(item);
                if (_dbContext.SaveChanges() > 0)
                    _ = _notificationService.OnUpdate(item, OldItem);
            }
        }
        public async Task UpdateAsync(HelpRequest item)
        {
            var reqEntry = _dbContext.Entry(item);
            if (reqEntry.State == EntityState.Modified)
            {
                var OldItem = (HelpRequest)reqEntry.OriginalValues.ToObject();
                _dbContext.HelpRequests.Update(item);
                if (await _dbContext.SaveChangesAsync() > 0)
                    await _notificationService.OnUpdate(item, OldItem);
            }
        }

        public void Delete(int id)
        {
            var item = _dbContext.HelpRequests.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _dbContext.HelpRequests.Remove(item);
                if (_dbContext.SaveChanges() > 0)
                    Task.Run(() => _notificationService.OnDelete(item).GetAwaiter().GetResult());
            }
        }
        public async Task DeleteAsync(int id)
        {
            var item = await _dbContext.HelpRequests.FirstOrDefaultAsync(x => x.Id == id);
            if (item != null)
            {
                _dbContext.HelpRequests.Remove(item);
                if (await _dbContext.SaveChangesAsync() > 0)
                    await _notificationService.OnDelete(item);
            }
        }
    }
}
