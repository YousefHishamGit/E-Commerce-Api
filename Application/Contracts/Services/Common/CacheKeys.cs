using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Services.Common
{
    public static class CacheKeys
    {
        private const string Prefix = "ecom";

        // ─── Expirations ────────────────────────────────────────────────────────
        public static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan BasketExpiration  = TimeSpan.FromDays(30);
        public static readonly TimeSpan ShortExpiration   = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan LongExpiration    = TimeSpan.FromHours(1);

        // ─── Generic helpers ────────────────────────────────────────────────────

        /// <summary>Single entity by id. e.g. ecom:product:42</summary>
        public static string ById<TEntity>(object id)
            => $"{Prefix}:{Name<TEntity>()}:{id}";

        /// <summary>All entities of a type. e.g. ecom:product:all</summary>
        public static string All<TEntity>()
            => $"{Prefix}:{Name<TEntity>()}:all";

        /// <summary>Filtered list. e.g. ecom:product:list:category=3:sort=price</summary>
       public static string List<TEntity>(params (string Name, object? Value)[] filters)
    => $"{Prefix}:{Name<TEntity>()}:list:" +
       string.Join(":", filters
           .Where(f => f.Value is not null && !(f.Value is string s && string.IsNullOrWhiteSpace(s)))
           .OrderBy(f => f.Name, StringComparer.Ordinal)
           .Select(f => $"{f.Name.ToLowerInvariant()}={Normalize(f.Value)}"));

        /// <summary>Entity scoped to a user. e.g. ecom:user:{userId}:address:all</summary>
        public static string ForUser<TEntity>(Guid userId, string suffix)
            => $"{Prefix}:user:{userId}:{Name<TEntity>()}:{suffix}";

        // ─── Basket (fixed key — basket lives in Redis per userId) ──────────────

        /// <summary>Basket key for a user. e.g. ecom:basket:{userId}</summary>
        public static string Basket(Guid userId)
            => $"{Prefix}:basket:{userId}";

        // ─── Private helpers ────────────────────────────────────────────────────
        private static string Name<TEntity>() => typeof(TEntity).Name.ToLowerInvariant();

        private static string Normalize(object? value) => value switch
        {
            null     => "-",
            string s => string.IsNullOrWhiteSpace(s) ? "-" : s.Trim().ToLowerInvariant(),
            _        => value.ToString()!
        };
    }
}
