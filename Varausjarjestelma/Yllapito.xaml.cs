using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.DataGrid;
using System.Windows.Threading;

namespace Varausjarjestelma
{
    /// <summary>
    /// Interaction logic for Yllapito.xaml
    /// </summary>
    public partial class Yllapito : Window
    {
        private Tietokanta tietokanta;
        private List<Elokuva> kaikkiElokuvat;
        private Elokuva lisattavaElokuva;
        private List<Näytös> elokuvanNaytokset;
        private List<Näytös> lisattavatNaytokset;

        private String elokuvanNimi;
        private int elokuvanVuosi;
        private int elokuvanKesto;
        private String elokuvanKuvaus;
        private DateTime aika;
        private DispatcherTimer ajastin;
        private List<int> poistettavatIndeksit;

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Yllapito()
        {
            InitializeComponent();
            tietokanta = new Tietokanta();
            ajastin = new DispatcherTimer();
            poistettavatIndeksit = new List<int>();
            kaikkiElokuvat = tietokanta.GetElokuvat();
            elokuvanNaytokset = new List<Näytös>();
            dg_Elokuvat.ItemsSource = kaikkiElokuvat;
            lisattavatNaytokset = new List<Näytös>();
        }

        #region tietokantametodit
        //Lisää elokuvan tietokantaan
        //Palauttaa true jos onnistuu
        private bool lisaaElokuvaTietokantaan(Elokuva elokuva, List<Näytös> naytokset)
        {
            tietokanta.SetElokuva(elokuva);

            foreach (Näytös naytos in naytokset)
            {
                tietokanta.Ajasql("INSERT INTO naytokset(elokuvannimi, aika, sali, teatteri) VALUES ('" + elokuva.Nimi + "', '" + naytos.Aika + "', '" + naytos.Sali.Nimi + "', '" + naytos.Teatteri.Nimi + "')");
            }

            return true;
        }

        //Poistaa elokuvan ja siihen liittyvät näytökset tietokannasta
        //Palauttaa true jos onnistuu
        private bool poistaElokuva(Elokuva elokuva)
        {
            return true;
        }

        //Etsii ja palauttaa halutun elokuvan tietokannasta
        private Elokuva haeElokuva(Elokuva elokuva)
        {
            return null;
        }

        //Etsii ja palauttaa elokuvaan liittyvät näytökset
        private List<Näytös> haeElokuvanNaytokset(Elokuva elokuva)
        {
            return null;
        }

        //Hakee kaikki elokuvateatterit tietokannasta
        private List<Teatteri> haeTeatterit()
        {
            return null;
        }

        //Hakee kaikki tietokannassa olevat elokuvasalit
        private List<Elokuvasali> haeElokuvaSalit()
        {
            return tietokanta.GetElokuvasalit();
        }

        //Päivittää annetun elokuvan tiedot tietokannassa
        //Palauttaa true jos onnistuu
        private bool PaivitaElokuva(Elokuva elokuva)
        {
            tietokanta.DelElokuva(elokuva);
            tietokanta.SetElokuva(elokuva);
            kaikkiElokuvat = tietokanta.GetElokuvat();
            return true;
        }

        //Päivittää annetun elokuvan näytökset
        //Palauttaa true jos onnistuu
        private bool paivitaElokuvanNaytokset(Elokuva elokuva, List<Näytös> elokuvanNäytökset)
        {
            return true;
        }

        #endregion tietokantametodit
        #region yleisetUImetodit

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

        //Toiminnot Enter-painikkeelle
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Kirjaudu_Ulos_Tab.IsSelected)
            {
                btn_kirjaudu_ulos_Click(sender, e);
            }
        }

        //Metodi, joka muotoilee UI-elementin vastaanottamaan vain numeroita
        private void vainNumeroita(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ValittuTab(object sender, RoutedEventArgs e)
        {
            var tab = sender as TabItem;
            if (tab != null)
            {
                //toiminnot, jotka suoritetaan kun joku tabi valitaan
                if (tab.Name == "YllapidonEtusivuTab")
                {
                    paivitaElokuvatDG();
                }

                if (tab.Name == "Lisaa_Elokuva_Tab ")
                {

                }
            }
        }
        #endregion UImetodit
        #region etusivu
        private void paivitaElokuvatDG()
        {
            //Haetaan elokuvat tietokannasta
            kaikkiElokuvat = tietokanta.GetElokuvat();
            dg_Elokuvat.ItemsSource = kaikkiElokuvat;
        }

        private void dg_Elokuvat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var elokuva = dg_Elokuvat.SelectedItem;

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
        #endregion
        #region elokuvanLisays

        private void btn_Lisaa_Elokuvan_Perustiedot_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_Nimi.Text.Equals("") || txt_Vuosi.Text.Equals("") || txt_Kesto.Text.Equals("") || txt_Kuvaus.Text.Equals(""))
            {
                tulostaIlmoitus("Vaadittavia tietoja puuttuu! Tarkista tiedot", lbl_lisays_ilmoitus, true);
            }
            else
            {
                elokuvanNimi = txt_Elokuvan_Nimi.Text;
                elokuvanVuosi = Int32.Parse(txt_Vuosi.Text);
                elokuvanKesto = Int32.Parse(txt_Kesto.Text);
                elokuvanKuvaus = txt_Kuvaus.Text;

                lisattavaElokuva = new Elokuva(elokuvanNimi, elokuvanVuosi, elokuvanKesto, elokuvanKuvaus, "Kylla");

                Perustiedot_Grid.Visibility = Visibility.Collapsed;
                Naytokset_Lisays_Grid.Visibility = Visibility.Visible;
            }
        }

        private void btn_Takaisin_Lisays_Click(object sender, RoutedEventArgs e)
        {
            Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
            Perustiedot_Grid.Visibility = Visibility.Visible;
        }

        private void cmb_Elokuvateatteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElokuvaT1.Content.Equals("Elokuvateatteri1"))
            {
                Elokuva1_Sali1.Visibility = Visibility.Visible;
                Elokuva1_Sali2.Visibility = Visibility.Visible;
                Elokuva2_Sali1.Visibility = Visibility.Collapsed;
                Elokuva2_Sali2.Visibility = Visibility.Collapsed;
            }
            else if (ElokuvaT1.Content.Equals("Elokuvateatteri2"))
            {
                Elokuva2_Sali1.Visibility = Visibility.Visible;
                Elokuva2_Sali2.Visibility = Visibility.Visible;
                Elokuva1_Sali1.Visibility = Visibility.Collapsed;
                Elokuva1_Sali2.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_Lisaa_Naytos_Click(object sender, RoutedEventArgs e)
        {
            Näytös naytos = new Näytös();
            Teatteri teatteri = new Teatteri(cmb_Elokuvateatteri.Text, "Turku");
            Elokuvasali sali = new Elokuvasali(cmb_Salit.Text, 20, 10, teatteri);

            naytos.Elokuva = this.lisattavaElokuva;
            naytos.Sali = sali;

            aika = Convert.ToDateTime(datep_Naytoksen_aika.Text);
            naytos.Aika = aika;

            lisattavatNaytokset.Add(naytos);

            dg_Lisattavat_Naytokset.Items.Add(new
            {
                Elokuvateatteri = teatteri.Nimi,
                Sali = naytos.Sali.Nimi,
                Pvm = naytos.Aika.ToShortDateString(),
                Klo = naytos.Aika.ToShortTimeString()
            });

        }

        private void btn_Poista_Lisattava_Naytos_Click(object sender, RoutedEventArgs e)
        {
            int naytoksenIndeksi = dg_Lisattavat_Naytokset.SelectedIndex;
            lisattavatNaytokset.RemoveAt(naytoksenIndeksi);
            dg_Lisattavat_Naytokset.Items.RemoveAt(naytoksenIndeksi);
        }

        private async void btn_Lisaa_Elokuva_Click_(object sender, RoutedEventArgs e)
        {
            if (cmb_Elokuvateatteri.Equals(null) || datep_Naytoksen_aika.Text.Equals(null))
            {
                tulostaIlmoitus("Tarvittavia tietoja puuttuu! Tarkista tiedot", lbl_Elokuvan_Lisays_Ilmoitus, true);
            }
            else
            {
                lisaaElokuvaTietokantaan(this.lisattavaElokuva, this.lisattavatNaytokset);
                tulostaIlmoitus("Elokuvan lisääminen onnistui. Palataan alkuun...", lbl_Elokuvan_Lisays_Ilmoitus, false);
                await Task.Delay(1000);
                lisattavaElokuva = null;
                txt_Elokuvan_Nimi.Clear();
                txt_Vuosi.Clear();
                txt_Kesto.Clear();
                txt_Kuvaus.Clear();
                cmb_Elokuvateatteri.SelectedIndex = -1;
                cmb_Salit.SelectedIndex = -1;
                datep_Naytoksen_aika.Text = "";
                lisattavatNaytokset.Clear();
                dg_Lisattavat_Naytokset.Items.Clear();
                btn_Takaisin_Lisays_Click(sender, e);
                YllapidonEtusivuTab.IsSelected = true;
            }
        }
        #endregion
        #region elokuvanPaivitys
        private void btn_Paivitys_Seuraava_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_NimiP.Text.Equals("") || txt_VuosiP.Text.Equals("") || txt_KestoP.Text.Equals("") || txt_KuvausP.Text.Equals(""))
            {
                tulostaIlmoitus("Vaadittavia tietoja puuttuu! Tarkista tiedot", lbl_Paivitys_ilmoitus, true);
            }
            else
            {
                elokuvanNimi = txt_Elokuvan_NimiP.Text;
                elokuvanVuosi = Int32.Parse(txt_VuosiP.Text);
                elokuvanKesto = Int32.Parse(txt_KestoP.Text);
                elokuvanKuvaus = txt_KuvausP.Text;

                lisattavaElokuva = new Elokuva(elokuvanNimi, elokuvanVuosi, elokuvanKesto, elokuvanKuvaus, "Kylla");

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
        #endregion

        //Hoitaa käyttäjän kirjautumisen ulos järjestelmästä ja 
        //avaa login-formin ja tyhjentää muuttujat
        private async void btn_kirjaudu_ulos_Click(object sender, RoutedEventArgs e)
        {
            lbl_logout_ilmoitus.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            new Login().Show();
            this.Close();
        }

        private void YllapidonControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*if (e.Source is TabControl && Lisaa_Elokuva_Tab.IsSelected)
            {
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Haluatko varmasti keskeyttää elokuvan lisäyksen ? Tallentamattomat tiedot menetetään.", 
                                                                            "Varoitus", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {

                }
                else
                {

                }*/
            }
        }
    }
}

