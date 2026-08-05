using Espada.Application.Contracts.Time;
using Espada.Db.Models;
using Espada.Infrastructure.Database;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Espada.Infrastructure.Security
{
    public sealed class OneTimeBootstrapCodeStore(
        EspadaDbContext dbContext,
        IClockService clockService)
    {
        private const int CodeSizeInBytes = 32;

        public async Task<string> CreateAsync(
            string purpose,
            string identityIssuer,
            string identitySubject,
            TimeSpan lifetime,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
            ArgumentException.ThrowIfNullOrWhiteSpace(identityIssuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(identitySubject);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
                lifetime,
                TimeSpan.Zero);

            string code = WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(CodeSizeInBytes));
            DateTimeOffset createdAtUtc = clockService.UtcNow;
            OneTimeBootstrapCodes record = new()
            {
                OneTimeBootstrapCodeId = Guid.NewGuid(),
                CodeHash = Hash(code),
                Purpose = purpose.Trim(),
                IdentityIssuer = identityIssuer.Trim(),
                IdentitySubject = identitySubject.Trim(),
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = createdAtUtc.Add(lifetime)
            };

            await dbContext.OneTimeBootstrapCodes.AddAsync(
                record,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return code;
        }

        public async Task<BootstrapIdentity?> ConsumeAsync(
            string purpose,
            string code,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
            ArgumentException.ThrowIfNullOrWhiteSpace(code);

            string codeHash = Hash(code);
            DateTimeOffset consumedAtUtc = clockService.UtcNow;
            int consumed = await dbContext.OneTimeBootstrapCodes
                .Where(record =>
                    record.CodeHash == codeHash
                    && record.Purpose == purpose
                    && record.ConsumedAtUtc == null
                    && record.ExpiresAtUtc > consumedAtUtc)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        record => record.ConsumedAtUtc,
                        consumedAtUtc),
                    cancellationToken);

            if (consumed == 0)
            {
                return null;
            }

            return await dbContext.OneTimeBootstrapCodes
                .AsNoTracking()
                .Where(record => record.CodeHash == codeHash)
                .Select(record => new BootstrapIdentity(
                    record.IdentityIssuer,
                    record.IdentitySubject))
                .SingleAsync(cancellationToken);
        }

        private static string Hash(string code)
        {
            return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(code)));
        }
    }
}