import requests
import re

def test_kugou_urls():
    """测试酷狗音乐URL"""
    headers = {
        'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
    }
    
    urls = [
        'https://www.kugou.com/yy/html/rank.html',
        'https://www.kugou.com/yy/rank/home/1-8888.html?from=rank'
    ]
    
    for url in urls:
        print(f"\n测试URL: {url}")
        try:
            response = requests.get(url, headers=headers, timeout=10)
            print(f"状态码: {response.status_code}")
            print(f"内容类型: {response.headers.get('content-type', 'unknown')}")
            print(f"内容长度: {len(response.text)}")
            
            # 查找可能的API端点或数据
            if 'json' in response.text.lower():
                print("发现JSON内容")
            if 'api' in response.text.lower():
                print("发现API相关内容")
            
            # 查找排行榜ID模式
            rank_ids = re.findall(r'rank[/\w-]*(\d+)[/\w-]*\.html', response.text)
            if rank_ids:
                print(f"发现排行榜ID: {rank_ids[:5]}")  # 只显示前5个
                
            # 显示前200字符
            print(f"前200字符: {response.text[:200]}")
            
        except Exception as e:
            print(f"请求失败: {e}")

if __name__ == "__main__":
    test_kugou_urls()