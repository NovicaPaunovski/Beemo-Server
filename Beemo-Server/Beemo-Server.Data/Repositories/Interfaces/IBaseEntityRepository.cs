namespace Beemo_Server.Data.Repositories
{
    public interface IBaseEntityRepository<TEntity>
    {
        TEntity Insert(TEntity entity);
        TEntity Delete(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity GetById(int id);
        ICollection<TEntity> GetAll(bool includeDeprecated = false);
        void SetValues(TEntity target, TEntity source);
    }
}
