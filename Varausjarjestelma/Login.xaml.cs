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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Varausjarjestelma
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Login : Window
    {

        //Käyttäjän tiedot
        private string kayttajanimi;
        private string etunimi;
        private string sukunimi;
        private string salasana;
        private string salasanaVarmistus;
        private List<Kayttaja> _kayttajat;
        public Tietokanta Tietokanta;
        DispatcherTimer ajastin;

        private bool _rooli = false; // false jos user, true jos admin

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            Tietokanta = new Tietokanta();
            ajastin = new DispatcherTimer();
        }

        private async void btnkirjaudu_Click(object sender, RoutedEventArgs e)
        {

            // Luetaan käyttäjät tietokannasta
            _kayttajat = Tietokanta.GetKayttajat();
            //Alustetaan muuttujat tekstilaatikoiden avulla
            this.kayttajanimi = txt_kayttajaNimi.Text;
            this.salasana = txt_salasana.Password;

            Kayttaja kayttaja = _kayttajat.SingleOrDefault(n => n.Salasana == salasana && n.Tunnus == kayttajanimi);

            //Toiminnot jos käyttäjänimi ja salasana ovat oikein
            //if (this.kayttajanimi == "Matti" && this.salasana == "Matti")
            if (kayttaja != null)
            {
                tulostaIlmoitus("Kirjautuminen onnistui. Ladataan...", lbl_ilmoitus, false);
                await Task.Delay(100);

                _rooli = _kayttajat.Any(k => k.Rooli.Equals("Admin") && k.Tunnus == kayttajanimi);
                //Käyttäjän roolin mukaan avataan käyttäjälle tarkoitettu näkymä
                if (!_rooli)
                {
                    new Asiakas(kayttaja).Show();
                    this.Close();
                }
                else if (_rooli)
                {
                    new Yllapito().Show();
                    this.Close();
                }
            }
            //Virheilmoitus jos käyttäjänimi/salasana ovat väärin
            else
            {
                tulostaIlmoitus("Väärä käyttäjänimi tai salasana", lbl_ilmoitus, true);
            }
        }

        //Painike joka avaa rekisteröinti sivun
        private void btn_rekisteroidy_Click(object sender, RoutedEventArgs e)
        {
            txt_kayttajaNimi.Clear();
            txt_salasana.Clear();
            Login_Grid.Visibility = Visibility.Collapsed;
            Register_Grid.Visibility = Visibility.Visible;
            this.Width = 300;
            this.Height = 385;           
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
            this.kayttajanimi = txt_kayttajaNimiR.Text;
            this.etunimi = txt_etunimi.Text;
            this.sukunimi = txt_sukunimi.Text;
            this.salasana = txt_salasanaR.Password;
            this.salasanaVarmistus = txt_salasanan_vahvistus.Password;

            if (!kayttajanimi.Equals("") && !etunimi.Equals("") && !sukunimi.Equals("") && !salasana.Equals("") && !salasanaVarmistus.Equals("") )
            {
                //Tarkistetaan onko sama käyttäjänimi jo tietokannassa
                //Kayttaja kayttaja = getKayttaja(kayttajanimi);

                //Jos käyttäjää ei löydy tietokannasta niin jatketaan rekisteröintiä
                /*if (kayttaja == null)
                {

                }
                else
                {

                }*/

                if (this.salasana.Equals(this.salasanaVarmistus))
                {
                    //Rekisteröi käyttäjäjän tietokantaan
                    Kayttaja uusiKayttaja = new Kayttaja(etunimi, sukunimi, kayttajanimi, salasana, "User");

                    Tietokanta.SetKayttaja(uusiKayttaja);

                    //Ilmoitetaan käyttäjälle että rekisteöinti onnistui,
                    //tyhjennetään tekstilaatikot ja siirrytään login-formiin
                    tulostaIlmoitus("Rekisteröinti onnistui. Ladataan...", lbl_ilmoitusR, false);
                    await Task.Delay(200);
                    btn_takaisinR_Click(sender, e);
                }
                else
                {
                    tulostaIlmoitus("Salasanat eivät täsmää!", lbl_ilmoitusR, true);

                }

            }
            else
            {
                tulostaIlmoitus("Tarvittavia tietoja puuttuu", lbl_ilmoitusR, true);
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
            this.Width = 300;
            this.Height = 218;
            this.Title = "Kirjaudu Sisään";
        }

        //Metodi joka tulostaa ilmoituksen haluttuun labeliin
        private void tulostaIlmoitus(string tuloste, Label lbl, Boolean virheilmoitus)
        {
            if (virheilmoitus)
            {
                lbl.Foreground = red;
            }

            else
            {
                lbl.Foreground = white;
            }

            lbl.Content = tuloste;
            lbl.Visibility = Visibility.Visible;

            ajastin.Interval = new TimeSpan(0, 0, 3);
            ajastin.Tick += (EventHandler)delegate (object snd, EventArgs ea)
            {
                lbl.Visibility = Visibility.Collapsed;
                ((DispatcherTimer)snd).Stop();
            };
            ajastin.Start();

        }
        
        //Metodi joka etsii ja palauttaa käyttäjän tietokannasta
        //käyttäjänimen perusteella
        private Kayttaja getKayttaja(String kayttajatunnus)
        {
            return null;
        }

    }
}
