using Sever.Models;

namespace Sever.Repositories
{
    public interface IAuthCodeRepository
    {
        AuthCodeItem? FindByCode(string code);
        void Add(string code, AuthCodeItem codeItem);
        void Delete(string code);

    }
}
