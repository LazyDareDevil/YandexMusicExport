import requests 
import json 
import time 
import re 
import uuid
from datetime import datetime

def print_error(e):
    print(datetime.now().strftime("%Y-%m-%d %H:%M:%S"), "|", ">>>", e)

def send_welcome():
    print("=== Экспорт Яндекс Музыки === \n\n" +
          "Программа позволяет экспортировать любой плейлист Яндекс Музыки в текстовое " +
          "представление ИМЯ ИСПОЛНИТЕЛЯ - НАЗВАНИЕ ТРЕКА.\n\n" +
          "1. Скопируйте и вставьте ниже ссылку на плейлист. Обязательно проверьте, чтобы она была " +
          "вида https://music.yandex.ru/users/USERNAME/playlists/PLAYLIST_ID." +
          " или вида https://music.yandex.ru/playlists/lk.PLAYLIST_UUID или https://music.yandex.ru/playlists/PLAYLIST_UUID (данный вид в тестовом режиме)\n" +
          "<b><i>Также убедитесь, что плейлист ❗️не приватный❗️</i></b>\n" +
          "2. Если плейлист большой, может потребоваться некоторое время для обработки.\n" +
          "3. Если ссылка корректная, но возникает ошибка, то, вероятно, сработал 'бан' со " +
          "стороны Яндекса. В таком случае попробуйте еще раз через некоторое время или на " +
          "другом устройстве.\n" +
          "4. Предложения, критика и прочее принимаются тута: https://t.me/aleqsanbr")
    print("Также функционал доступен на сайте :) https://files.u-pov.ru/programs/YandexMusicExport")

def handle_message(uri_raw):
    try:
        uri_raw = uri_raw.strip()
        uri_parts = re.split('/|\?', uri_raw)

        owner = uri_parts[4]
        playlist_uuid_part = uri_parts[4]
        kinds = ""

        if len(uri_parts) > 6:
            kinds = uri_parts[6]

        playlist_uuid = playlist_uuid_part
        if (playlist_uuid_part.startswith('lk.')) :
            playlist_uuid = playlist_uuid[3,-1]
            
        is_parsed = False
        try:
            uuid.UUID(playlist_uuid)
            is_parsed = True
        except ValueError:
            is_parsed = False

        if (is_parsed):
            responseHtml = requests.get(uri_raw)
            responseHtml.raise_for_status()
            data = responseHtml.content.decode(responseHtml.encoding)
            match = re.match("\"uuid\":\"" +  playlist_uuid_part + "\".+\"uid\":(?  <useruid>[0-9]+).+\"kind\":(?P<playlistkind>[0-9]+)")
            if (match):
                try:
                    owner = match.group('useruid')
                    kinds = match.group('playlistkind')
                except:
                    is_parsed = False
                    
        
        uri = f'https://api.music.yandex.net/users/{owner}/playlists/{kinds}'

        response = requests.get(uri)
        response.raise_for_status()

        data = response.json()['result']
        
        playlist_title = data['title']
        tracks = data['tracks']

        all_file = f"{playlist_title}\n"

        for track in tracks:
            artists_names = ", ".join(artist['name'] for artist in track['track']['artists'])
            full_track = f"{artists_names} - {track['track']['title']}\n"
            all_file += full_track

        today_text = datetime.now().strftime("%Y-%m-%d")
        filename = f"{playlist_title}_{today_text}.txt"
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(all_file)

        print(f"Плейлист сохранен в файл: {filename}")
        print("Поддержите работу сервиса: https://u-pov.ru/donate. Спасибо за использование! 💜")

    except (json.JSONDecodeError, requests.exceptions.RequestException) as e:
        print_error(e)
        print("Ошибка! Несуществующий плейлист или временный бан от Яндекса. Проверьте ссылку " +
              "и попробуйте еще раз через некоторое время или на другом устройстве.\n\n" +
              f"Дополнительная информация: {e}")
    except IndexError as e:
        print_error(e)
        print("Ошибка! Вероятно, некорректная ссылка. Проверьте, чтобы она была вида " +
              "https://music.yandex.ru/users/USERNAME/playlists/PLAYLIST_ID. Попробуйте еще раз.\n\n" +
              f"Дополнительная информация: {e}")
    except Exception as e:
        print_error(e)
        print("Ошибка! Проверьте правильность ссылки и попробуйте еще раз. " +
              "Также учтите, что из-за большого количества запросов может последовать временный " +
              "бан от Яндекса. В таком случае попробуйте с другого устройства или на сайте " +
              "https://files.u-pov.ru/programs/YandexMusicExport.\n\n" +
              f"Дополнительная информация: {e}")

if __name__ == "__main__":
    send_welcome()
    while True:
        try:
            uri_raw = input("\nВведите ссылку на плейлист: ")
            handle_message(uri_raw)
        except KeyboardInterrupt:
            print("\nВыход из программы.")
            break
        except Exception as e:
            print_error(e)
            time.sleep(15)
