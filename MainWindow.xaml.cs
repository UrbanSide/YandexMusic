using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using YandexMusic.Properties;
using System.Runtime;
using System.IO;
using System.Windows.Shapes;
using Path = System.IO.Path;
using YandexMusic.Api;
using Newtonsoft.Json;
using System.Windows.Controls.Primitives;
using Microsoft.Web.WebView2.Wpf;
using YandexMusic.Pages;
using ControlzEx.Theming;

namespace YandexMusic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public DispatcherTimer trackData = new DispatcherTimer();
        public DispatcherTimer dBot = new DispatcherTimer();
        public DispatcherTimer LastFM = new DispatcherTimer();
        public DiscordRpcClient client;
        private static Settings settings = Settings.Default;
        string selfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Node\SmollNet.exe");
        public string LIText = "By Aralim";
        public string LIKey = "yandexlogomusic";
        private ClientSocket clientSocket;
        public MainWindow()
        {
            InitializeComponent();
            if (Properties.Settings.Default.theme.Length > 1)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, Properties.Settings.Default.theme);
                ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
                ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncWithAppMode);
            }
            webView.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
            webView.Source = new Uri("https://music.yandex.ru/");
            /*
               Create a Discord client
               NOTE: 	If you are using Unity3D, you must use the full constructor and define
                        the pipe connection.
               */
            if (!settings.isSelfBot && settings.userToken.Length>0)
            {
                client = new DiscordRpcClient("848835233767751680");

                //Set the logger
                client.Logger = new ConsoleLogger()
                {
                    Level = LogLevel.None
                };

                //Subscribe to events
                client.OnReady += (sender, e) => {
                    Debug.WriteLine("Received Ready from user {0}", e.User.Username);
                };
                client.Initialize();
            }
            else
            {
                string processName = "SmollNet"; // Замените на имя вашего процесса

                // Проверка наличия запущенного процесса с заданным именем
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    // Завершение всех найденных процессов
                    foreach (Process process in processes)
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                Directory.CreateDirectory("logs");
                StartSocketServer();
            }
        }
        static string logFilePath = Path.Combine("logs", "server.log");
        StreamWriter logWriter = new StreamWriter(logFilePath, true);
        private void StartSocketServer()
        {
            try
            {
            // Создание объекта процесса и настройка его параметров
            var arg = AppDomain.CurrentDomain.BaseDirectory + @"Node\main.js " + Properties.Settings.Default.userToken;
            Process process = new Process();
            process.StartInfo.FileName = selfFilePath;
            process.StartInfo.Arguments = arg;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // Перенаправление стандартного потока вывода процесса в StreamReader
            process.StartInfo.RedirectStandardOutput = true;

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data.Contains("Server started"))
                    {
                        Debug.WriteLine("=======================================ЗАПУСКАЕМ СОКЕТ=======================================");
                        clientSocket = new ClientSocket();
                        clientSocket.Connect("localhost", 8800);
                        dBot.Start();
                    }
                    logWriter.WriteLine(e.Data);
                    logWriter.Flush();
                };

                // Запуск процесса и начало асинхронного чтения его стандартного потока вывода
                process.Start();
                process.BeginOutputReadLine();
            }
            catch(Exception ex)
            {
                logWriter.WriteLine(ex.Message);
                logWriter.Flush();
            }
        }
        async void trackData_TickAsync(object sender, EventArgs e)
        {
            try
            {
                var playing = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.isPlaying();");
                var plus = await webView.CoreWebView2.ExecuteScriptAsync($"DataSrc.auth.user.havePlus.toString();");

                if (playing.Contains("true"))
                {
                    var progress = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getProgress();");
                    var track = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getCurrentTrack();");
                    JObject messageData = JObject.Parse(track);
                    JObject time = JObject.Parse(progress);
                    string artist = (string)messageData["artists"][0]["title"];
                    string title = (string)messageData["title"];
                    string cover = (string)messageData["cover"];
                    int position = (int)time["position"];
                    int duration = (int)time["duration"];
                    client.ClearPresence();
                    client.SetPresence(new RichPresence()
                    {
                        Details = artist + " - " + title,
                        State = FormatSeconds(position) + " из " + FormatSeconds(duration),
                        Assets = new Assets()
                        {
                            LargeImageKey = "https://" + cover.Replace("%%", "200x200"),
                            LargeImageText = LIText,
                            SmallImageKey = "yandexlogomusic"
                        }
                    });
                    client.Invoke();
                }
            }
            catch
            {
                Debug.WriteLine("Статус: ошибка");
            }
        }
        async void dBot_Handler(object sender, EventArgs e)
        {
            try
            {
                var playing = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.isPlaying();");
                var plus = await webView.CoreWebView2.ExecuteScriptAsync($"DataSrc.auth.user.havePlus.toString();");

                if (playing.Contains("true"))
                {
                    var progress = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getProgress();");
                    var track = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getCurrentTrack();");
                    JObject messageData = JObject.Parse(track);
                    JObject time = JObject.Parse(progress);
                    string artist = (string)messageData["artists"][0]["title"];
                    string title = (string)messageData["title"];
                    string cover = (string)messageData["cover"];
                    int position = (int)time["position"];
                    int duration = (int)time["duration"];
                    clientSocket.Send(progress, track, plus);
                }
                else
                {
                    clientSocket.Clear();
                }
            }
            catch
            {
                Debug.WriteLine("Статус: ошибка");
            }
        }
        public string FormatSeconds(int totalSeconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
            string timeString;

            if (timeSpan.TotalMinutes >= 60)
            {
                timeString = $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
            else
            {
                timeString = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }

            return timeString;
        }
        private async void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                webView.CoreWebView2.DocumentTitleChanged += DocumentTitleChanged;
                await webView.EnsureCoreWebView2Async();
                webView.NavigationCompleted += MusicPlayer_NavigationCompleted;
            }
        }
        string LFTitle = "";
        private async void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Получаем сообщение в формате JSON
            string jsonMessage = e.WebMessageAsJson;

            // Распарсим JSON для получения данных
            JObject message = JObject.Parse(jsonMessage);
            if (message["type"].ToString() == "console")
            {
                var playing = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.isPlaying();");

                if (playing.Contains("true"))
                {
                    var progress = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getProgress();");
                    JObject messageData = JObject.Parse(message["message"].ToString());
                    JObject time = JObject.Parse(progress);
                    string artist = (string)messageData["artists"][0]["title"];
                    string title = (string)messageData["title"];
                    string cover = (string)messageData["cover"];
                    int position = (int)time["position"];
                    int duration = (int)time["duration"];
                    LFTitle = title;
                    // Обрабатываем информацию о текущем треке
                    Console.WriteLine(artist+" - "+title);
                    LastFM.Tick += new EventHandler(LastFM_Handler);
                    LastFM.Interval = new TimeSpan(0, 0, 0, 1);
                    if (LastFM.IsEnabled)
                    {
                        LastFM.Stop();
                    }
                    LastFM.Start();
                }
                else
                {
                    if (LastFM.IsEnabled)
                    {
                        LastFM.Stop();
                    }
                }
            }
        }

        private async void LastFM_Handler(object sender, EventArgs e)
        {
            var progress = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getProgress();");
            var track = await webView.CoreWebView2.ExecuteScriptAsync($"externalAPI.getCurrentTrack();");
            JObject messageData = JObject.Parse(track);
            JObject time = JObject.Parse(progress);
            string artist = (string)messageData["artists"][0]["title"];
            string title = (string)messageData["title"];
            string cover = (string)messageData["cover"];
            int position = (int)time["position"];
            int duration = (int)time["duration"];
            // Обрабатываем информацию о текущем треке
            if (title.Equals(LFTitle))
            {
                Debug.WriteLine(FormatSeconds(position) + " из " + FormatSeconds(duration));
                if (position > duration / 2)
                {
                    if (settings.secretToken != null && settings.apiKey != null && settings.userLogin != null && settings.userPassword != null)
                    {
                        LastfmScrobbler scrobbler = new LastfmScrobbler();
                        scrobbler.ScrobbleTrack(artist, title);
                    }
                    Debug.WriteLine("Стучим в LastFM!");
                    LastFM.Stop();
                }
            }
        }

        private async void MusicPlayer_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                if (webView.Visibility == Visibility.Hidden || webView.Visibility == Visibility.Collapsed)
                {
                    webView.Visibility = Visibility.Visible;
                    loadingScreen.Visibility = Visibility.Collapsed;
                }
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                trackData.Tick += new EventHandler(trackData_TickAsync);
                trackData.Interval = new TimeSpan(0, 0, 0, 1);
                dBot.Tick += new EventHandler(dBot_Handler);
                dBot.Interval = new TimeSpan(0, 0, 0, 5);
                if (!settings.isSelfBot)
                {
                    trackData.Start();
                }

                // Включаем прием сообщений из JavaScript
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                await webView.ExecuteScriptAsync(@"externalAPI.on(externalAPI.EVENT_TRACK, function() {
                    var track = externalAPI.getCurrentTrack();
                    console.log(track);
                    window.chrome.webview.postMessage({ type: 'console', message: track });;
                });");
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

                // Регистрация обработчика события WebMessageReceived
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                ((WebView2)sender).ExecuteScriptAsync("document.querySelector('body').style.overflow='scroll';var style=document.createElement('style');style.type='text/css';style.innerHTML='::-webkit-scrollbar{display:none}';document.getElementsByTagName('body')[0].appendChild(style)");
            }
        }

        private void DocumentTitleChanged(object sender, object e)
        {
            var title = this.webView.CoreWebView2.DocumentTitle;
            if (!title.Equals("null"))
            {
                this.Title = title;
            }
        }

        private void openSettings(object sender, RoutedEventArgs e)
        {
            appSettings settings = new appSettings();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            settings.ShowDialog();
        }

        private void openBeer(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("СБП 5469 9804 7111 9954 Даниил", "Поддержать проект", MessageBoxButton.OK);
        }

        private void openLastFM(object sender, RoutedEventArgs e)
        {
            LastFMsettings settings = new LastFMsettings();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            settings.ShowDialog();
        }
    }
}