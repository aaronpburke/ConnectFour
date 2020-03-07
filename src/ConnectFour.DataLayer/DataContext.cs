using ConnectFour.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectFour.DataLayer
{
    /// <summary>
    /// <seealso cref="DbContext"/> containing all tracked Entity Framework entities
    /// </summary>
    public class DataContext : DbContext
    {
        /// <summary>
        /// Collection of all <see cref="Game"/>s
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <inheritdoc/>
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>();

            modelBuilder.Entity<Player>()
                // Every unique player name only gets one DB ID
                .HasIndex(p => new { p.Id, p.Name })
                    .IsUnique();

            modelBuilder.Entity<GameBoard>();

            modelBuilder.Entity<GameMove>()
                // Ensure that move sequence numbers within a single game board are unique
                .HasIndex(m => new { m.GameBoardId, m.MoveId })
                    .IsUnique();
        }
    }

    /// <summary>
    /// Required interface for DB objects stored via Entity Framework to use <seealso cref="IGenericRepository{TKey, TEntity}"/>
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key</typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        TKey Id { get; set; }
    }

    /// <summary>
    /// Generic interface for easily accessing data objects stored in Entity Framework.
    /// </summary>
    /// <typeparam name="TKey">Type of the <typeparamref name="TEntity"/>'s primary key</typeparam>
    /// <typeparam name="TEntity">Type of the entity to store</typeparam>
    public interface IGenericRepository<TKey, TEntity> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Returns the queryable entity set for the given type {T}.
        /// </summary>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// Gets an entity by id from the database or the local change tracker.
        /// </summary>
        /// <param name="id">The id of the entity. This can also be a composite key.</param>
        /// <returns>The resolved entity</returns>
        TEntity GetById(TKey id);

        /// <summary>
        /// Gets an entity by id from the database or the local change tracker.
        /// </summary>
        /// <param name="id">The id of the entity. This can also be a composite key.</param>
        /// <returns>The resolved entity</returns>
        Task<TEntity> GetByIdAsync(TKey id);

        /// <summary>
        /// Marks the entity instance to be saved to the store.
        /// </summary>
        /// <param name="entity">An entity instance that should be saved to the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        void Create(TEntity entity);

        /// <summary>
        /// Marks the entity instance to be saved to the store.
        /// </summary>
        /// <param name="entity">An entity instance that should be saved to the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        Task CreateAsync(TEntity entity);

        /// <summary>
        /// Marks the changes of an existing entity to be saved to the store.
        /// </summary>
        /// <param name="entity">An instance that should be updated in the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        void Update(TKey id, TEntity entity);

        /// <summary>
        /// Marks the changes of an existing entity to be saved to the store.
        /// </summary>
        /// <param name="entity">An instance that should be updated in the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        Task UpdateAsync(TKey id, TEntity entity);

        /// <summary>
        /// Marks an existing entity to be deleted from the store.
        /// </summary>
        /// <param name="entity">An entity instance that should be deleted from the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        void Delete(TKey id);

        /// <summary>
        /// Marks an existing entity to be deleted from the store.
        /// </summary>
        /// <param name="entity">An entity instance that should be deleted from the database.</param>
        /// <remarks>Implementors should delegate this to the current <see cref="IDbContext" /></remarks>
        Task DeleteAsync(TKey id);
    }

    /// <summary>
    /// Generic class for easily accessing data objects stored in Entity Framework.
    /// </summary>
    /// <typeparam name="TKey">Type of the <typeparamref name="TEntity"/>'s primary key</typeparam>
    /// <typeparam name="TEntity">Type of the entity to store</typeparam>
    public class GenericRepository<TKey, TEntity> : IGenericRepository<TKey, TEntity> where TEntity : class, IEntity<TKey>
    {
        protected readonly DbContext Context;

        public GenericRepository(DbContext dbContext)
        {
            Context = dbContext;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TEntity> GetAll()
        {
            return Context.Set<TEntity>().AsNoTracking();
        }

        /// <inheritdoc/>
        public virtual TEntity GetById(TKey id)
        {
            return Context.Set<TEntity>()
                        .Find(id);
        }

        /// <inheritdoc/>
        public virtual async Task<TEntity> GetByIdAsync(TKey id)
        {
            return await Context.Set<TEntity>()
                        .FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual void Create(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
            Context.SaveChanges();
        }

        /// <inheritdoc/>
        public virtual async Task CreateAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual void Update(TKey id, TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
            Context.SaveChanges();
        }

        /// <inheritdoc/>
        public virtual async Task UpdateAsync(TKey id, TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
            await Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual void Delete(TKey id)
        {
            var entity = GetById(id);
            Context.Set<TEntity>().Remove(entity);
            Context.SaveChanges();
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            Context.Set<TEntity>().Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}
