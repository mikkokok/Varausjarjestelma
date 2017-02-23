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
        private enum ProgramState
        {
            Kirjautuminen,
            Rekisterointi,
            YllapidonControl,
            AsiakkaanControl
        };

        private ProgramState state;

        //Pääikkunan koko
        private readonly int mainWidth = 800;
        private readonly int mainHeight = 600;

        //Login formin koko
        private readonly int loginWidth = 300;
        private readonly int loginHeight = 218;

        //Annetut käyttäjänimi ja salasana
        private string username;
        private string password;
        private string repeatedPassword;

        private int rooli = 1; // 0 = asiakas 1 = ylläpitäjä, Testausta varten

        private Elokuva lisattavaElokuva;
        private List<Näytös>  lisattavatNaytokset;

        private String elokuvanNimi;
        private int elokuvanVuosi;
        private int elokuvanKesto;
        private String elokuvanKuvaus;

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            state = ProgramState.YllapidonControl;
            lisattavatNaytokset = new List<Näytös>();

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
            this.username = txt_kayttajaNimi.Text;
            this.password = txt_salasana.Password;

            //Toiminnot jos käyttäjänimi ja salasana ovat oikein
            if (this.username == "Matti" && this.password == "Matti")
            {
                //Tulostetaan ilmoitus käyttäjälle
                lbl_ilmoitus.Foreground = white;
                lbl_ilmoitus.Content = "Kirjautuminen onnistui. Ladataan...";
                lbl_ilmoitus.Visibility = Visibility.Visible;

                //pb_login.Visibility = Visibility.Visible;
                //pb_login.IsIndeterminate = true;
                await Task.Delay(2000);

                lbl_ilmoitus.Visibility = Visibility.Collapsed;

                //Piilotetaan Login ja muutetaan ikkunan kokoa
                Login_Grid.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Width = this.mainWidth;
                Application.Current.MainWindow.Height = this.mainHeight;

                txt_kayttajaNimi.Clear();
                txt_salasana.Clear();

                //Käyttäjän roolin mukaan avataan käyttäjälle tarkoitettu näkymä
                if (this.rooli == 1)
                {
                    this.Title = "Varausjärjestelmän Ylläpito";
                    state = ProgramState.YllapidonControl;
                    YllapidonControl.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Title = "Elokuvalippujen Varausjärjestelmä";
                    state = ProgramState.AsiakkaanControl;
                    AsiakkaanControl.Visibility = Visibility.Visible;
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
            if (e.Key == Key.Enter && state == ProgramState.Kirjautuminen)
            {
                btnkirjaudu_Click(sender, e);
            }
            //Jos käyttäjä on rekisteröinti formissa ja painaa enteriä niin yritetään rekisteröityä
            else if (e.Key == Key.Enter && state == ProgramState.Rekisterointi)
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
            this.username = txt_kayttajaNimiR.Text;
            this.password = txt_salasanaR.Password;
            this.repeatedPassword = txt_salasanan_vahvistus.Password;

            if (this.password == this.repeatedPassword)
            {
                //Rekisteöi käyttäjäjän tietokantaan
                rekisteroiKayttaja(this.username, this.password);

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

        //Painike joka hoitaa käyttäjän kirjautumisen ulos järjestelmästä ja 
        //avaa login-formin ja tyhjentää muuttujat
        private async void btn_kirjaudu_ulos_Click(object sender, RoutedEventArgs e)
        {
            lbl_logout_ilmoitus.Visibility = Visibility.Visible;
            await Task.Delay(2000);

            if (state == ProgramState.YllapidonControl)
            {
                YllapidonControl.Visibility = Visibility.Collapsed;
                Login_Grid.Visibility = Visibility.Visible;
                resetYllapidonControl();
                this.Title = "Kirjaudu Sisään";
                Application.Current.MainWindow.Width = this.loginWidth;
                Application.Current.MainWindow.Height = this.loginHeight;
                state = ProgramState.Kirjautuminen;

                this.username = null;
                this.password = null;
                //Lisää tyhjennyksiä tulossa
            }
        }

        //Metodi, joka muotoilee UI-elementin vastaanottamaan vain numeroita
        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        private void resetYllapidonControl()
        {
            YllapidonEtusivuTab.IsSelected = true;
            lbl_logout_ilmoitus.Visibility = Visibility.Collapsed;
        }

        private async void btn_Lisaa_Elokuvan_Perustiedot_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_Nimi.Text.Equals("") || txt_Vuosi.Text.Equals("") || txt_Kesto.Text.Equals("") || txt_Kuvaus.Text.Equals(""))
            {
                lbl_lisays_ilmoitus.Foreground = red;
                lbl_lisays_ilmoitus.Content = "Vaadittavia tietoja puuttuu! Tarkista tiedot";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
            else
            {
                elokuvanNimi = txt_Elokuvan_Nimi.Text;
                elokuvanVuosi = Int32.Parse(txt_Vuosi.Text);
                elokuvanKesto = Int32.Parse(txt_Kesto.Text);
                elokuvanKuvaus = txt_Kuvaus.Text;

                lisattavaElokuva = new Elokuva(elokuvanNimi, elokuvanKesto, elokuvanKuvaus);

                Perustiedot_Grid.Visibility = Visibility.Collapsed;
                Naytokset_Lisays_Grid.Visibility = Visibility.Visible;
            }
        }

        private void btn_Takaisin_Lisays_Click(object sender, RoutedEventArgs e)
        {
            Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
            Perustiedot_Grid.Visibility = Visibility.Visible;

        }

        private void btn_Lisaa_Naytos_Click(object sender, RoutedEventArgs e)
        {
            dg_Lisattavat_Naytokset.Items.Add(new
            {
                Elokuvateatteri = txt_Elokuvateatteri.Text,
                Pvm = datep_Naytoksen_pvm.Text,
                Klo = txt_Aika.Text
            });

            /*Näytös naytos = new Näytös();
            Elokuvasali sali = new Elokuvasali();
            Teatteri teatteri = new Teatteri();
            teatteri.Nimi = txt_Elokuvateatteri.Text;
            teatteri.Kaupunki = "Turku"; //debuggausta

            sali.Teatteri = teatteri;

            naytos.Elokuva = this.lisattavaElokuva;
            naytos.Sali = sali;*/

        }

        private void btn_Poista_Lisattava_Naytos_Click(object sender, RoutedEventArgs e)
        {
            var myNaytokset = dg_Lisattavat_Naytokset;
            var naytokset = dg_Lisattavat_Naytokset;

            if (myNaytokset.SelectedItems.Count >= 1)
            {
                for (int i = 0; i < myNaytokset.SelectedItems.Count; i++)
                {
                    naytokset.Items.Remove(myNaytokset.SelectedItems[i]);
                }
            }

            myNaytokset = naytokset;
        }

        private async void btn_Lisaa_Elokuva_Click_(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvateatteri.Equals(null) || datep_Naytoksen_pvm.Text.Equals(null) || txt_Aika.Text.Equals(null))
            {
                lbl_lisays_ilmoitus.Foreground = red;
                lbl_lisays_ilmoitus.Content = "Tarvittavia tietoja puuttuu! Tarkista tiedot";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
            else
            {
                lisattavatNaytokset = dg_Lisattavat_Naytokset.Items.Cast<Näytös>().ToList();
                lisaaElokuvaTietokantaan(this.lisattavaElokuva, this.lisattavatNaytokset);
                lbl_lisays_ilmoitus.Foreground = white;
                lbl_lisays_ilmoitus.Content = "Elokuvan lisääminen onnistui. Palataan alkuun...";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
        }

        private void ValittuTab(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;
            if (tab != null)
            {
                // this tab is selected!
                if (tab.Name == "YllapidonEtusivuTab")
                {
                    paivitaElokuvatDG();

                    dg_Elokuvat.Items.Add(new
                    {
                        ElokuvaID = "1",
                        Elokuvan_Nimi = "Ihmeotukset ja niiden olinpaikat",
                        Elokuvan_Vuosi = "2016",
                        Elokuvan_Kesto = "120",
                        Elokuvan_Kuvaus = "Lorem ipsum dolor sit amet,consectetur adipiscing elit"
                    });
                }
            }
        }

        private void paivitaElokuvatDG()
        {
            dg_Elokuvat.Items.Clear();
            //Haetaan elokuvat tietokannasta
        }

        private void rekisteroiKayttaja(String nimi, String salasana)
        {

        }

        private void lisaaElokuvaTietokantaan(Elokuva elokuva, List<Näytös> naytokset)
        {

        }

        private void haeElokuva(Elokuva elokuva)
        {

        }

        //Poistaa elokuvan ja siihen liittyvät näytökset tietokannasta
        private void poistaElokuva(Elokuva elokuva)
        {

        }

        private void dg_Elokuvat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var elokuva = dg_Elokuvat.SelectedItem;
           // haeElokuva((Elokuva)elokuva);

            lbl_Naytokset.Visibility = Visibility.Visible;
            dg_Naytokset.Visibility = Visibility.Visible;
            btn_Avaa_Elokuvan_Muokkaus.Visibility = Visibility.Visible;
            btn_Poista_Elokuva.Visibility = Visibility.Visible;
        }

        private void btn_Poista_Elokuva_Click(object sender, RoutedEventArgs e)
        {
            var elokuva = dg_Elokuvat.SelectedItem;
            MessageBoxResult varmistus = System.Windows.MessageBox.Show("Haluatko varmasti poistaa elokuvan: " + elokuva.ToString(), "Delete Confirmation", System.Windows.MessageBoxButton.OKCancel);
            if (varmistus == MessageBoxResult.OK)
            {
                //poistaElokuva(elokuva);
                dg_Elokuvat.Items.Remove(elokuva); //Testaamista varten
                //paivitaElokuvatDG();
            }

        }

        private void btn_Avaa_Elokuvan_Muokkaus_Click(object sender, RoutedEventArgs e)
        {
            Perustiedot_Grid.Visibility = Visibility.Collapsed;
            Perustietojen_Paivitys_Grid.Visibility = Visibility.Visible;
            Lisaa_Elokuva_Tab.IsSelected = true;
        }

        private async void btn_Paivitys_Seuraava_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_Nimi.Text.Equals("") || txt_Vuosi.Text.Equals("") || txt_Kesto.Text.Equals("") || txt_Kuvaus.Text.Equals(""))
            {
                lbl_lisays_ilmoitus.Foreground = red;
                lbl_lisays_ilmoitus.Content = "Vaadittavia tietoja puuttuu! Tarkista tiedot";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
            else
            {
                elokuvanNimi = txt_Elokuvan_NimiP.Text;
                elokuvanVuosi = Int32.Parse(txt_VuosiP.Text);
                elokuvanKesto = Int32.Parse(txt_KestoP.Text);
                elokuvanKuvaus = txt_KuvausP.Text;

                lisattavaElokuva = new Elokuva(elokuvanNimi, elokuvanKesto, elokuvanKuvaus);

                Perustietojen_Paivitys_Grid.Visibility = Visibility.Collapsed;
                Naytosten_Paivitys_Grid.Visibility = Visibility.Visible;
            }
        }

        private void btn_Lisaa_NaytosP_Click(object sender, RoutedEventArgs e)
        {
            dg_Paivitettavat_Naytokset.Items.Add(new
            {
                Elokuvateatteri = txt_ElokuvateatteriP.Text,
                Pvm = datep_Naytoksen_pvmP.Text,
                Klo = txt_AikaP.Text
            });
        }

        private void btn_Paivita_Naytos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Poista_Valittu_NaytosP_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Takaisin_Paivitys_Click(object sender, RoutedEventArgs e)
        {
            Naytosten_Paivitys_Grid.Visibility = Visibility.Collapsed;
            Perustietojen_Paivitys_Grid.Visibility = Visibility.Visible;
        }

        private void btn_Paivita_Elokuva_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
