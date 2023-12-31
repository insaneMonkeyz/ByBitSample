﻿using MarketDataProvider.BybitApi;
using MarketDataProvider.BybitApi.DTO.Rest;
using MarketDataProvider.BybitApi.DTO.Rest.Market;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace MarketDataProvider
{
    internal static class Extensions
    {
        public static string ToBybitDateFormat(this DateOnly date)
        {
            var month = 
                System
                    .Globalization
                    .CultureInfo
                    .GetCultureInfo("en-US")
                    .DateTimeFormat
                    .GetAbbreviatedMonthName(date.Month);

            return $"{date:dd}{month}{date:yy}";
        }

        public static string ToBybitCategory(this Categories securityType)
        {
            return securityType switch
            {
                Categories.Spot => "spot",
                Categories.Option => "option",
                Categories.Linear => "linear",
                Categories.Inverse => "inverse",

                _ => throw new NotSupportedException($"the security type {securityType} is not supported")
            };
        }

        public static Categories ToBybitCategory(this SecurityKind? kind)
        {
            return kind switch
            {
                SecurityKind.Spot => Categories.Spot,
                SecurityKind.Option => Categories.Option,
                SecurityKind.Cfd => Categories.Linear,
                SecurityKind.Futures => Categories.Linear,
                _ => throw new NotSupportedException($"security kind {kind} has no corresponding Bybit category")
            };
        }

        public static bool TryGetFreshFromCache<TKey, TCache>(
            this Dictionary<TKey, Cache<TCache>> repository, TKey key, out IReadOnlyDictionary<string, TCache>? cachedItems) 
                where TKey : notnull
        {
            if (!repository.TryGetValue(key, out var cache))
            {
                repository[key] = new();
                cachedItems = null;
                return false;
            }

            if (cache.IsOutdated)
            {
                cachedItems = null;
                return false;
            }

            cachedItems = cache.Items!;
            return true;
        }

        public static Security[] ToSecurities<TSecurity>(this Response<TSecurity> responseMessage, SecurityKind kind) 
               where TSecurity : SecurityDescription
        {
            var descriptions = responseMessage.Data?.Entities;

            if (descriptions is null)
            {
                return Array.Empty<Security>();
            }

            return descriptions
                .Where(d => d.Ticker is not null)
                .Select(d => new Security()
                {
                    EntityType = TradingEntityType.Cryptocurrency,
                    Kind = kind,
                    Ticker = d.Ticker!
                })
                .ToArray();
        }

        public static bool Is<T>(this JObject jobj, JSchema schema, out T deserialized)
        {
            if (jobj.IsValid(schema))
            {
                deserialized = jobj.ToObject<T>();
                return deserialized != null;
            }

            deserialized = default;
            return false;
        }

        public static ISecurity? FindInCache(this IEnumerable<Cache<ISecurity>> caches, string? ticker)
        {
            if (ticker is null)
            {
                return null;
            }

            foreach (var cache in caches)
            {
                if (cache.Items!.TryGetValue(ticker, out var security))
                {
                    return security;
                }
            }

            return default;
        }

        public static DateTime ToBybitTimestamp(this long value)
        {
            return DateTime.UnixEpoch.AddMilliseconds(value);
        }
    }
}
