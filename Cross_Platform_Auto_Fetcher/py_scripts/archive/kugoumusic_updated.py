import requests
import json
import re
from typing import Dict, List, Optional, Any
import time

class KugouAPI:
    """酷狗音乐API客户端，基于官方API结构实现"""
    
    def __init__(self, timeout: int = 10):
        """
        初始化酷狗API客户端
        
        Args:
            timeout: 请求超时时间（秒）
        """
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Referer': 'https://www.kugou.com/'
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
            return None
    
    def get_song_info(self, hash_value: str) -> Optional[Dict[str, Any]]:
        """
        获取歌曲详细信息
        
        Args:
            hash_value: 歌曲hash值
            
        Returns:
            歌曲信息字典，失败时返回None
        """
        url = f"https://m.kugou.com/app/i/getSongInfo.php?cmd=playInfo&hash={hash_value}"
        return self._make_request(url)
    
    def get_album_info(self, album_id: str) -> Optional[Dict[str, Any]]:
        """
        获取专辑信息
        
        Args:
            album_id: 专辑ID
            
        Returns:
            专辑信息字典，失败时返回None
        """
        url = f"http://mobilecdnbj.kugou.com/api/v3/album/info?albumid={album_id}"
        return self._make_request(url)
    
    def get_playlist(self, playlist_id: str) -> Optional[Dict[str, Any]]:
        """
        获取歌单信息
        
        Args:
            playlist_id: 歌单ID
            
        Returns:
            歌单信息字典，失败时返回None
        """
        url = f"https://m.kugou.com/plist/list/{playlist_id}?json=true"
        return self._make_request(url)
    
    def get_rank_list(self, rank_id: str = "1", page: int = 1) -> Optional[List[Dict[str, Any]]]:
        """
        获取排行榜列表
        
        Args:
            rank_id: 排行榜ID (1=飙升榜, 2=新歌榜, 等)
            page: 页码
            
        Returns:
            歌曲列表，失败时返回None
        """
        # 基于JavaScript代码中的排行榜URL结构
        url = f"https://www.kugou.com/yy/rank/home/{rank_id}-8888.html"
        
        try:
            response = self.session.get(url, timeout=self.timeout)
            response.raise_for_status()
            
            # 从HTML中提取歌曲信息
            html_content = response.text
            
            # 查找包含歌曲数据的脚本
            pattern = r'global\.features\s*=\s*({.*?});'
            match = re.search(pattern, html_content, re.DOTALL)
            
            if match:
                try:
                    data = json.loads(match.group(1))
                    # 解析歌曲数据
                    songs = []
                    # 这里需要根据实际数据结构来解析
                    return songs
                except json.JSONDecodeError:
                    print("解析排行榜数据失败")
                    return None
            
            # 备用方法：尝试从其他模式提取数据
            song_pattern = r'"SongName":"([^"]+).*?"SingerName":"([^"]+).*?"FileHash":"([^"]+)"'
            matches = re.findall(song_pattern, html_content)
            
            if matches:
                songs = []
                for idx, (song_name, singer_name, file_hash) in enumerate(matches, 1):
                    songs.append({
                        '排名': idx,
                        '歌曲名': song_name,
                        '歌手': singer_name,
                        'Hash': file_hash
                    })
                return songs
            
            print("未找到排行榜数据")
            return None
            
        except Exception as e:
            print(f"获取排行榜失败: {e}")
            return None
    
    def get_rank_detail(self, rank_id: str = "6666", limit: int = 100) -> Optional[Dict[str, Any]]:
        """
        获取排行榜详细信息（包含歌曲详情）
        
        Args:
            rank_id: 排行榜ID
            limit: 获取歌曲数量限制
            
        Returns:
            排行榜详细信息，失败时返回None
        """
        # 首先获取排行榜基本信息
        playlist_data = self.get_playlist(rank_id)
        if not playlist_data:
            return None
        
        try:
            info = playlist_data.get('info', {}).get('list', {})
            song_list = playlist_data.get('list', {}).get('list', {}).get('info', [])
            
            songs = []
            for idx, item in enumerate(song_list[:limit], 1):
                song_hash = item.get('hash', '')
                if not song_hash:
                    continue
                
                # 获取歌曲详细信息
                song_info = self.get_song_info(song_hash)
                if song_info:
                    songs.append({
                        '排名': idx,
                        '歌曲名': song_info.get('songName', ''),
                        '歌手': song_info.get('singerName', '未知'),
                        '专辑': song_info.get('albumName', ''),
                        'Hash': song_hash,
                        '时长': song_info.get('duration', 0),
                        '比特率': song_info.get('bitRate', 0)
                    })
                
                # 添加延时避免请求过快
                time.sleep(0.1)
            
            return {
                'title': info.get('specialname', '未知排行榜'),
                'cover': info.get('imgurl', '').replace('{size}', '400') if info.get('imgurl') else '',
                'songs': songs
            }
            
        except Exception as e:
            print(f"解析排行榜详细信息时出错: {e}")
            return None
    
    def search(self, keyword: str, page: int = 1, search_type: int = 0) -> Optional[Dict[str, Any]]:
        """
        搜索音乐
        
        Args:
            keyword: 搜索关键词
            page: 页码
            search_type: 搜索类型 (0=歌曲, 1=歌单)
            
        Returns:
            搜索结果，失败时返回None
        """
        if search_type == 1:  # 搜索歌单
            url = f"http://mobilecdnbj.kugou.com/api/v3/search/special"
            params = {
                'keyword': keyword,
                'pagesize': 20,
                'filter': 0,
                'page': page
            }
        else:  # 搜索歌曲
            url = "https://songsearch.kugou.com/song_search_v2"
            params = {
                'keyword': keyword,
                'page': page
            }
        
        result = self._make_request(url, params)
        if not result:
            return None
        
        try:
            if search_type == 1:
                # 解析歌单搜索结果
                info_list = result.get('data', {}).get('info', [])
                playlists = []
                for item in info_list:
                    playlists.append({
                        'id': f"kgplaylist_{item.get('specialid', '')}",
                        'title': item.get('specialname', ''),
                        'cover': item.get('imgurl', '').replace('{size}', '400') if item.get('imgurl') else '',
                        'author': item.get('nickname', ''),
                        'count': item.get('songcount', 0)
                    })
                return {
                    'result': playlists,
                    'total': result.get('data', {}).get('total', 0),
                    'type': 'playlist'
                }
            else:
                # 解析歌曲搜索结果
                song_list = result.get('data', {}).get('lists', [])
                songs = []
                for item in song_list:
                    songs.append({
                        'SongName': item.get('SongName', ''),
                        'SingerName': item.get('SingerName', ''),
                        'AlbumName': item.get('AlbumName', ''),
                        'FileHash': item.get('FileHash', ''),
                        'Duration': item.get('Duration', 0)
                    })
                return {
                    'result': songs,
                    'total': result.get('data', {}).get('total', 0),
                    'type': 'song'
                }
        except Exception as e:
            print(f"解析搜索结果时出错: {e}")
            return None
    
    def close(self):
        """关闭会话"""
        self.session.close()

def main():
    """主函数，演示API使用"""
    kg = KugouAPI()
    
    try:
        print("=== 酷狗音乐API测试 ===\n")
        
        # 测试1: 获取排行榜
        print("1. 测试获取排行榜...")
        rank_data = kg.get_rank_detail("6666", 5)  # 只获取前5首
        if rank_data:
            print(f"排行榜: {rank_data['title']}")
            print(f"歌曲数量: {len(rank_data['songs'])}")
            for song in rank_data['songs']:
                print(f"  {song['排名']}. {song['歌曲名']} - {song['歌手']}")
        else:
            print("获取排行榜失败")
        
        print("\n" + "="*50 + "\n")
        
        # 测试2: 搜索功能
        print("2. 测试搜索功能...")
        search_result = kg.search("林俊杰", 1, 0)
        if search_result and search_result.get('result'):
            print(f"搜索结果总数: {search_result.get('total', 0)}")
            print("前3首歌曲:")
            for song in search_result['result'][:3]:
                print(f"  {song.get('SongName', '')} - {song.get('SingerName', '')}")
        else:
            print("搜索失败")
        
        print("\n" + "="*50 + "\n")
        
        # 测试3: 获取歌单
        print("3. 测试获取歌单...")
        playlist_data = kg.get_playlist("313917")  # 一个示例歌单ID
        if playlist_data:
            info = playlist_data.get('info', {}).get('list', {})
            print(f"歌单名称: {info.get('specialname', '未知')}")
            song_count = len(playlist_data.get('list', {}).get('list', {}).get('info', []))
            print(f"歌曲数量: {song_count}")
        else:
            print("获取歌单失败")
            
    except Exception as e:
        print(f"程序运行出错: {e}")
    finally:
        kg.close()

if __name__ == "__main__":
    main()