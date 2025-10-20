using Cross_Platform_Auto_Fetcher.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Cross_Platform_Auto_Fetcher
{
    public partial class MainWindow : Window
    {
        // 存储平台及其对应的榜单信息 <PlatformName, <ChartName, ChartId>>
        private readonly Dictionary<string, Dictionary<string, string>> _platformCharts = new Dictionary<string, Dictionary<string, string>>();
        private IMusicDataService _musicService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            // 初始化QQ音乐榜单
            _platformCharts.Add("QQ音乐", new Dictionary<string, string>
            {
                { "热歌榜", "26" },
                { "新歌榜", "27" },
                { "飙升榜", "62" }
            });

            // 初始化酷狗音乐榜单
            _platformCharts.Add("酷狗音乐", new Dictionary<string, string>
            {
                { "TOP500榜", "8888" },
                { "飙升榜", "6666" }
            });

            // 初始化网易云音乐榜单
            _platformCharts.Add("网易云音乐", new Dictionary<string, string>
            {
                { "热歌榜", "3778678" },
                { "新歌榜", "3779629" },
                { "飙升榜", "19723756" }
            });

            // 填充平台选择框
            PlatformComboBox.ItemsSource = _platformCharts.Keys;
            PlatformComboBox.SelectedIndex = 0;
        }

        private void PlatformComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlatformComboBox.SelectedItem == null) return;

            var selectedPlatform = PlatformComboBox.SelectedItem.ToString();
            if (_platformCharts.ContainsKey(selectedPlatform))
            {
                ChartComboBox.ItemsSource = _platformCharts[selectedPlatform].Keys;
                ChartComboBox.SelectedIndex = 0;
            }
        }

        private async void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlatformComboBox.SelectedItem == null || ChartComboBox.SelectedItem == null)
            {
                StatusTextBlock.Text = "请先选择平台和榜单";
                return;
            }

            var selectedPlatform = PlatformComboBox.SelectedItem.ToString();
            var selectedChart = ChartComboBox.SelectedItem.ToString();
            var chartId = _platformCharts[selectedPlatform][selectedChart];

            // 根据选择实例化对应的服务
            switch (selectedPlatform)
            {
                case "QQ音乐":
                    _musicService = new QQMusicService();
                    break;
                case "酷狗音乐":
                    _musicService = new KugouMusicService();
                    break;
                case "网易云音乐":
                    _musicService = new NeteaseMusicService();
                    break;
                default:
                    StatusTextBlock.Text = "暂不支持该平台";
                    return;
            }

            FetchButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            ExportAllButton.IsEnabled = false;
            StatusTextBlock.Text = $"正在抓取 {selectedPlatform} - {selectedChart}..." ;
            SongsDataGrid.ItemsSource = null;

            try
            {
                // 使用带重试机制的方法，特别是针对网易云音乐
                var songs = await _musicService.GetTopListWithRetryAsync(chartId, 100, maxRetries: 3, retryDelayMs: 2000);
                SongsDataGrid.ItemsSource = songs;

                if (songs.Count > 0)
                {
                    StatusTextBlock.Text = $"抓取完成！共获取 {songs.Count} 首歌曲。";
                }
                else
                {
                    StatusTextBlock.Text = $"抓取失败或无数据，已重试3次。请查看日志文件了解详情。";
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"发生错误: {ex.Message}" ;
            }
            finally
            {
                FetchButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                ExportAllButton.IsEnabled = true;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlatformComboBox.SelectedItem == null)
            {
                StatusTextBlock.Text = "请先选择平台";
                return;
            }

            var selectedPlatform = PlatformComboBox.SelectedItem.ToString();

            FetchButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            ExportAllButton.IsEnabled = false;

            var exportBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            var timestampFolder = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(timestampFolder);

            StatusTextBlock.Text = $"开始导出 {selectedPlatform} 的所有榜单...";

            try
            {
                IMusicDataService service = null;
                switch (selectedPlatform)
                {
                    case "QQ音乐":
                        service = new QQMusicService();
                        break;
                    case "酷狗音乐":
                        service = new KugouMusicService();
                        break;
                    case "网易云音乐":
                        service = new NeteaseMusicService();
                        break;
                }

                if (service == null)
                {
                    StatusTextBlock.Text = "暂不支持该平台";
                    return;
                }

                var charts = _platformCharts[selectedPlatform];
                foreach (var chartEntry in charts)
                {
                    var chartName = chartEntry.Key;
                    var chartId = chartEntry.Value;

                    StatusTextBlock.Text = $"正在导出: {selectedPlatform} - {chartName}...";

                    // 使用带重试机制的方法
                    var songs = await service.GetTopListWithRetryAsync(chartId, 100);

                    if (songs.Count == 0)
                    {
                        StatusTextBlock.Text = $"警告: {selectedPlatform} - {chartName} 未获取到数据，跳过...";
                        continue;
                    }

                    var csvContent = GenerateCsvContent(songs);
                    var fileName = $"{selectedPlatform}_{chartName}.csv";
                    var filePath = Path.Combine(timestampFolder, fileName);
                    await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
                }

                StatusTextBlock.Text = $"{selectedPlatform} 导出完成！文件已保存至 {timestampFolder}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"导出过程中发生错误: {ex.Message}";
            }
            finally
            {
                FetchButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                ExportAllButton.IsEnabled = true;
            }
        }

        private async void ExportAllButton_Click(object sender, RoutedEventArgs e)
        {
            FetchButton.IsEnabled = false;
            ExportButton.IsEnabled = false;
            ExportAllButton.IsEnabled = false;

            var exportBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            var timestampFolder = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(timestampFolder);

            StatusTextBlock.Text = "开始导出所有榜单...";

            try
            {
                foreach (var platformEntry in _platformCharts)
                {
                    var platformName = platformEntry.Key;
                    IMusicDataService service = null;
                    switch (platformName)
                    {
                        case "QQ音乐":
                            service = new QQMusicService();
                            break;
                        case "酷狗音乐":
                            service = new KugouMusicService();
                            break;
                        case "网易云音乐":
                            service = new NeteaseMusicService();
                            break;
                    }

                    if (service == null) continue;

                    foreach (var chartEntry in platformEntry.Value)
                    {
                        var chartName = chartEntry.Key;
                        var chartId = chartEntry.Value;

                        StatusTextBlock.Text = $"正在导出: {platformName} - {chartName}..." ;

                        // 使用带重试机制的方法
                        var songs = await service.GetTopListWithRetryAsync(chartId, 100);

                        if (songs.Count == 0)
                        {
                            StatusTextBlock.Text = $"警告: {platformName} - {chartName} 未获取到数据，跳过...";
                            continue;
                        }

                        var csvContent = GenerateCsvContent(songs);
                        var fileName = $"{platformName}_{chartName}.csv" ;
                        var filePath = Path.Combine(timestampFolder, fileName);
                        await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
                    }
                }

                StatusTextBlock.Text = $"全部导出完成！文件已保存至 {timestampFolder}" ;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"导出过程中发生错误: {ex.Message}" ;
            }
            finally
            {
                FetchButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                ExportAllButton.IsEnabled = true;
            }
        }

        private string GenerateCsvContent(List<Song> songs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("排名,歌名,歌手,专辑");

            foreach (var song in songs)
            {
                sb.AppendLine($"{song.Rank},{SanitizeForCsv(song.Title)},{SanitizeForCsv(song.Artist)},{SanitizeForCsv(song.Album)}");
            }

            return sb.ToString();
        }

        private string SanitizeForCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
            {
                // Use simple concatenation to avoid complex interpolation issues
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            return text;
        }
    }
}