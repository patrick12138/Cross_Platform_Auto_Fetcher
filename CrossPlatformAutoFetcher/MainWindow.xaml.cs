using CrossPlatformAutoFetcher.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CrossPlatformAutoFetcher
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Dictionary<string, string>> _platformCharts = new();
        private IMusicDataService _musicService;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            _platformCharts.Add("网易云音乐", new Dictionary<string, string>
            {
                { "热歌榜", "3778678" },
                { "新歌榜", "3779629" },
                { "飙升榜", "19723756" }
            });

            _platformCharts.Add("QQ音乐", new Dictionary<string, string>
            {
                { "热歌榜", "26" },
                { "新歌榜", "27" },
                { "飙升榜", "62" }
            });

            _platformCharts.Add("酷狗音乐", new Dictionary<string, string>
            {
                { "TOP500榜", "8888" },
                { "飙升榜", "6666" }
            });

            PlatformComboBox.ItemsSource = _platformCharts.Keys;
            PlatformComboBox.SelectedIndex = 0;
        }

        private void PlatformComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlatformComboBox.SelectedItem == null)
            {
                return;
            }

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

            _musicService = CreateMusicService(selectedPlatform);
            if (_musicService == null)
            {
                StatusTextBlock.Text = "暂不支持该平台";
                return;
            }

            SetButtonsEnabled(false);
            StatusTextBlock.Text = $"正在抓取 {selectedPlatform} - {selectedChart}...";
            SongsDataGrid.ItemsSource = null;

            try
            {
                var songs = await _musicService.GetTopListWithRetryAsync(chartId, 100, maxRetries: 3, retryDelayMs: 2000);

                if (songs.Count > 0)
                {
                    SongsDataGrid.ItemsSource = songs;
                    StatusTextBlock.Text = $"抓取完成，共获取 {songs.Count} 首歌曲";
                }
                else
                {
                    SongsDataGrid.ItemsSource = new List<Song>
                    {
                        new Song
                        {
                            Rank = 0,
                            Title = "未获取到数据",
                            Artist = "请稍后重试或检查网络连接",
                            Album = string.Empty
                        }
                    };
                    StatusTextBlock.Text = "未获取到数据，请稍后重试";
                }
            }
            catch (Exception ex)
            {
                SongsDataGrid.ItemsSource = new List<Song>
                {
                    new Song
                    {
                        Rank = 0,
                        Title = "抓取失败",
                        Artist = ex.Message,
                        Album = ex.GetType().Name
                    }
                };
                StatusTextBlock.Text = $"发生错误: {ex.Message}";
            }
            finally
            {
                SetButtonsEnabled(true);
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
            var service = CreateMusicService(selectedPlatform);
            if (service == null)
            {
                StatusTextBlock.Text = "暂不支持该平台";
                return;
            }

            SetButtonsEnabled(false);

            var exportBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            var timestampFolder = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(timestampFolder);

            StatusTextBlock.Text = $"开始导出 {selectedPlatform} 的所有榜单...";

            try
            {
                var charts = _platformCharts[selectedPlatform];
                var successCount = 0;
                var totalCount = charts.Count;

                foreach (var chartEntry in charts)
                {
                    var chartName = chartEntry.Key;
                    var chartId = chartEntry.Value;

                    StatusTextBlock.Text = $"正在导出 ({successCount + 1}/{totalCount}): {selectedPlatform} - {chartName}...";

                    var songs = await service.GetTopListWithRetryAsync(chartId, 100);
                    if (songs.Count == 0)
                    {
                        StatusTextBlock.Text = $"{selectedPlatform} - {chartName} 未获取到数据，跳过";
                        await Task.Delay(500);
                        continue;
                    }

                    var csvContent = GenerateCsvContent(songs);
                    var fileName = $"{selectedPlatform}_{chartName}.csv";
                    var filePath = Path.Combine(timestampFolder, fileName);
                    await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
                    successCount++;
                }

                StatusTextBlock.Text = $"{selectedPlatform} 导出完成，成功导出 {successCount}/{totalCount} 个榜单\n目录: {timestampFolder}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"导出过程中发生错误: {ex.Message}";
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private async void ExportAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsEnabled(false);

            var exportBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            var timestampFolder = Path.Combine(exportBasePath, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(timestampFolder);

            StatusTextBlock.Text = "开始导出所有平台的全部榜单...";

            try
            {
                var totalSuccess = 0;
                var totalCharts = 0;

                foreach (var platformEntry in _platformCharts)
                {
                    totalCharts += platformEntry.Value.Count;
                }

                foreach (var platformEntry in _platformCharts)
                {
                    var platformName = platformEntry.Key;
                    var service = CreateMusicService(platformName);
                    if (service == null)
                    {
                        continue;
                    }

                    foreach (var chartEntry in platformEntry.Value)
                    {
                        var chartName = chartEntry.Key;
                        var chartId = chartEntry.Value;

                        StatusTextBlock.Text = $"正在导出 ({totalSuccess + 1}/{totalCharts}): {platformName} - {chartName}...";

                        var songs = await service.GetTopListWithRetryAsync(chartId, 100);
                        if (songs.Count == 0)
                        {
                            StatusTextBlock.Text = $"{platformName} - {chartName} 未获取到数据，跳过";
                            await Task.Delay(500);
                            continue;
                        }

                        var csvContent = GenerateCsvContent(songs);
                        var fileName = $"{platformName}_{chartName}.csv";
                        var filePath = Path.Combine(timestampFolder, fileName);
                        await File.WriteAllTextAsync(filePath, csvContent, Encoding.UTF8);
                        totalSuccess++;
                    }
                }

                StatusTextBlock.Text = $"全部导出完成，成功导出 {totalSuccess}/{totalCharts} 个榜单\n目录: {timestampFolder}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"导出过程中发生错误: {ex.Message}";
            }
            finally
            {
                SetButtonsEnabled(true);
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
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }

            return text;
        }

        private IMusicDataService CreateMusicService(string platformName)
        {
            return platformName switch
            {
                "QQ音乐" => new QQMusicService(),
                "酷狗音乐" => new KugouMusicService(),
                "网易云音乐" => new NeteaseMusicService(),
                _ => null
            };
        }

        private void SetButtonsEnabled(bool enabled)
        {
            FetchButton.IsEnabled = enabled;
            ExportButton.IsEnabled = enabled;
            ExportAllButton.IsEnabled = enabled;
        }
    }
}
