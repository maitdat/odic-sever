using Microsoft.AspNetCore.Identity;
using Sever.Entity;

namespace Sever.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository ( UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public bool CheckUser(string username)
        {
            return _userManager.Users.Any(u => u.UserName == username);
        }

        public User Get(string username)
        {
            return _userManager.Users.FirstOrDefault(x => x.UserName == username);
        }
    }
}
