using Cross_Platform_Auto_Fetcher.Services;
using System;
using System.Threading.Tasks;

namespace Cross_Platform_Auto_Fetcher
{
    public static class DebugTester
    {
        public static async Task RunNeteaseTest()
        {
            UILogger.Log("=============================================");
            UILogger.Log("           正在开始网易云测试");
            UILogger.Log("=============================================");

            var neteaseService = new NeteaseMusicService();
            var chartId = "3778678"; // 热歌榜

            UILogger.Log($"正在尝试获取榜单ID: {chartId}");

            var songs = await neteaseService.GetTopListAsync(chartId, 100);

            UILogger.Log("=============================================");
            UILogger.Log($"            测试结束");
            UILogger.Log($"返回歌曲数: {songs.Count}");
            UILogger.Log("=============================================");
        }
    }
}
