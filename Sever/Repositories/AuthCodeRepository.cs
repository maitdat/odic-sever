using Microsoft.EntityFrameworkCore;
using Sever.Entity;
using Sever.Models;

namespace Sever.Repositories
{
    public class AuthCodeRepository : IAuthCodeRepository
    {
        private readonly AppDbContext _appDbContext;
        public readonly Dictionary<string, AuthCodeItem> items = [];
        
        public AuthCodeRepository (AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void Delete(string code)
        {
            var item = _appDbContext.AuthorizationRequestData.FirstOrDefault(x => x.AuthorizationCode == code);
            if (item == null) return;
            _appDbContext.AuthorizationRequestData.Remove(item);
        }

        public async Task<AuthorizationRequestData>? FindByCode(string code)
        {
            return await _appDbContext.AuthorizationRequestData
                .Include(x=>x.User)
                .Where(x => x.AuthorizationCode == code)
                .FirstOrDefaultAsync();
        }

        public async Task Add(AuthorizationRequestData data)
        {
            _appDbContext.AuthorizationRequestData.Add(data);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
