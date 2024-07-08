using System.Collections;
using TMP.Application.Interfaces;

namespace TMP.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseService _dbContext;

        private Hashtable _repositories;

        public UnitOfWork(DatabaseService dbContext)
        {
            _dbContext = dbContext;
        }

        public bool Complete()
        {
            var numberOfAffectedRows = _dbContext.SaveChanges();
            return numberOfAffectedRows > 0;
        }

        public ITMPRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TEntity).Name;

            if (!_repositories.Contains(type))
            {
                var repositoryType = typeof(TMPRepository<>);

                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (ITMPRepository<TEntity>)_repositories[type];
        }
    }
}
