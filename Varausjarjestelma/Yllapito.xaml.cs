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

namespace Varausjarjestelma
{
    /// <summary>
    /// Interaction logic for Yllapito.xaml
    /// </summary>
    public partial class Yllapito : Window
    {

        private Elokuva lisattavaElokuva;
        private List<Näytös> lisattavatNaytokset;

        private String elokuvanNimi;
        private int elokuvanVuosi;
        private int elokuvanKesto;
        private String elokuvanKuvaus;

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Yllapito()
        {
            InitializeComponent();
        }

        //Toiminnot Enter-painikkeelle
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Kirjaudu_Ulos_Tab.IsSelected)
            {
                btn_kirjaudu_ulos_Click(sender, e);
            }
        }

        //Hoitaa käyttäjän kirjautumisen ulos järjestelmästä ja 
        //avaa login-formin ja tyhjentää muuttujat
        private async void btn_kirjaudu_ulos_Click(object sender, RoutedEventArgs e)
        {
            lbl_logout_ilmoitus.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            new Login().Show();
            this.Close();
        }

        //Metodi, joka muotoilee UI-elementin vastaanottamaan vain numeroita
        private void vainNumeroita(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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
            /*Näytös naytos = new Näytös();
            Elokuvasali sali = new Elokuvasali();
            Teatteri teatteri = new Teatteri();
            teatteri.Nimi = txt_Elokuvateatteri.Text;
            teatteri.Kaupunki = "Turku"; //debuggausta

            sali.Teatteri = teatteri;

            naytos.Elokuva = this.lisattavaElokuva;
            naytos.Sali = sali;*/

            dg_Lisattavat_Naytokset.Items.Add(new
             {
                 Elokuvateatteri = txt_Elokuvateatteri.Text,
                 Pvm = datep_Naytoksen_pvm.Text,
                 Klo = txt_Aika.Text
             });

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
                //toiminnot, jotka suoritetaan kun joku tabi valitaan
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

        //Lisää käyttäjän tietokantaan
        ////Palauttaa true:n jos onnistuu
        private bool rekisteroiKayttaja(String nimi, String salasana)
        {
            return true;
        }

        //Lisää elokuvan tietokantaan
        //Palauttaa true:n jos onnistuu
        private bool lisaaElokuvaTietokantaan(Elokuva elokuva, List<Näytös> naytokset)
        {
            return true;
        }

        //Etsii ja palauttaa halutun elokuvan tietokannasta
        private Elokuva haeElokuva(Elokuva elokuva)
        {
            return null;
        }

        //Poistaa elokuvan ja siihen liittyvät näytökset tietokannasta
        //Palauttaa true:n jos onnistuu
        private bool poistaElokuva(Elokuva elokuva)
        {
            return true;
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

