using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cross_Platform_Auto_Fetcher.Services.Log;

namespace Cross_Platform_Auto_Fetcher.Services
{
    /// <summary>
    /// 音乐服务基类，提供通用的重试机制
    /// </summary>
    public abstract class MusicServiceBase : IMusicDataService
    {
        public abstract Task<List<Song>> GetTopListAsync(string topId, int limit = 100);

        public async Task<List<Song>> GetTopListWithRetryAsync(string topId, int limit = 100, int maxRetries = 3, int retryDelayMs = 1000)
        {
            int attempt = 0;
            List<Song> result = null;

            while (attempt < maxRetries)
            {
                attempt++;
                try
                {
                    FileLogger.Log($"[{GetType().Name}] 尝试第 {attempt}/{maxRetries} 次获取榜单 {topId}");

                    result = await GetTopListAsync(topId, limit);

                    // 检查是否成功获取到数据
                    if (result != null && result.Count > 0)
                    {
                        FileLogger.Log($"[{GetType().Name}] 成功获取 {result.Count} 首歌曲");
                        return result;
                    }

                    FileLogger.Log($"[{GetType().Name}] 第 {attempt} 次尝试未获取到数据");

                    // 如果还有重试机会，等待后重试
                    if (attempt < maxRetries)
                    {
                        FileLogger.Log($"[{GetType().Name}] 等待 {retryDelayMs}ms 后重试...");
                        await Task.Delay(retryDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Log($"[{GetType().Name}] 第 {attempt} 次尝试失败: {ex.Message}");

                    // 如果还有重试机会，等待后重试
                    if (attempt < maxRetries)
                    {
                        FileLogger.Log($"[{GetType().Name}] 等待 {retryDelayMs}ms 后重试...");
                        await Task.Delay(retryDelayMs);
                    }
                    else
                    {
                        // 最后一次尝试也失败了，抛出异常
                        FileLogger.Log($"[{GetType().Name}] 所有重试均失败");
                        throw;
                    }
                }
            }

            // 所有尝试都未获取到数据
            FileLogger.Log($"[{GetType().Name}] 重试 {maxRetries} 次后仍未获取到数据");
            return result ?? new List<Song>();
        }
    }
}
