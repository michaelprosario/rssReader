using AppCore.Models;

namespace AppCore.Services.Articles;

public interface IBlogReaderProvider
{
    Task<List<Article>> GetArticlesFromFeed(Guid feedId, int maxCount);
}

public interface IArticleService2
{
    Task<List<Article>> GetArticlesFromFeedAsync(Guid feedId, int maxCount);
}

public class ArticleService2 : IArticleService2
{
    private readonly IBlogReaderProvider _blogReaderProvider;

    public ArticleService2(IBlogReaderProvider blogReaderProvider)
    {
        this._blogReaderProvider = blogReaderProvider ?? throw new ArgumentNullException(nameof(blogReaderProvider));
    }

    public async Task<List<Article>> GetArticlesFromFeedAsync(Guid feedId, int maxCount)
    {
        if (feedId == Guid.Empty)
            throw new ArgumentException("Invalid feed ID", nameof(feedId));

        if (maxCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Max count must be greater than zero");

        return await _blogReaderProvider.GetArticlesFromFeed(feedId, maxCount);
    }
} 