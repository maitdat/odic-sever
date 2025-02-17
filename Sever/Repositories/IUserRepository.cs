using Sever.Entity;

namespace Sever.Repositories
{
    public interface IUserRepository
    {
        public bool CheckUser(string username);

        public User Get(string username);
    }
}
