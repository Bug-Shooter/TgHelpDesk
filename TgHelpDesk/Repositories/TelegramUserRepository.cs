using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TgHelpDesk.Data;
using TgHelpDesk.Models.Users;
using TgHelpDesk.Repositories.Interface;

namespace TgHelpDesk.Repositories
{
    public class TelegramUserRepository : IRepository<TelegramUser>
    {
        private readonly TgHelpDeskDbContext _dbContext;
        public TelegramUserRepository(TgHelpDeskDbContext dbContext) 
        {
            _dbContext = dbContext;
        }        
        public EntityEntry<TelegramUser> Entry(TelegramUser item) => _dbContext.Entry(item);
        
        public IEnumerable<TelegramUser> AsEnumerable() => _dbContext.TelegramUsers.AsNoTracking();

        public TelegramUser? Get(int id) => _dbContext.TelegramUsers.FirstOrDefault(x => x.Id == id);
        public async Task<TelegramUser?> GetAsync(int id) => await _dbContext.TelegramUsers.FirstOrDefaultAsync(x=>x.Id == id);

        public void Create(TelegramUser item)
        {
            _dbContext.TelegramUsers.Add(item);
            _dbContext.SaveChanges();
        }
        public async Task CreateAsync(TelegramUser item)
        {
            _dbContext.TelegramUsers.Add(item);
            await _dbContext.SaveChangesAsync();
        }

        public void Update(TelegramUser item)
        {
            _dbContext.TelegramUsers.Update(item);
            _dbContext.SaveChanges();
        }
        public async Task UpdateAsync(TelegramUser item)
        {
            _dbContext.TelegramUsers.Update(item);
            await _dbContext.SaveChangesAsync();
        }

        public void Delete(int id)
        {
            var TgUser = _dbContext.TelegramUsers.FirstOrDefault(x => x.Id == id);
            if (TgUser != null)
            {
                _dbContext.Remove(TgUser);
                _dbContext.SaveChanges();
            }
        }
        public async Task DeleteAsync(int id)
        {
            var TgUser = _dbContext.TelegramUsers.FirstOrDefault(x => x.Id == id);
            if (TgUser != null)
            {
                _dbContext.Remove(TgUser);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
