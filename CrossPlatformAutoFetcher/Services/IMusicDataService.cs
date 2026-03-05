using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossPlatformAutoFetcher
{
    public interface IMusicDataService
    {
        Task<List<Song>> GetTopListAsync(string topId, int limit = 100);

        /// <summary>
        /// 带重试机制的榜单获取方法。
        /// </summary>
        /// <param name="topId">榜单 ID。</param>
        /// <param name="limit">限制数量。</param>
        /// <param name="maxRetries">最大重试次数（默认 3 次）。</param>
        /// <param name="retryDelayMs">重试延迟毫秒数（默认 1000ms）。</param>
        /// <returns>歌曲列表；如果重试后仍失败则返回空列表。</returns>
        Task<List<Song>> GetTopListWithRetryAsync(string topId, int limit = 100, int maxRetries = 3, int retryDelayMs = 1000);
    }
}
