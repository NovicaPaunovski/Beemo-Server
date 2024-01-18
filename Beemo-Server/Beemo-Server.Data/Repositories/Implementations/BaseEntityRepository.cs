using Beemo_Server.Data.Context;
using Beemo_Server.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beemo_Server.Data.Repositories
{
    public class BaseEntityRepository<TEntity> : IBaseEntityRepository<TEntity>
            where TEntity : BaseEntity
    {
        #region Fields
        protected readonly BeemoContext _context;
        #endregion Fields

        #region Public Constructor
        public BaseEntityRepository(BeemoContext context)
        {
            _context = context;
        }
        #endregion Public Constructor

        #region Public Methods
        public TEntity Insert(TEntity entity)
        {
            entity.CreationDate = DateTime.Now;
            _context.Set<TEntity>().Add(entity);

            _context.SaveChanges();
            return entity;
        }

        public TEntity Delete(TEntity entity)
        {
            _context.Set<TEntity>().Attach(entity);

            entity.ModifiedDate = DateTime.Now;
            entity.Deprecated = true;

            _context.SaveChanges();
            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            _context.Set<TEntity>().Attach(entity);

            entity.ModifiedDate = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;

            _context.SaveChanges();
            return entity;
        }

        public ICollection<TEntity> GetAll(bool includeDeprecated = false)
        {
            return _context.Set<TEntity>().Where(entity => includeDeprecated || !entity.Deprecated).ToList();
        }

        public TEntity GetById(int id)
        {
            var entity = _context.Set<TEntity>().FirstOrDefault(entity => entity.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException($"No entity of type {typeof(TEntity)} with id {id} found.");
            }
            return entity;
        }

        public void SetValues(TEntity target, TEntity source)
        {
            _context.Entry(target).CurrentValues.SetValues(source);
        }

        #endregion Public Methods
    }
}
