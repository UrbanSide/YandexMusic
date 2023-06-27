using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YandexMusic.Pages
{
    /// <summary>
    /// Логика взаимодействия для appSettings.xaml
    /// </summary>
    public partial class appSettings
    {
        public appSettings()
        {
            InitializeComponent();
            Style.SelectedIndex = Properties.Settings.Default.theme_index;
            DSBot.IsOn = Properties.Settings.Default.isSelfBot;
            DSCToken.Text = Properties.Settings.Default.userToken;
        }
        #region Смена оформления
        /*
         ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, "Dark.Green");
         ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
         ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncWithAppMode); 
         */
        public string color = "Dark.";
        private void ChangeColor(object sender, EventArgs e)
        {
            if (themeType.IsOn)
            {
                color = "Light.";
            }
            else
            {
                color = "Dark.";
            }
            string style = color + Style.Text;
            ThemeManager.Current.ChangeTheme(Application.Current, style);
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncWithAppMode);
            Properties.Settings.Default.theme_index = Style.SelectedIndex;
            Properties.Settings.Default.theme = style;
            Properties.Settings.Default.dark = themeType.IsOn;
            Properties.Settings.Default.Save();
        }
        #endregion

        private void SaveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string style = color + Style.Text;
            Properties.Settings.Default.theme_index = Style.SelectedIndex;
            Properties.Settings.Default.theme = style;
            Properties.Settings.Default.dark = themeType.IsOn;
            Properties.Settings.Default.isSelfBot = DSBot.IsOn;
            Properties.Settings.Default.userToken = DSCToken.Text;
            Properties.Settings.Default.Save();
            //Рестарт
            string applicationPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            // Запускаем новый процесс с помощью метода Start
            System.Diagnostics.Process.Start(applicationPath);
            // Закрываем текущее приложение
            System.Windows.Application.Current.Shutdown();
        }
    }
}
