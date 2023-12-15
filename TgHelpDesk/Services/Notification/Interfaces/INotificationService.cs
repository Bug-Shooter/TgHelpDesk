namespace TgHelpDesk.Services.Notification.Interfaces
{
    public interface INotificationService<T>
        where T : class
    {
        Task OnCreate(T entity, CancellationToken cancellationToken = default);
        Task OnUpdate(T entity, T oldEntity, CancellationToken cancellationToken = default);
        Task OnDelete(T entity, CancellationToken cancellationToken = default);
    }
}
