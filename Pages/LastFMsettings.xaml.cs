using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
    /// Логика взаимодействия для LastFMsettings.xaml
    /// </summary>
    public partial class LastFMsettings
    {
        public LastFMsettings()
        {
            InitializeComponent();
            apiKey.Text = Properties.Settings.Default.apiKey;
            secretKey.Text = Properties.Settings.Default.secretToken;
            userLogin.Text = Properties.Settings.Default.userLogin;
            userPassword.Text = Properties.Settings.Default.userPassword;
        }

        private void guideOpen(object sender, RoutedEventArgs e)
        {
            Process.Start("https://telegra.ph/settings-06-27");
        }

        private void createLastFM(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.last.fm/api/account/create");
        }

        private void saveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.apiKey = apiKey.Text;
            Properties.Settings.Default.secretToken = secretKey.Text;
            Properties.Settings.Default.userLogin = userLogin.Text;
            Properties.Settings.Default.userPassword = userPassword.Text;
            Properties.Settings.Default.Save();
        }
    }
}
