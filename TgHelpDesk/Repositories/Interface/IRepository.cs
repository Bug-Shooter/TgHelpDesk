using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TgHelpDesk.Repositories.Interface
{
    public interface IRepository<T>
            where T : class
    {
        EntityEntry<T> Entry(T item); // получение Entry

        IEnumerable<T> AsEnumerable(); // получение всех объектов


        T? Get(int id); // получение одного объекта по id
        Task<T?> GetAsync(int id); // получение одного объекта по id


        void Create(T item); // создание объекта
        Task CreateAsync(T item); // создание объекта

        void Update(T item); // обновление объекта
        Task UpdateAsync(T item); // обновление объекта

        void Delete(int id); // удаление объекта по id
        Task DeleteAsync(int id); // удаление объекта по id
    }
}
