using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrossPlatformAutoFetcher.Services.Log;

namespace CrossPlatformAutoFetcher.Services
{
    /// <summary>
    /// 音乐服务基类，提供通用重试机制。
    /// </summary>
    public abstract class MusicServiceBase : IMusicDataService
    {
        public abstract Task<List<Song>> GetTopListAsync(string topId, int limit = 100);

        public async Task<List<Song>> GetTopListWithRetryAsync(string topId, int limit = 100, int maxRetries = 3, int retryDelayMs = 1000)
        {
            var attempt = 0;
            List<Song> result = null;

            while (attempt < maxRetries)
            {
                attempt++;
                try
                {
                    FileLogger.Log($"[{GetType().Name}] 尝试第 {attempt}/{maxRetries} 次获取榜单 {topId}");

                    result = await GetTopListAsync(topId, limit);
                    if (result != null && result.Count > 0)
                    {
                        FileLogger.Log($"[{GetType().Name}] 成功获取 {result.Count} 首歌曲");
                        return result;
                    }

                    FileLogger.Log($"[{GetType().Name}] 第 {attempt} 次未获取到数据");

                    if (attempt < maxRetries)
                    {
                        FileLogger.Log($"[{GetType().Name}] 等待 {retryDelayMs}ms 后重试");
                        await Task.Delay(retryDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Log($"[{GetType().Name}] 第 {attempt} 次失败: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        FileLogger.Log($"[{GetType().Name}] 等待 {retryDelayMs}ms 后重试");
                        await Task.Delay(retryDelayMs);
                    }
                    else
                    {
                        FileLogger.Log($"[{GetType().Name}] 所有重试均失败");
                        throw;
                    }
                }
            }

            FileLogger.Log($"[{GetType().Name}] 重试 {maxRetries} 次后仍未获取到数据");
            return result ?? new List<Song>();
        }
    }
}
