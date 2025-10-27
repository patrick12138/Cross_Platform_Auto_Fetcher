import requests
import json
from typing import Dict, List, Optional, Any
import time

class KugouAPI:
    """酷狗音乐API客户端，用于获取歌单和排行榜数据"""
    
    def __init__(self, timeout: int = 10):
        """
        初始化酷狗API客户端
        
        Args:
            timeout: 请求超时时间（秒）
        """
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
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
            response.raise_for_status()  # 检查HTTP状态码
            
            # 检查响应内容是否为空
            if not response.text.strip():
                print(f"警告: API返回空内容 - {url}")
                return None
                
            return response.json()
        except requests.exceptions.RequestException as e:
            print(f"请求错误: {e} - {url}")
            return None
        except json.JSONDecodeError as e:
            print(f"JSON解析错误: {e} - {url}")
            print(f"响应内容: {response.text[:200]}...")
            return None
    
    def get_playlist(self, playlist_id: int) -> Optional[Dict[str, Any]]:
        """
        获取歌单/排行榜数据
        
        Args:
            playlist_id: 歌单ID或排行榜ID
            
        Returns:
            包含歌单信息和歌曲列表的字典，失败时返回None
        """
        # 使用原始API端点（可能需要更新）
        url = f'https://m.kugou.com/plist/list/{playlist_id}?json=true'
        data = self._make_request(url)
        
        if not data:
            return None
        
        try:
            # 获取歌单信息
            info = data.get('info', {}).get('list', {})
            
            # 获取歌曲列表
            songs = []
            song_list = data.get('list', {}).get('list', {}).get('info', [])
            
            for idx, item in enumerate(song_list, 1):
                # 获取详细歌曲信息
                song_hash = item.get('hash', '')
                if not song_hash:
                    continue
                    
                song_url = f"https://m.kugou.com/app/i/getSongInfo.php?cmd=playInfo&hash={song_hash}"
                song_data = self._make_request(song_url)
                
                if song_data:
                    songs.append({
                        '排名': idx,
                        '歌曲名': song_data.get('songName', ''),
                        '歌手': song_data.get('singerName', '未知'),
                        '专辑': song_data.get('albumName', ''),
                        'Hash': song_hash
                    })
                
                # 添加延时避免请求过快
                time.sleep(0.1)
            
            return {
                'title': info.get('specialname', '未知歌单'),
                'cover': info.get('imgurl', '').replace('{size}', '400') if info.get('imgurl') else '',
                'songs': songs
            }
            
        except Exception as e:
            print(f"解析歌单数据时出错: {e}")
            return None
    
    def close(self):
        """关闭会话"""
        self.session.close()

def main():
    """主函数，演示API使用"""
    kg = KugouAPI()
    
    try:
        # 获取酷狗飙升榜 (playlist_id=6666)
        print("正在获取酷狗飙升榜...")
        playlist = kg.get_playlist(6666)
        
        if playlist:
            print(f"排行榜: {playlist['title']}")
            print(f"歌曲数量: {len(playlist['songs'])}")
            print("\n前10首歌曲:")
            for song in playlist['songs'][:10]:
                print(f"{song['排名']}. {song['歌曲名']} - {song['歌手']}")
        else:
            print("获取排行榜失败")
            
    except Exception as e:
        print(f"程序运行出错: {e}")
    finally:
        kg.close()

if __name__ == "__main__":
    main()