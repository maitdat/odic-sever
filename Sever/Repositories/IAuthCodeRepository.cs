using Sever.Entity;
using Sever.Models;

namespace Sever.Repositories
{
    public interface IAuthCodeRepository
    {
        Task<AuthorizationRequestData>? FindByCode(string code);
        Task Add(AuthorizationRequestData data);
        void Delete(string code);

    }
}
