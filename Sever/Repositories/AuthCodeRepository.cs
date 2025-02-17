using Sever.Models;

namespace Sever.Repositories
{
    public class AuthCodeRepository : IAuthCodeRepository
    {
        public readonly Dictionary<string, AuthCodeItem> items = [];

        public void Add(string code, AuthCodeItem codeItem)
        {
            items[code] = codeItem;
            var obj = items;
        }

        public void Delete(string code)
        {
            items.Remove(code);
        }

        public AuthCodeItem? FindByCode(string code)
        {
            return items.TryGetValue(code, out var codeItem) ? codeItem : null;
        }

    }
}
