using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        //Pääikkunan koko
        private readonly int mainWidth = 800;
        private readonly int mainHeight = 600;

        //Annetut käyttäjänimi ja salasana
        private String username;
        private String password;

        private int rooli = 1; // 0 = asiakas 1 = ylläpitäjä

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
        }

        private void btnkirjaudu_Click(object sender, RoutedEventArgs e)
        {
            //Alustetaan muuttujat tekstilaatikoiden avulla
            this.username = txt_kayttajaNimi.Text;
            this.password = txt_salasana.Text;

            //Toiminnot jos käyttäjänimi ja salasana ovat oikein
            if (this.username == "Matti" && this.password == "Matti")
            {
                //Tulostetaan ilmoitus käyttäjälle
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.White);
                lbl_ilmoitus.Content = "Kirjautuminen onnistui";
                lbl_ilmoitus.Visibility = Visibility.Visible;

                //Thread.Sleep(1000);

                //Piilotetaan Login ja muutetaan ikkunan kokoa
                Login_Grid.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Width = this.mainWidth;
                Application.Current.MainWindow.Height = this.mainHeight;

                //Käyttäjän roolin mukaan avataan käyttäjälle tarkoitettu näkymä
                if (this.rooli == 1)
                {
                    this.Title = "Varausjärjestelmän Ylläpito";
                    YllapidonControl.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Title = "Elokuvalippujen Varausjärjestelmä";
                    AsiakkaanControl.Visibility = Visibility.Visible;
                }
                

            }
            //Virheilmoitus jos käyttäjänimi/salasana ovat väärin
            else
            {
                lbl_ilmoitus.Visibility = Visibility.Visible;
                lbl_ilmoitus.Content = "Väärä käyttäjänimi tai salasana";
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void btn_rekisteroidy_Click(object sender, RoutedEventArgs e)
        {

        }

        //Toiminnot Enter-painikkeelle
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Jos käyttäjä on login formissa ja painaa enteriä niin yritetään kirjautua sisään
            if (e.Key== Key.Enter && Login_Grid.Visibility == Visibility.Visible)
            {
                btnkirjaudu_Click(sender, e);
            }
        }

        //Metodi joka sijoittaa ikkunan keskelle tietokoneen näyttöä kun ikkunan koko muuttuu
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize == e.NewSize)
                return;

            var width = SystemParameters.PrimaryScreenWidth;
            var height = SystemParameters.PrimaryScreenHeight;

            this.Left = (width - e.NewSize.Width) / 2;
            this.Top = (height - e.NewSize.Height) / 2;
        }
    }
}
