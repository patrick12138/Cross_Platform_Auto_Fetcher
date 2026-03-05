import requests
import json
import re
from typing import Dict, List, Optional, Any

class KugouAPI:
    """酷狗音乐API客户端，通过解析页面内嵌JSON获取排行榜"""

    def __init__(self, timeout: int = 15):
        self.headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36',
        }
        self.timeout = timeout
        self.session = requests.Session()
        self.session.headers.update(self.headers)

    def _fetch_html(self, url: str) -> Optional[str]:
        """安全的HTTP GET请求方法，用于获取HTML页面内容"""
        try:
            response = self.session.get(url, timeout=self.timeout)
            response.raise_for_status()
            return response.text
        except requests.exceptions.RequestException as e:
            print(f"请求HTML页面时出错: {e} - {url}")
            return None

    def get_toplist(self, rank_id: int) -> Optional[Dict[str, Any]]:
        """
        获取排行榜数据，通过解析页面内嵌的JSON

        Args:
            rank_id: 排行榜ID (例如 8888 for TOP500, 6666 for 飙升榜)

        Returns:
            包含排行榜信息和歌曲列表的字典，失败时返回None
        """
        url = f"https://www.kugou.com/yy/rank/home/1-{rank_id}.html"
        
        html_content = self._fetch_html(url)
        if not html_content:
            return None

        # 目标是 global.features = [...] 中的 [...]
        # 正确转义了方括号
        match = re.search(r'global.features\s*=\s*\[(.*?)\];', html_content)
        if not match:
            print(f"在页面 {url} 中未找到global.features数据")
            return None

        try:
            # 提取歌曲列表JSON
            songs_json_str = match.group(1)
            song_list = json.loads(f'[{songs_json_str}]')

            # 提取榜单标题
            title_match = re.search(r'<title>(.*?)_排行榜', html_content)
            title = title_match.group(1) if title_match else '未知榜单'

            songs = []
            for idx, item in enumerate(song_list, 1):
                filename = item.get('FileName', ' - ')
                parts = filename.split(' - ', 1)
                singer = parts[0].strip()
                song_name = parts[1].strip() if len(parts) > 1 else filename

                songs.append({
                    '排名': idx,
                    '歌曲名': song_name,
                    '歌手': singer,
                    '专辑': item.get('album_name', '未知专辑'),
                    'Hash': item.get('Hash', ''),
                    '时长': item.get('timeLen', 0)
                })
            
            return {
                'title': title,
                'total': len(songs),
                'songs': songs
            }

        except (json.JSONDecodeError, IndexError, Exception) as e:
            print(f"解析页面数据时出错: {e}")
            return None

    def close(self):
        self.session.close()

def main():
    """主函数，演示API使用"""
    kg_api = KugouAPI()

    toplists_to_test = {
        "酷狗TOP500榜": 8888,
        "酷狗飙升榜": 6666,
    }

    try:
        for name, rank_id in toplists_to_test.items():
            print(f"--- 正在获取 {name} (ID: {rank_id}) ---")
            toplist = kg_api.get_toplist(rank_id)
            
            if toplist and toplist['songs']:
                print(f"榜单名称: {toplist['title']}")
                print(f"榜单歌曲总数: {toplist['total']}")
                print(f"成功获取歌曲数量: {len(toplist['songs'])}")
                print("前5首歌曲:")
                for song in toplist['songs'][:5]:
                    print(f"  {song['排名']}. {song['歌曲名']} - {song['歌手']}")
            else:
                print(f"获取 {name} 失败")

            print("\n" + "="*50 + "\n")

    except Exception as e:
        print(f"程序运行出错: {e}")
    finally:
        print("酷狗API测试完成。")
        kg_api.close()

if __name__ == "__main__":
    main()
