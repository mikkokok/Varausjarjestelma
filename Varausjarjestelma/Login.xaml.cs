using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        //Annetut käyttäjänimi ja salasana
        private string kayttajanimi;
        private string salasana;
        private string salasanaVarmistus;

        private int rooli = 1; // 0 = asiakas 1 = ylläpitäjä, Testausta varten

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;

            //var tietokanta = new Tietokanta(); // Debuggia varten / mikko
            //for (var i = 0; i < 10; i++)
            //{
            //    tietokanta.Ajasql($"INSERT INTO elokuvat VALUES(null, 'Elokuva {i}', '2005', 'Kylla')");
            //}
            //var testi = tietokanta.Ajasql("SELECT * FROM elokuvat");
            //foreach (var testib in testi)
            //{
            //    Console.Write(testib);
            //}
        }

        private async void btnkirjaudu_Click(object sender, RoutedEventArgs e)
        {
            //Alustetaan muuttujat tekstilaatikoiden avulla
            this.kayttajanimi = txt_kayttajaNimi.Text;
            this.salasana = txt_salasana.Password;

            //Toiminnot jos käyttäjänimi ja salasana ovat oikein
            if (this.kayttajanimi == "Matti" && this.salasana == "Matti")
            {
                //Tulostetaan ilmoitus käyttäjälle
                lbl_ilmoitus.Foreground = white;
                lbl_ilmoitus.Content = "Kirjautuminen onnistui. Ladataan...";
                lbl_ilmoitus.Visibility = Visibility.Visible;

                await Task.Delay(2000);

                //Käyttäjän roolin mukaan avataan käyttäjälle tarkoitettu näkymä
                if (this.rooli == 0)
                {
                    Window AsiakasWindow = new Window();
                    AsiakasWindow.Content = new Asiakas();
                    AsiakasWindow.Show();
                    this.Close();
                }
                else if(this.rooli == 1)
                {
                    new Yllapito().Show();
                    this.Close();
                }
            }

            //Virheilmoitus jos käyttäjänimi/salasana ovat väärin
            else
            {
                lbl_ilmoitus.Content = "Väärä käyttäjänimi tai salasana";
                lbl_ilmoitus.Foreground = red;
                lbl_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_ilmoitus.Visibility = Visibility.Collapsed;

            }
        }

        //Painike joka avaa rekisteröinti sivun
        private void btn_rekisteroidy_Click(object sender, RoutedEventArgs e)
        {
            txt_kayttajaNimi.Clear();
            txt_salasana.Clear();
            Login_Grid.Visibility = Visibility.Collapsed;
            Register_Grid.Visibility = Visibility.Visible;
            this.Title = "Rekisteröidy";
        }

        //Toiminnot Enter-painikkeelle
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Jos käyttäjä on login formissa ja painaa enteriä niin yritetään kirjautua sisään
            if (e.Key == Key.Enter && Login_Grid.IsVisible)
            {
                btnkirjaudu_Click(sender, e);
            }
            //Jos käyttäjä on rekisteröinti formissa ja painaa enteriä niin yritetään rekisteröityä
            else if (e.Key == Key.Enter && Register_Grid.IsVisible)
            {
                btn_rekisteroidy_Click(sender, e);
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

        //Painike joka lisää käyttäjän tietokantaan
        private async void btn_rekisteroi_Click(object sender, RoutedEventArgs e)
        {
            this.kayttajanimi= txt_kayttajaNimiR.Text;
            this.salasana = txt_salasanaR.Password;
            this.salasanaVarmistus = txt_salasanan_vahvistus.Password;

            if (this.salasana == this.salasanaVarmistus)
            {
                //Rekisteöi käyttäjäjän tietokantaan
                rekisteroiKayttaja(this.kayttajanimi, this.salasana);

                //Ilmoitetaan käyttäjälle että rekisteöinti onnistui,
                //tyhjennetään tekstilaatikot ja siirrytään login-formiin
                lbl_ilmoitusR.Foreground = white;
                lbl_ilmoitusR.Visibility = Visibility.Visible;
                lbl_ilmoitusR.Content = "Rekisteröinti onnistui. Ladataan...";
                await Task.Delay(2000);
                lbl_ilmoitusR.Visibility = Visibility.Collapsed;

                txt_kayttajaNimiR.Clear();
                txt_salasanaR.Clear();
                txt_salasanan_vahvistus.Clear();

                Register_Grid.Visibility = Visibility.Collapsed;
                Login_Grid.Visibility = Visibility.Visible;
                this.Title = "Kirjaudu Sisään";

            }
            else
            {
                lbl_ilmoitusR.Foreground = red;
                lbl_ilmoitusR.Content = "Salasanat eivät täsmää!";
                lbl_ilmoitusR.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_ilmoitusR.Visibility = Visibility.Collapsed;
            }
        }

        //Painike joka palaa login-formiin rekisteröinti-lomakkeelta
        private void btn_takaisinR_Click(object sender, RoutedEventArgs e)
        {
            txt_kayttajaNimiR.Clear();
            txt_salasanaR.Clear();
            txt_salasanan_vahvistus.Clear();

            Register_Grid.Visibility = Visibility.Collapsed;
            Login_Grid.Visibility = Visibility.Visible;
            this.Title = "Kirjaudu Sisään";
        }

        //Metodi joka lisää käyttäjän tietokantaan
        //Palauttaa true:n jos onnistuu
        private bool rekisteroiKayttaja(String username, String password)
        {
            return true;
        }
    }
}
