using System;
using System.Threading.Tasks;

namespace NovaCommerce.Identity.Domain
{
    public interface ISessionRepository
    {
        Task<Session> GetByIdAsync(Guid id);
        Task AddAsync(Session session);
        Task UpdateAsync(Session session);
        Task<Session> GetByRefreshTokenAsync(string refreshToken);
    }
}
