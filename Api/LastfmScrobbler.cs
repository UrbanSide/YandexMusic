using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Xml;
using YandexMusic.Properties;
using System.Diagnostics;

namespace YandexMusic.Api
{
    public class LastfmScrobbler
    {
        private static Settings settings = Settings.Default;
        private string apiKey;
        private string apiSecret;
        private string username;
        private string password;
        public LastfmScrobbler()
        {
            this.apiKey = settings.apiKey;
            this.apiSecret = settings.secretToken;
            this.username = settings.userLogin;
            this.password = settings.userPassword;
        }

        public void ScrobbleTrack(string artist, string track)
        {
            string timestamp = DateTime.Now.ToUnixTimestamp().ToString(); // Текущее время

            // Аутентификация пользователя и получение sessionKey
            Dictionary<string, string> authParams = new Dictionary<string, string>()
        {
            { "username", username },
            { "password", password }
        };
            string authResponse = CallLastfmAPI(apiKey, apiSecret, "auth.getMobileSession", authParams);

            // Обработка ответа от API Last.fm
            Debug.WriteLine(authResponse);

            // Парсинг XML-ответа для получения session key
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(authResponse);
            string sessionKey = xmlDoc.SelectSingleNode("//session/key")?.InnerText;

            if (!string.IsNullOrEmpty(sessionKey))
            {
                // Обновление sessionKey в параметрах скробблинга трека
                Dictionary<string, string> scrobbleParams = new Dictionary<string, string>()
            {
                { "sk", sessionKey },
                { "artist", artist },
                { "track", track },
                { "timestamp", timestamp }
            };

                // Скробблинг трека
                string scrobbleResponse = CallLastfmAPI(apiKey, apiSecret, "track.scrobble", scrobbleParams);

                // Обработка ответа от API Last.fm
                Debug.WriteLine(scrobbleResponse);
            }
            else
            {
                Debug.WriteLine("Failed to obtain session key.");
            }
        }

        private string CallLastfmAPI(string apiKey, string apiSecret, string method, Dictionary<string, string> parameters)
        {
            string url = "http://ws.audioscrobbler.com/2.0/";

            // Добавление общих параметров
            parameters["method"] = method;
            parameters["api_key"] = apiKey;

            // Сортировка параметров по алфавиту
            SortedDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);

            // Создание подписи запроса
            string sig = "";
            foreach (KeyValuePair<string, string> param in sortedParams)
            {
                sig += param.Key + param.Value;
            }
            sig += apiSecret;
            parameters["api_sig"] = GetMd5Hash(sig);

            // Создание POST-запроса
            using (WebClient client = new WebClient())
            {
                var postData = new System.Collections.Specialized.NameValueCollection();
                foreach (KeyValuePair<string, string> param in parameters)
                {
                    postData.Add(param.Key, param.Value);
                }

                var responseBytes = client.UploadValues(url, "POST", postData);

                return Encoding.UTF8.GetString(responseBytes);
            }
        }

        private string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }

    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
    }

}
