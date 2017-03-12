﻿using System;
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

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Yllapito()
        {
            InitializeComponent();
            tietokanta = new Tietokanta();
            kaikkiElokuvat = tietokanta.GetElokuvat();
            elokuvanNaytokset = new List<Näytös>();
            dg_Elokuvat.ItemsSource = kaikkiElokuvat;
            lisattavatNaytokset = new List<Näytös>();
        }

        //Lisää elokuvan tietokantaan
        //Palauttaa true jos onnistuu
        private bool lisaaElokuvaTietokantaan(Elokuva elokuva, List<Näytös> naytokset)
        {
            tietokanta.SetElokuva(elokuva);

            foreach (Näytös naytos in naytokset)
            {
                tietokanta.Ajasql("INSERT INTO naytokset(elokuvannimi, aika, teatteri) VALUES (" + elokuva.Nimi + ", " + naytos.Aika + ", " + naytos.Teatteri + ")");
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
        private bool paivitaElokuva(Elokuva elokuva, List<Näytös> elokuvanNäytökset)
        {
            return true;
        }

        //Päivittää annetun elokuvan näytökset
        //Palauttaa true jos onnistuu
        private bool paivitaElokuvanNaytokset(Elokuva elokuva, List<Näytös> elokuvanNäytökset)
        {
            return true;
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

                lisattavaElokuva = new Elokuva(elokuvanNimi, elokuvanVuosi , elokuvanKesto, elokuvanKuvaus, "Kylla");

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
                naytos,
                Elokuvateatteri = teatteri.Nimi,
                Sali = naytos.Sali.Nimi,
                Pvm = naytos.Aika.ToShortDateString(),
                Klo = naytos.Aika.ToShortTimeString()
            });

        }

        private void btn_Poista_Lisattava_Naytos_Click(object sender, RoutedEventArgs e)
        {

            /*Näytös poistettavaNaytos = dg_Lisattavat_Naytokset.SelectedItem;

            lisattavatNaytokset.Remove(poistettavaNaytos);
            dg_Lisattavat_Naytokset.Items.Remove(poistettavaNaytos);*/

            /*var myNaytokset = dg_Lisattavat_Naytokset;
            var naytokset = dg_Lisattavat_Naytokset;

            if (myNaytokset.SelectedItems.Count >= 1)
            {
                for (int i = 0; i < myNaytokset.SelectedItems.Count; i++)
                {
                    naytokset.Items.Remove(myNaytokset.SelectedItems[i]);
                }
            }

            myNaytokset = naytokset;*/
        }

        private async void btn_Lisaa_Elokuva_Click_(object sender, RoutedEventArgs e)
        {
            if (cmb_Elokuvateatteri.Equals(null) || datep_Naytoksen_aika.Text.Equals(null))
            {
                lbl_lisays_ilmoitus.Foreground = red;
                lbl_lisays_ilmoitus.Content = "Tarvittavia tietoja puuttuu! Tarkista tiedot";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
            else
            {
                lisaaElokuvaTietokantaan(this.lisattavaElokuva, this.lisattavatNaytokset);
                lbl_lisays_ilmoitus.Foreground = white;
                lbl_lisays_ilmoitus.Content = "Elokuvan lisääminen onnistui. Palataan alkuun...";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
                lisattavaElokuva = null;
                lisattavatNaytokset.Clear();
                YllapidonEtusivuTab.IsSelected = true;
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

                    /*foreach(Elokuva elokuva in kaikkiElokuvat)
                    {
                        dg_Elokuvat.Items.Add(new
                        {
                            ElokuvaID = elokuva.Id,
                            Elokuvan_Nimi = elokuva.Nimi,
                            Elokuvan_Vuosi = "2017",
                            Elokuvan_Kesto = elokuva.Kesto,
                            Elokuvan_Kuvaus = elokuva.Kesto
                        });
                    }*/
                }
            }
        }

        private void paivitaElokuvatDG()
        {
            //Haetaan elokuvat tietokannasta
            kaikkiElokuvat = tietokanta.GetElokuvat();
            dg_Elokuvat.ItemsSource = kaikkiElokuvat;

            /*foreach (Elokuva elokuva in kaikkiElokuvat)
            {
                dg_Elokuvat.Items.Add(new
                {
                    ElokuvaID = elokuva.Id,
                    Elokuvan_Nimi = elokuva.Nimi,
                    Elokuvan_Vuosi = "2017",
                    Elokuvan_Kesto = elokuva.Kesto,
                    Elokuvan_Kuvaus = elokuva.Kesto
                });
            }*/
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
    }
}

