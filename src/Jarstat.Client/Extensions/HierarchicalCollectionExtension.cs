using Jarstat.Client.Abstractions;
using Jarstat.Domain.Abstractions;

namespace Jarstat.Client.Extensions;

public static class HierarchicalCollectionExtension
{
    public static T? FindOrDefault<T>(this IEnumerable<T> collection, Guid id)
        where T : IHierarchical<T>, IDefault<T>
    {
        var result = T.Default;

        foreach (var item in collection)
        {
            if (item.Id == id) 
                return item;

            result = item.Children.FindOrDefault(id);

            if (result is not null) 
                return result;
        }

        return result;
    }
}
