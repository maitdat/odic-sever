using Sever.Entity;

namespace Sever.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ICollection<User> _users = new List<User>()
        {
            new User() { Id = "1", UserName = "admin" },
            new User() { Id = "2", UserName = "user" },
            new User() { Id = "3", UserName = "dat" }
        };

        public UserRepository() { }
        public bool CheckUser(string username)
        {
            if(_users.Any(u => u.UserName == username))
            {
                return true;
            }
            return false;
        }

        public User Get(string username)
        {
            return _users.FirstOrDefault(x=>x.UserName == username);
        }
    }
}
