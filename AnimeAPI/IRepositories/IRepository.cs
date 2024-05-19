namespace AnimeAPI.IRepositories
{
    /// <summary>
    /// Defines a generic repository interface for performing CRUD operations on entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Retrieves an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumeration of all entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        Task<TEntity> UpdateAsync(TEntity entity);

        /// <summary>
        /// Removes an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to remove.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the entity was successfully removed.</returns>
        Task<bool> RemoveAsync(int id);
    }

}
