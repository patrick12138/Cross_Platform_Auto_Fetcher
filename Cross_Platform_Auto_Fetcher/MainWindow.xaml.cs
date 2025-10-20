using System;
using System.Collections.Generic;
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
                default:
                    StatusTextBlock.Text = "暂不支持该平台";
                    return;
            }

            FetchButton.IsEnabled = false;
            StatusTextBlock.Text = $"正在抓取 {selectedPlatform} - {selectedChart}...";
            SongsDataGrid.ItemsSource = null;

            try
            {
                var songs = await _musicService.GetTopListAsync(chartId, 100); // MVP: Limit to 100
                SongsDataGrid.ItemsSource = songs;
                StatusTextBlock.Text = $"抓取完成！共获取 {songs.Count} 首歌曲。";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"发生错误: {ex.Message}";
            }
            finally
            {
                FetchButton.IsEnabled = true;
            }
        }
    }
}