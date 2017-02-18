using System;
using System.Collections.Generic;
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

namespace Varausjarjestelma
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Login : Window
    {

        private readonly int mainWidth = 800;
        private readonly int mainHeight = 600;

        private String username;
        private String password;

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
        }

        private void btnkirjaudu_Click(object sender, RoutedEventArgs e)
        {
            this.username = txt_kayttajaNimi.Text;
            this.password = txt_salasana.Text;

            if (this.username == "Matti" && this.password == "Matti")
            {
                lbl_ilmoitus.Content = "Kirjautuminen onnistui";
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.White);
                Login_Grid.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Width = this.mainWidth;
                Application.Current.MainWindow.Height = this.mainHeight;
                YllapidonControl.Visibility = Visibility.Visible;

            }
            else
            {
                lbl_ilmoitus.Visibility = Visibility.Visible;
                lbl_ilmoitus.Content = "Käyttäjänimi tai salasana on väärin";
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void btn_rekisteroidy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key== Key.Enter)
            {
                btnkirjaudu_Click(sender, e);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize == e.NewSize)
                return;

            var w = SystemParameters.PrimaryScreenWidth;
            var h = SystemParameters.PrimaryScreenHeight;

            this.Left = (w - e.NewSize.Width) / 2;
            this.Top = (h - e.NewSize.Height) / 2;
        }
    }
}
