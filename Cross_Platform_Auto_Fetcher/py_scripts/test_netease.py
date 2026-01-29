import requests
import json
import time
import base64
import binascii
import os
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad

# ------------------------------------------------------------------------------
# 1. 简单 GET API (目前稳定可用)
# ------------------------------------------------------------------------------
def fetch_netease_simple(playlist_id, name):
    url = f"http://music.163.com/api/playlist/detail?id={playlist_id}"
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Referer": "http://music.163.com/"
    }
    
    print(f"[Simple GET] 正在获取榜单: {name} (ID: {playlist_id})...")
    try:
        response = requests.get(url, headers=headers, timeout=10)
        if response.status_code == 200:
            data = response.json()
            if data['code'] == 200:
                result = data['result']
                tracks = result['tracks']
                print(f"[Simple GET] 成功获取 {name}。共 {len(tracks)} 首歌曲。")
                return True
            else:
                print(f"[Simple GET] API 返回错误代码: {data['code']}")
        else:
            print(f"[Simple GET] HTTP 请求失败: {response.status_code}")
    except Exception as e:
        print(f"[Simple GET] 发生异常: {e}")
    return False

# ------------------------------------------------------------------------------
# 2. V3 Weapi (需要 AES + RSA 加密)
# ------------------------------------------------------------------------------

# 移植自 NeteaseCrypto.cs 的常量
MODULUS = "00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7"
NONCE = "0CoJUm6Qyw8W8jud"
PUB_KEY = "010001"

def aes_encrypt(text, key):
    iv = "0102030405060708"
    cipher = AES.new(key.encode('utf-8'), AES.MODE_CBC, iv.encode('utf-8'))
    encrypted = cipher.encrypt(pad(text.encode('utf-8'), 16))
    return base64.b64encode(encrypted).decode('utf-8')

def rsa_encrypt(text, pubKey, modulus):
    text = text[::-1]
    rs = pow(int(binascii.hexlify(text.encode('utf-8')), 16), int(pubKey, 16), int(modulus, 16))
    return format(rs, 'x').zfill(256)

def create_secret_key(size):
    return binascii.hexlify(os.urandom(size // 2)).decode('utf-8')

def weapi(text):
    secret_key = create_secret_key(16)
    params = aes_encrypt(aes_encrypt(text, NONCE), secret_key)
    enc_sec_key = rsa_encrypt(secret_key, PUB_KEY, MODULUS)
    return {
        'params': params,
        'encSecKey': enc_sec_key
    }

def fetch_netease_v3_weapi(playlist_id, name):
    url = "https://music.163.com/weapi/v3/playlist/detail"
    
    # 构造 payload，完全模仿 C# 代码
    payload = {
        "id": str(playlist_id),
        "offset": 0,
        "total": True,
        "limit": 1000,
        "n": 1000,
        "csrf_token": ""
    }
    
    data = weapi(json.dumps(payload))
    
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Referer": "https://music.163.com/",
        "Content-Type": "application/x-www-form-urlencoded"
    }

    print(f"[V3 Weapi] 正在获取榜单: {name} (ID: {playlist_id})...")
    try:
        response = requests.post(url, headers=headers, data=data, timeout=10)
        if response.status_code == 200:
            json_resp = response.json()
            if json_resp.get('code') == 200:
                # 注意：V3 接口返回的结构通常是 playlist -> tracks
                if 'playlist' in json_resp and 'tracks' in json_resp['playlist']:
                    tracks = json_resp['playlist']['tracks']
                    print(f"[V3 Weapi] 成功获取 {name}。共 {len(tracks)} 首歌曲。")
                    # 打印第一首歌用于字段名对比 (ar/al vs artists/album)
                    if len(tracks) > 0:
                        first = tracks[0]
                        print("  [V3 Debug] First Track Keys:", list(first.keys()))
                        if 'ar' in first: print("  [V3 Debug] Has 'ar' field (Artists)")
                        if 'al' in first: print("  [V3 Debug] Has 'al' field (Album)")
                    return True
                else:
                    print(f"[V3 Weapi] 响应中缺少 playlist/tracks 数据")
            else:
                print(f"[V3 Weapi] API 返回错误代码: {json_resp.get('code')}")
        else:
            print(f"[V3 Weapi] HTTP 请求失败: {response.status_code}")
    except Exception as e:
        print(f"[V3 Weapi] 发生异常: {e}")
    return False

# ------------------------------------------------------------------------------
# 主程序
# ------------------------------------------------------------------------------
if __name__ == "__main__":
    playlists = [
        {"name": "飙升榜", "id": "19723756"},
        {"name": "新歌榜", "id": "3779629"},
        # {"name": "热歌榜", "id": "3778678"} # 减少请求，测试前两个即可
    ]

    print("开始测试网易云音乐 API (Simple GET vs V3 Weapi)...")
    print("=" * 60)
    
    for pl in playlists:
        # 测试简单 GET
        fetch_netease_simple(pl['id'], pl['name'])
        print("-" * 30)
        
        # 测试 V3 Weapi
        fetch_netease_v3_weapi(pl['id'], pl['name'])
        
        print("=" * 60)
        time.sleep(1)