using Jarstat.Client.Responses;

namespace Jarstat.Client.Extensions;

public static class HierarchicalCollectionExtension
{
    public static async Task<ItemResponse?> FindItemResponseOrDefaultAsync(this Assortment<ItemResponse> collection, Guid itemId)
    {
        ItemResponse? result = null;

        foreach (var item in collection)
        {
            if (item.ItemId == itemId) 
                return item;

            result = await item.Children.FindItemResponseOrDefaultAsync(itemId);

            if (result is not null) 
                return result;
        }

        return result;
    }
}
