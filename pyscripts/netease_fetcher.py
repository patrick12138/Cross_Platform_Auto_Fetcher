# -*- coding: utf-8 -*-
import requests
import json
import base64
import os
import csv
from Crypto.Cipher import AES

# --- Constants ---
MODULUS = '00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7'
NONCE = b'0CoJUm6Qyw8W8jud'
PUBKEY = '010001'
API_URL = "https://music.163.com/weapi/v3/playlist/detail"
HEADERS = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
    'Referer': 'https://music.163.com/',
    'Content-Type': 'application/x-www-form-urlencoded'
}

# --- Crypto Functions ---
def aes_encrypt(text, key, iv):
    pad = 16 - len(text) % 16
    text = text + bytes([pad]) * pad
    cipher = AES.new(key, AES.MODE_CBC, iv)
    encrypted_bytes = cipher.encrypt(text)
    return base64.b64encode(encrypted_bytes)

def rsa_encrypt(text, pub_key, modulus):
    text = text[::-1]
    rs = int(text.hex(), 16) ** int(pub_key, 16) % int(modulus, 16)
    return format(rs, 'x').zfill(256)

def create_secret_key(size):
    import random
    return (''.join(random.choice('0123456789abcdef') for _ in range(size))).encode('utf-8')

def weapi_encrypt(data):
    data_bytes = json.dumps(data).encode('utf-8')
    secret_key = create_secret_key(16)
    iv = b'0102030405060708'
    params = aes_encrypt(aes_encrypt(data_bytes, NONCE, iv), secret_key, iv)
    enc_sec_key = rsa_encrypt(secret_key, PUBKEY, MODULUS)
    return {
        'params': params.decode('utf-8'),
        'encSecKey': enc_sec_key
    }

# --- Main Logic ---
def fetch_and_save_toplist(chart_name, chart_id, output_dir):
    print(f"正在抓取网易云音乐 -> {chart_name}...")
    payload = {
        "id": str(chart_id),
        "offset": 0,
        "total": True,
        "limit": 1000,
        "n": 1000,
        "csrf_token": ""
    }
    
    encrypted_data = weapi_encrypt(payload)

    try:
        response = requests.post(API_URL, headers=HEADERS, data=encrypted_data)
        if response.status_code != 200:
            print(f"  -> 错误：HTTP状态码 {response.status_code}")
            return

        data = response.json()
        tracks = data.get('playlist', {}).get('tracks', [])

        if not tracks:
            print("  -> 错误：未在响应中找到歌曲列表。")
            return

        song_list = []
        for i, track in enumerate(tracks, 1):
            artist_names = ' / '.join([ar['name'] for ar in track.get('ar', [])])
            song_info = {
                '排名': i,
                '歌曲名': track.get('name'),
                '歌手': artist_names,
                '专辑': track.get('al', {}).get('name')
            }
            song_list.append(song_info)
        
        # Save to CSV
        output_path = os.path.join(output_dir, f"{chart_name}.csv")
        with open(output_path, 'w', newline='', encoding='utf-8-sig') as f:
            writer = csv.DictWriter(f, fieldnames=['排名', '歌曲名', '歌手', '专辑'])
            writer.writeheader()
            writer.writerows(song_list)
        
        print(f"  -> 成功！已保存 {len(song_list)} 首歌曲到 {output_path}")

    except Exception as e:
        print(f"  -> 发生意外错误: {e}")

def main():
    charts_to_fetch = {
        "飙升榜": 19723756,
        "新歌榜": 3779629,
        "热歌榜": 3778678
    }
    
    output_dir = os.path.join(os.path.dirname(__file__), 'netease_toplists')
    os.makedirs(output_dir, exist_ok=True)
    print(f"文件将保存在: {output_dir}")

    for name, chart_id in charts_to_fetch.items():
        fetch_and_save_toplist(name, chart_id, output_dir)
        print("---")

    print("所有网易云榜单处理完毕。")

if __name__ == '__main__':
    main()
