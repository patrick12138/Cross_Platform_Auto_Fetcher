import requests
import json
import html
from typing import Dict, List, Optional, Any
import time
from datetime import datetime

class QQMusicAPI:
    """QQ音乐API客户端，用于获取排行榜数据"""
    
    def __init__(self, timeout: int = 10):
        """
        初始化QQ音乐API客户端
        
        Args:
            timeout: 请求超时时间（秒）
        """
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Referer': 'https://y.qq.com/'
        }
        self.timeout = timeout
        self.session = requests.Session()
        self.session.headers.update(self.headers)
    
    def _make_request(self, url: str, params: Optional[Dict] = None) -> Optional[Dict]:
        """
        安全的HTTP请求方法
        
        Args:
            url: 请求URL
            params: 请求参数
            
        Returns:
            响应JSON数据，失败时返回None
        """
        try:
            response = self.session.get(url, params=params, timeout=self.timeout)
            response.raise_for_status()
            
            if not response.text.strip():
                print(f"警告: API返回空内容 - {url}")
                return None
                
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"请求错误: {e} - {url}")
            return None
        except json.JSONDecodeError as e:
            print(f"JSON解析错误: {e} - {url}")
            # 尝试从文本中提取JSON
            match = re.search(r'\((.*?)\)', response.text)
            if match:
                try:
                    return json.loads(match.group(1))
                except json.JSONDecodeError:
                    print("提取后JSON解析仍然失败")
                    return None
            return None
    
    def get_toplist_period(self, topid: int) -> str:
        """
        获取排行榜周期信息。
        对于大多数日常更新的榜单，直接使用当前日期即可。
        
        Args:
            topid: 排行榜ID (此版本中未使用，但保留以兼容旧接口)
            
        Returns:
            当前日期的字符串，格式为 YYYY-MM-DD
        """
        return datetime.now().strftime('%Y-%m-%d')
    
    def get_toplist(self, topid: int, limit: int = 300) -> Optional[Dict[str, Any]]:
        """
        获取排行榜数据
        
        Args:
            topid: 排行榜ID
            limit: 获取歌曲数量限制
            
        Returns:
            包含排行榜信息和歌曲列表的字典，失败时返回None
        """
        period = self.get_toplist_period(topid)
        
        url = 'https://u.y.qq.com/cgi-bin/musicu.fcg'
        # 请求体现在更规范，直接从浏览器开发者工具中获取
        data = {
            "comm": {"cv": 4747474, "ct": 24, "format": "json", "inCharset": "utf-8", "outCharset": "utf-8", "notice": 0, "platform": "yqq.json", "needNewCode": 1, "uin": 0, "g_tk_new_20200303": 5381, "g_tk": 5381},
            "detail": {
                "module": "musicToplist.ToplistInfoServer",
                "method": "GetDetail",
                "param": {
                    "topId": topid,
                    "offset": 0,
                    "num": limit,
                    "period": period
                }
            }
        }
        
        # 移除旧的、复杂的参数构造，使用更简洁的方式
        params = {
            '_': str(int(time.time() * 1000)),
            'data': json.dumps(data)
        }
        
        result = self._make_request(url, params=params)
        if not result:
            return None
        
        try:
            # 注意：API返回的数据结构有嵌套的data字段
            toplist_data = result.get('detail', {}).get('data', {}).get('data', {})
            # API数据结构已更新，歌曲信息现在在'song'数组中，而不是'songInfoList'
            song_info_list = toplist_data.get('song', [])
            # 如果song为空，尝试旧的songInfoList字段（向后兼容）
            if not song_info_list:
                song_info_list = toplist_data.get('songInfoList', [])
            
            title = toplist_data.get('title', '未知排行榜')
            
            songs = []
            for idx, song in enumerate(song_info_list, 1):
                try:
                    # 新数据结构中歌手信息在singerName字段中，而不是singer数组
                    singer_name = song.get('singerName', '未知歌手')
                    # 如果singerName不存在，尝试从singer数组中获取（向后兼容）
                    if not singer_name and song.get('singer'):
                        singer_name = ' & '.join([s.get('name', '未知歌手') for s in song.get('singer', [])])
                    
                    # 新数据结构中没有专辑信息，使用空字符串
                    album_name = ''  # 新API结构中不包含专辑信息
                    
                    songs.append({
                        '排名': idx,
                        '歌曲名': self.html_decode(song.get('title', '')),  # 新API使用title字段
                        '歌手': self.html_decode(singer_name),
                        '专辑': album_name,
                        '歌曲ID': song.get('songId', '')  # 新API使用songId字段
                    })
                except Exception as e:
                    print(f"解析歌曲信息时出错 (第{idx}首): {e}")
                    continue
            
            return {
                'title': self.html_decode(title),
                'songs': songs
            }
            
        except Exception as e:
            print(f"解析排行榜数据时出错: {e}")
            return None
    
    @staticmethod
    def html_decode(text: str) -> str:
        """HTML解码"""
        if not text:
            return ""
        return html.unescape(text)
    
    def close(self):
        """关闭会话"""
        self.session.close()

def main():
    """主函数，演示API使用"""
    qq = QQMusicAPI()
    
    # 定义要测试的榜单
    toplists_to_test = {
        "飙升榜": 62,
        "热歌榜": 26,
        "新歌榜": 27,
    }
    
    try:
        for name, topid in toplists_to_test.items():
            print(f"正在获取QQ音乐 {name} (ID: {topid})...")
            # 尝试获取最多300首
            toplist = qq.get_toplist(topid, limit=300) 
            
            if toplist:
                print(f"排行榜: {toplist['title']}")
                print(f"成功获取歌曲数量: {len(toplist['songs'])}")
                print("\n前10首歌曲:")
                for song in toplist['songs'][:10]:
                    print(f"  {song['排名']}. {song['歌曲名']} - {song['歌手']}")
            else:
                print(f"获取 {name} 失败")
            print("\n" + "="*50 + "\n")
            
    except Exception as e:
        print(f"程序运行出错: {e}")
    finally:
        qq.close()

if __name__ == "__main__":
    main()
