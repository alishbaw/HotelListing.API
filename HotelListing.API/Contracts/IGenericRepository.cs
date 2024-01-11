namespace HotelListing.API.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id); //retrieves one record of type T and takes a potentially nullable integer called id
        Task<List<T>> GetAllAsync();

        Task<T> AddAsync(T entity);

        Task DeleteAsync(int id);

        Task UpdateAsync(T entity);

        Task<bool> Exists(int id);
    }
}
