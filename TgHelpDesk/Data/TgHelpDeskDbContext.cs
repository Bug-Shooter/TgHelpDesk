using Microsoft.EntityFrameworkCore;
using TgHelpDesk.Models.Audit;
using TgHelpDesk.Models.Core;
using TgHelpDesk.Models.Users;
using TgHelpDesk.Services.Bot;

namespace TgHelpDesk.Data
{
    public class TgHelpDeskDbContext : DbContext
    {
        public DbSet<HelpRequest> HelpRequests { get; set; }
        public DbSet<HelpRequestLog> HelpRequestsLog { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public TgHelpDeskDbContext(DbContextOptions<TgHelpDeskDbContext> options) : base(options)
        {
            //Database.Migrate();
        }
        public override int SaveChanges()
        {
            var auditEntries = ActionBeforeSaveChanges();
            var result = base.SaveChanges();
            ActionAfterSaveChanges(auditEntries);
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var auditEntries = ActionBeforeSaveChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await ActionAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> ActionBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                //исключаем сущности и состояния из логирования
                if (entry.Entity is HelpRequestLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)              
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName()
                };
                entries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    var propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                        case EntityState.Detached:
                            break;
                        case EntityState.Unchanged:
                            break;
                    }
                }
            }

            foreach (var entry in entries.Where(_ => !_.HasTemporaryProperties))
            {
                HelpRequestsLog.Add(entry.ToAudit());
            }

            return entries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task ActionAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var entry in auditEntries)
            {
                foreach (var prop in entry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                HelpRequestsLog.Add(entry.ToAudit());
            }

            return SaveChangesAsync();
        }
    }
}
