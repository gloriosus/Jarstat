using Jarstat.Client.Responses;

namespace Jarstat.Client.Extensions;

public static class HierarchicalListExtension
{
    public static async Task<ItemResponse?> FindItemResponseOrDefaultAsync(this List<ItemResponse> list, Guid itemId)
    {
        ItemResponse? result = default;

        foreach (var item in list)
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
