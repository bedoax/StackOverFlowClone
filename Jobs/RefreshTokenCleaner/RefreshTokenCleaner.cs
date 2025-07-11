using StackOverFlowClone.Data;

namespace StackOverFlowClone.Jobs.CleanerRefreshToken
{
    public class RefreshTokenCleaner
    {
        private readonly AppDbContext _context;

        public RefreshTokenCleaner(AppDbContext context)
        {
            _context = context;
        }

        public async Task RemoveExpiredTokensAsync()
        {
            var expiredTokens = _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }

}
