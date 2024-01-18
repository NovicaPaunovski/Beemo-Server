using Beemo_Server.Data.Context;
using Beemo_Server.Data.Models.Entities;
using Beemo_Server.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Beemo_Server.Service
{
    public abstract class BaseEntityService<TEntity, TRepository> : IBaseEntityService<TEntity, TRepository>
            where TEntity : BaseEntity
            where TRepository : IBaseEntityRepository<TEntity>
    {
        #region Fields
        protected readonly IDbContextFactory<BeemoContext> _dbContextFactory;
        protected TRepository EntityRepository;
        #endregion Fields 

        #region Public Constructor
        public BaseEntityService(TRepository entityRepository, IDbContextFactory<BeemoContext> dbContextFactory)
        {
            EntityRepository = entityRepository;
            _dbContextFactory = dbContextFactory;
        }
        #endregion Public Constructor

        #region Public Methods
        public virtual TEntity Create(TEntity entity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingEntity = EntityRepository.GetById(entity.Id);
                if (existingEntity != null) return existingEntity;

                EntityRepository.Insert(entity);
                return entity;
            }
        }

        public virtual TEntity Delete(TEntity entity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingEntity = EntityRepository.GetById(entity.Id);

                return EntityRepository.Delete(existingEntity);
            }
        }

        public virtual TEntity Update(TEntity entity)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingEntity = EntityRepository.GetById(entity.Id);

                entity.CreationDate = existingEntity.CreationDate;
                entity.ModifiedDate = existingEntity.ModifiedDate;
                entity.Deprecated = existingEntity.Deprecated;
                EntityRepository.SetValues(existingEntity, entity);

                return EntityRepository.Update(entity);
            }
        }

        public virtual ICollection<TEntity> GetAll()
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return EntityRepository.GetAll();
            }
        }

        public virtual TEntity GetById(int id)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                return EntityRepository.GetById(id);
            }
        }
        #endregion Public Methods
    }
}