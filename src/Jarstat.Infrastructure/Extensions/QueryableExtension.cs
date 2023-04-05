using Jarstat.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Jarstat.Infrastructure.Extensions;

internal static class QueryableExtension
{
    public static async Task<Assortment<TSource>> ToAssortmentAsync<TSource>(
        this IQueryable<TSource> source,
        CancellationToken cancellationToken = default)
    {
        var assortment = new Assortment<TSource>();
        await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            assortment.Add(element);
        }

        return assortment;
    }
}
