namespace Beemo_Server.Service
{
    public interface IBaseEntityService<TEntity, TRepository>
    {
        TEntity Create(TEntity entity);
        TEntity Delete(TEntity entity);
        TEntity Update(TEntity entity);
        TEntity GetById(int id);
        ICollection<TEntity> GetAll();
    }
}