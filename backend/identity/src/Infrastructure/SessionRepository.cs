using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovaCommerce.Identity.Domain;

namespace NovaCommerce.Identity.Infrastructure
{
    public class SessionRepository : ISessionRepository
    {
        private readonly IdentityDbContext _context;

        public SessionRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<Session> GetByIdAsync(Guid id)
        {
            return await _context.Sessions.FindAsync(id);
        }

        public async Task AddAsync(Session session)
        {
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Session session)
        {
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync();
        }

        public async Task<Session> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Sessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);
        }
    }
}
