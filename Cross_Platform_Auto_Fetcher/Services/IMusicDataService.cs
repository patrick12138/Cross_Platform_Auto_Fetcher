using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cross_Platform_Auto_Fetcher
{
    public interface IMusicDataService
    {
        Task<List<Song>> GetTopListAsync(string topId, int limit = 100);

        /// <summary>
        /// 带重试机制的榜单获取方法
        /// </summary>
        /// <param name="topId">榜单ID</param>
        /// <param name="limit">限制数量</param>
        /// <param name="maxRetries">最大重试次数（默认3次）</param>
        /// <param name="retryDelayMs">重试延迟毫秒数（默认1000ms）</param>
        /// <returns>歌曲列表，如果所有重试都失败则返回空列表</returns>
        Task<List<Song>> GetTopListWithRetryAsync(string topId, int limit = 100, int maxRetries = 3, int retryDelayMs = 1000);
    }
}
