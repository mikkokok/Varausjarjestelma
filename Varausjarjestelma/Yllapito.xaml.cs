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
        private Elokuva paivitettavaElokuva;
        private Näytös paivitettavaNaytos;
        private List<Näytös> elokuvanNaytokset;
        private List<Näytös> lisattavatNaytokset;
        private List<Näytös> paivitettavatNaytokset;
        private List<Kayttaja> kayttajat;

        private String elokuvanNimi;
        private int elokuvanVuosi;
        private int elokuvanKesto;
        private String elokuvanKuvaus;
        private int naytoksenIndeksi;
        private DateTime aika;
        private DispatcherTimer ajastin;
        private int kayttajaIndeksi;
        private int elokuvaIndeksi;
        private string elokuvanVanhaNimi;
        private Boolean toimintoKesken = false;
        private TabItem viimeksiValittu;

        private SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private SolidColorBrush white = new SolidColorBrush(Colors.White);

        public Yllapito()
        {
            InitializeComponent();
            tietokanta = new Tietokanta();
            ajastin = new DispatcherTimer();        
            elokuvanNaytokset = new List<Näytös>();
            lisattavatNaytokset = new List<Näytös>();
            viimeksiValittu = null;
            paivitaElokuvatDG();
            paivitaKayttajat();
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

        #endregion tietokantametodit
        #region yleisetUImetodit

        private bool toiminnonTarkistus(int valittuIndeksi)
        {
            bool voikoJatkaa = true;

            if (viimeksiValittu == Lisaa_Elokuva_Tab && toimintoKesken)
            {

                MessageBoxResult vastaus = Xceed.Wpf.Toolkit.MessageBox.Show("Haluatko varmasti poistua? Tallentamattomat tiedot katoavat jos poistut nyt", "Viesti", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (vastaus == MessageBoxResult.OK)
                {
                    toimintoKesken = false;
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                    YllapidonControl.SelectedIndex = valittuIndeksi;
                    voikoJatkaa = true;
                }

                else if (vastaus == MessageBoxResult.Cancel)
                {
                    YllapidonControl.SelectedItem = viimeksiValittu;
                    voikoJatkaa = false;
                }
            }
            return voikoJatkaa;
        }

        private void YllapidonControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 0)
            {
                if (toiminnonTarkistus(0))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                    paivitaElokuvatDG();
                }
                
            }

            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 1 && paivitettavaElokuva != null)
            {
                elokuvanVanhaNimi = paivitettavaElokuva.Nimi;
                txt_Elokuvan_NimiP.Text = paivitettavaElokuva.Nimi;
                txt_VuosiP.Text = paivitettavaElokuva.Vuosi.ToString();
                txt_KestoP.Text = paivitettavaElokuva.Kesto.ToString();
                txt_KuvausP.Text = paivitettavaElokuva.Teksti;
            }

            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 2)
            {
                if (toiminnonTarkistus(2))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys(); 
                    paivitaKayttajat();
                }
                
            }

            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 3)
            {
                if (toiminnonTarkistus(3))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                }
            }

            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 4)
            {
                if (toiminnonTarkistus(4))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                }
            }

            viimeksiValittu = this.YllapidonControl.SelectedItem as TabItem;
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

            TextBlock teksti = new TextBlock();
            teksti.TextWrapping = TextWrapping.Wrap;
            teksti.Text = tuloste;

            lbl.Content = teksti;         
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
        #endregion UImetodit
        #region etusivu
        private void paivitaElokuvatDG()
        {
            dg_Elokuvat.Items.Clear();
            kaikkiElokuvat = tietokanta.GetElokuvat();

            foreach (Elokuva elokuva in kaikkiElokuvat)
            {
                dg_Elokuvat.Items.Add(new
                {
                    ElokuvanNimi = elokuva.Nimi,
                    Vuosi = elokuva.Vuosi,
                    Kesto = elokuva.Kesto,
                    Kuvaus = elokuva.Teksti,
                    Ohjelmistossa = elokuva.Ohjelmistossa
                });
            }
        }

        private void dg_Elokuvat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dg_Naytokset.Items.Clear();
            if (dg_Elokuvat.SelectedIndex != -1)
            {
                elokuvaIndeksi = dg_Elokuvat.SelectedIndex;

                Elokuva elokuva = kaikkiElokuvat[elokuvaIndeksi];
                elokuvanNaytokset = tietokanta.GetElokuvanNaytokset(elokuva);

                foreach (Näytös naytos in elokuvanNaytokset)
                {
                    dg_Naytokset.Items.Add(new
                    {
                        Teatteri = naytos.Teatteri.Nimi,
                        Sali = naytos.Sali.Nimi,
                        Pvm = naytos.Aika.ToShortDateString(),
                        Klo = naytos.Aika.ToShortTimeString()
                    });
                }

                btn_Avaa_Elokuvan_Muokkaus.Visibility = Visibility.Visible;
                btn_Muokkaa_Naytokset.Visibility = Visibility.Visible;
                btn_Poista_Elokuva.Visibility = Visibility.Visible;
            }
            else
            {
                dg_Elokuvat.Items.Clear();

                btn_Avaa_Elokuvan_Muokkaus.Visibility = Visibility.Collapsed;
                btn_Muokkaa_Naytokset.Visibility = Visibility.Collapsed;
                btn_Poista_Elokuva.Visibility = Visibility.Collapsed;
            }       
        }

        private void btn_Avaa_Elokuvan_Lisays_Click(object sender, RoutedEventArgs e)
        {
            paivitettavaElokuva = null;
            Perustiedot_Grid.Visibility = Visibility.Visible;
            Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
            Perustietojen_Paivitys_Grid.Visibility = Visibility.Collapsed;
            Naytosten_Paivitys_Grid.Visibility = Visibility.Collapsed;
            this.toimintoKesken = true;
            Lisaa_Elokuva_Tab.IsSelected = true;
        }

        private void btn_Avaa_Elokuvan_Muokkaus_Click(object sender, RoutedEventArgs e)
        {
            if (elokuvaIndeksi != -1)
            {
                paivitettavaElokuva = kaikkiElokuvat[elokuvaIndeksi];
                Perustiedot_Grid.Visibility = Visibility.Collapsed;
                Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
                Perustietojen_Paivitys_Grid.Visibility = Visibility.Visible;
                this.toimintoKesken = true;
                Lisaa_Elokuva_Tab.IsSelected = true;
            }

        }

        private void btn_Poista_Elokuva_Click(object sender, RoutedEventArgs e)
        {           
            if (elokuvaIndeksi != -1)
            {
                Elokuva elokuva = kaikkiElokuvat[elokuvaIndeksi];
                MessageBoxResult varmistus = Xceed.Wpf.Toolkit.MessageBox.Show("Haluatko varmasti poistaa elokuvan: " + elokuva.Nimi, "Elokuvan poistaminen", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (varmistus == MessageBoxResult.OK)
                {
                    dg_Elokuvat.Items.RemoveAt(elokuvaIndeksi);
                    tietokanta.DelElokuvanNaytos(elokuva);
                    tietokanta.DelElokuva(elokuva);
                    paivitaElokuvatDG();
                }
            }
           
        }

        private void btn_Muokkaa_Naytokset_Click(object sender, RoutedEventArgs e)
        {
            if (elokuvaIndeksi != -1)
            {
                paivitettavaElokuva = kaikkiElokuvat[elokuvaIndeksi];
                Naytosten_Paivitys_Grid.Visibility = Visibility.Visible;
                Perustietojen_Paivitys_Grid.Visibility = Visibility.Collapsed;
                Perustiedot_Grid.Visibility = Visibility.Collapsed;
                Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
                paivitettavatNaytokset = tietokanta.GetElokuvanNaytokset(paivitettavaElokuva);
                paivitaNaytoksetP();
                this.toimintoKesken = true;
                Lisaa_Elokuva_Tab.IsSelected = true;
            }

        }

        #endregion
        #region elokuvanLisays

        private void clearElokuvanLisays()
        {
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
        }

        private void btn_Lisaa_Elokuvan_Perustiedot_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_Nimi.Text.Equals("") || txt_Vuosi.Text.Equals("") || txt_Kesto.Text.Equals("") || txt_Kuvaus.Text.Equals(""))
            {
                tulostaIlmoitus("Vaadittavia tietoja puuttuu! Tarkista tiedot", lbl_lisays_ilmoitus, true);
            }
            else
            {
                Elokuva elokuva = tietokanta.GetElokuva(txt_Elokuvan_Nimi.Text);

                if(!elokuva.Nimi.Equals(""))
                {
                    tulostaIlmoitus("Elokuva on jo olemassa. Valitse toinen nimi elokuvalle", lbl_lisays_ilmoitus, true);
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
        }

        private void btn_Takaisin_Lisays_Click(object sender, RoutedEventArgs e)
        {
            Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
            Perustiedot_Grid.Visibility = Visibility.Visible;
        }

        private void cmb_Elokuvateatteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmb_Elokuvateatteri.SelectedIndex != -1)
            {
                Sali1.Visibility = Visibility.Visible;
                Sali2.Visibility = Visibility.Visible;
            }
            else
            {
                Sali1.Visibility = Visibility.Collapsed;
                Sali2.Visibility = Visibility.Collapsed;
            }

        }

        private void btn_Lisaa_Naytos_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_Elokuvateatteri.Text.Equals("") || cmb_Salit.Equals("") || datep_Naytoksen_aika.Text.Equals(""))
            {
                tulostaIlmoitus("Tarvittavat näytöksen tiedot puuttuvat!", lbl_Elokuvan_Lisays_Ilmoitus, true);
            }
            else
            {
                Teatteri teatteri = new Teatteri(cmb_Elokuvateatteri.Text, "Turku");
                Elokuvasali sali;

                //Luodaan erikokoiset salit teatterin ja salin nimen perusteella
                if (teatteri.Nimi.Equals("Teatteri1") && cmb_Salit.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 20, 10, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri1") && cmb_Salit.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 15, 25, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_Salit.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 25, 15, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_Salit.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 10, 10, teatteri);
                }
                else
                {
                    sali = null;
                }

                aika = Convert.ToDateTime(datep_Naytoksen_aika.Text);

                Näytös naytos = new Näytös(this.lisattavaElokuva, aika, sali, teatteri);
                lisattavatNaytokset.Add(naytos);

                dg_Lisattavat_Naytokset.Items.Add(new
                {
                    Elokuvateatteri = naytos.Teatteri.Nimi,
                    Sali = naytos.Sali.Nimi,
                    Pvm = naytos.Aika.ToShortDateString(),
                    Klo = naytos.Aika.ToShortTimeString()
                });
            }

        }

        private void btn_Poista_Lisattava_Naytos_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Lisattavat_Naytokset.SelectedIndex != -1)
            {
                naytoksenIndeksi = dg_Lisattavat_Naytokset.SelectedIndex;
                lisattavatNaytokset.RemoveAt(naytoksenIndeksi);
                dg_Lisattavat_Naytokset.Items.RemoveAt(naytoksenIndeksi);
            }
            else
            {
                tulostaIlmoitus("Et valinnut poistettavaa näytöstä. Valitse poistettava näytös listasta", lbl_Elokuvan_Lisays_Ilmoitus, true);
            }

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
                btn_Takaisin_Lisays_Click(sender, e);
                this.toimintoKesken = false;
                YllapidonEtusivuTab.IsSelected = true;
            }
        }
        #endregion
        #region elokuvanPaivitys

        private void clearElokuvanPaivitys()
        {
            paivitettavaElokuva = null;
            txt_Elokuvan_NimiP.Clear();
            txt_VuosiP.Clear();
            txt_KestoP.Clear();
            txt_KuvausP.Clear();
            txt_Elokuvan_NimiP.Clear();
        }

        private void paivitaNaytoksetP()
        {
            cmb_ElokuvateatteriP1.SelectedIndex = -1;
            cmb_SalitP1.SelectedIndex = -1;
            dp_Paivitetty_Aika.Text = "";
            dg_Paivitettavat_Naytokset.Items.Clear();
            foreach (Näytös naytos in paivitettavatNaytokset)
            {
                dg_Paivitettavat_Naytokset.Items.Add(new
                {
                    Teatteri = naytos.Teatteri.Nimi,
                    Sali = naytos.Sali.Nimi,
                    Pvm = naytos.Aika.ToShortDateString(),
                    Klo = naytos.Aika.ToShortTimeString()
                });
            }
        }

        private void clearNaytosPLisays()
        {
            cmb_ElokuvateatteriP2.SelectedIndex = -1;
            cmb_SalitP2.SelectedIndex = -1;
            dp_Paivitetty_Aika2.Text = "";
        }

        private void btn_Lisaa_NaytosP_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_ElokuvateatteriP2.Text.Equals("") || cmb_SalitP2.Text.Equals("") || dp_Paivitetty_Aika2.Text.Equals(""))
            {
                tulostaIlmoitus("Tarvittavia tietoja puuttuu näytöksestä. Tarkista tiedot.", lbl_Naytokset_Paivitys_Ilmoitus, true);
            }
            else
            {
                Teatteri teatteri = new Teatteri(cmb_ElokuvateatteriP2.Text, "Turku");
                Elokuvasali sali;

                //Luodaan erikokoiset salit teatterin ja salin nimen perusteella
                if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP2.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 20, 10, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP2.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 15, 25, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP2.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 25, 15, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP2.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 10, 10, teatteri);
                }
                else
                {
                    sali = null;
                }

                aika = Convert.ToDateTime(dp_Paivitetty_Aika2.Text);

                Näytös naytos = new Näytös(this.paivitettavaElokuva, aika, sali, teatteri);
                paivitettavatNaytokset.Add(naytos);
                paivitaNaytoksetP();
                clearNaytosPLisays();
            }

        }

        private void dg_Paivitettavat_Naytokset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clearNaytosPLisays();

            if (dg_Paivitettavat_Naytokset.SelectedIndex != -1)
            {
                btn_Paivita_Naytos.Visibility = Visibility.Visible;
                btn_Poista_Valittu_NaytosP.Visibility = Visibility.Visible;

                naytoksenIndeksi = dg_Paivitettavat_Naytokset.SelectedIndex;
                paivitettavaNaytos = paivitettavatNaytokset[naytoksenIndeksi];

                if (paivitettavaNaytos.Teatteri.Nimi.Equals("Teatteri1"))
                {
                    cmb_ElokuvateatteriP1.SelectedIndex = 0;
                }
                else
                {
                    cmb_ElokuvateatteriP1.SelectedIndex = 1;
                }

                if (paivitettavaNaytos.Sali.Nimi.Equals("Sali1"))
                {
                    cmb_SalitP1.SelectedIndex = 0;
                }
                else
                {
                    cmb_SalitP1.SelectedIndex = 1;
                }

                dp_Paivitetty_Aika.Value = paivitettavaNaytos.Aika;
            }

        }

        private void btn_Paivita_Naytos_Click(object sender, RoutedEventArgs e)
        {
            Teatteri teatteri = new Teatteri(cmb_ElokuvateatteriP1.Text, "Turku");
            Elokuvasali sali;

            //Luodaan erikokoiset salit teatterin ja salin nimen perusteella
            if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP1.Text.Equals("Sali1"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 20, 10, teatteri);
            }
            else if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP1.Text.Equals("Sali2"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 15, 25, teatteri);
            }
            else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP1.Text.Equals("Sali1"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 25, 15, teatteri);
            }
            else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP1.Text.Equals("Sali2"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 10, 10, teatteri);
            }
            else
            {
                sali = null;
            }

            aika = Convert.ToDateTime(dp_Paivitetty_Aika.Text);
            paivitettavaNaytos = new Näytös(this.paivitettavaElokuva, aika, sali, teatteri);

            paivitettavatNaytokset.RemoveAt(naytoksenIndeksi);
            paivitettavatNaytokset.Insert(naytoksenIndeksi, paivitettavaNaytos);
            paivitaNaytoksetP();
            clearNaytosPLisays();
        }

        private void btn_Poista_Valittu_NaytosP_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult varmistus = Xceed.Wpf.Toolkit.MessageBox.Show("Haluatko varmasti poistaa valitun näytöksen?", "Näytöksen poistaminen", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (varmistus == MessageBoxResult.OK)
            {
                paivitettavatNaytokset.RemoveAt(naytoksenIndeksi);
                dg_Paivitettavat_Naytokset.Items.RemoveAt(naytoksenIndeksi);
                paivitaNaytoksetP();
                btn_Paivita_Naytos.Visibility = Visibility.Collapsed;
                btn_Poista_Valittu_NaytosP.Visibility = Visibility.Collapsed;
            }
        }

        private async void btn_Paivita_Elokuvan_Perustiedot_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_NimiP.Text.Equals("") || txt_KestoP.Text.Equals("") || txt_VuosiP.Text.Equals("") || txt_KuvausP.Text.Equals(""))
            {
                tulostaIlmoitus("Tarvittavia tietoja puuttuu! Tarkista elokuvan tiedot", lbl_Paivitys_ilmoitus, true);
            }
            else
            {
                Elokuva elokuva = tietokanta.GetElokuva(txt_Elokuvan_NimiP.Text);

                if (!elokuva.Nimi.Equals(""))
                {
                    tulostaIlmoitus("Elokuva on jo olemassa. Valitse toinen nimi elokuvalle", lbl_Paivitys_ilmoitus, true);
                }
                else
                {
                    paivitettavaElokuva.Nimi = txt_Elokuvan_NimiP.Text;
                    paivitettavaElokuva.Vuosi = int.Parse(txt_VuosiP.Text);
                    paivitettavaElokuva.Kesto = int.Parse(txt_KestoP.Text);
                    paivitettavaElokuva.Teksti = txt_KuvausP.Text;

                    tietokanta.UpdateElokuva(paivitettavaElokuva, elokuvanVanhaNimi);
                    tietokanta.Ajasql($"UPDATE naytokset SET elokuvannimi='{paivitettavaElokuva.Nimi}' WHERE elokuvannimi='{elokuvanVanhaNimi}'");

                    tulostaIlmoitus("Elokuvan päivitys onnistui. Ladataan...", lbl_Paivitys_ilmoitus, false);
                    await Task.Delay(1000);
                    paivitettavaElokuva = null;
                    this.toimintoKesken = false;
                    Perustiedot_Grid.Visibility = Visibility.Visible;
                    YllapidonEtusivuTab.IsSelected = true;
                }
            }
        }

        private async void btn_Paivita_Naytokset_Click(object sender, RoutedEventArgs e)
        {
            tietokanta.muokkaaNaytokset(paivitettavaElokuva, paivitettavatNaytokset);
            tulostaIlmoitus("Näytökset päivitetty. Ladataan...", lbl_Naytokset_Paivitys_Ilmoitus, false);
            await Task.Delay(1000);
            paivitettavaElokuva = null;
            paivitettavatNaytokset = null;
            this.toimintoKesken = false;
            YllapidonEtusivuTab.IsSelected = true;
        }
        #endregion
        #region kayttajat

        private void paivitaKayttajat()
        {
            dg_kayttajat.Items.Clear();
            kayttajat = tietokanta.GetKayttajat();
            foreach (Kayttaja kayttaja in kayttajat)
            {

                dg_kayttajat.Items.Add(new
                {
                    Kayttajatunnus = kayttaja.Tunnus,
                    Etunimi = kayttaja.Etunimi,
                    Sukunimi = kayttaja.Sukunimi,
                    Rooli = kayttaja.Rooli
                });
            }
        }

        private void btn_Ylenna_Kayttaja_Click(object sender, RoutedEventArgs e)
        {
            Kayttaja kayttaja = kayttajat[kayttajaIndeksi];
            tietokanta.Ajasql("UPDATE kayttajat SET rooli = 'Admin' WHERE tunnus='" + kayttaja.Tunnus + "'");
            paivitaKayttajat();
            btn_Ylenna_Kayttaja.Visibility = Visibility.Collapsed;
        }

        private void dg_kayttajat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_kayttajat.SelectedIndex != -1)
            {
                kayttajaIndeksi = dg_kayttajat.SelectedIndex;
                Kayttaja kayttaja = kayttajat[kayttajaIndeksi];
                if (kayttaja.Rooli.Equals("User"))
                {
                    btn_Ylenna_Kayttaja.Visibility = Visibility.Visible;
                }
                else
                {
                    btn_Ylenna_Kayttaja.Visibility = Visibility.Collapsed;
                }
            }
        }

        #endregion
        #region kirjauduUlos

        //Hoitaa käyttäjän kirjautumisen ulos järjestelmästä ja 
        //avaa login-formin ja tyhjentää muuttujat
        private async void btn_kirjaudu_ulos_Click(object sender, RoutedEventArgs e)
        {
            lbl_logout_ilmoitus.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            new Login().Show();
            this.Close();
        }

        #endregion
    }
}

