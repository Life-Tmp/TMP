namespace TMP.Application.Interfaces
{
    public interface IUnitOfWork
    {
        public ITMPRepository<TEntity> Repository<TEntity>() where TEntity : class;

        bool Complete();
    }
}
