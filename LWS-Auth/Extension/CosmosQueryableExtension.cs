using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;

namespace LWS_Auth.Extension;

public static class CosmosQueryableExtension
{
    public static async Task<T> CosmosFirstOrDefaultAsync<T>(this IQueryable<T> queryable)
    {
        var targetObject = default(T);
        var feedIterator = queryable.Take(1).ToFeedIterator();

        if (feedIterator.HasMoreResults)
        {
            var read = await feedIterator.ReadNextAsync();
            targetObject = read.FirstOrDefault();
        }

        return targetObject;
    }

    public static async Task<List<T>> CosmosToListAsync<T>(this IQueryable<T> queryable)
    {
        var list = new List<T>();

        var feedIterator = queryable.ToFeedIterator();

        while (feedIterator.HasMoreResults)
        {
            var read = await feedIterator.ReadNextAsync();
            list.AddRange(read);
        }

        return list;
    }
}