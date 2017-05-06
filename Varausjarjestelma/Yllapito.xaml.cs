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

        /// <summary>
        /// Metodi joka lisää elokuvan tietokantaan ja 
        /// palauttaa true jos onnistuu
        /// </summary>
        /// <param name="elokuva"></param>
        /// <param name="naytokset"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metodi joka tulostaa varmistusviestin käyttäjälle mikäli käyttäjä
        /// on lisäämässä tai päivittämässä elokuvaa tai elokuvan näytöksiä ja on poistumassa
        /// kyseiseltä sivulta. Käyttäjän vastauksen mukaan joko pysytään sivulla tai siirrytään sieltä pois
        /// </summary>
        /// <param name="valittuIndeksi"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Metodi joka huolehtii siitä mitä tapahtuu kun käyttäjä vaihtaa sivun ja tallentaa 
        /// viimeksi valitun sivun muistiin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YllapidonControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Etusivu
            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 0)
            {
                if (toiminnonTarkistus(0))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                    paivitaElokuvatDG();
                }
                
            }

            //Elokuvan päivitys - sivu
            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 1 && paivitettavaElokuva != null)
            {
                elokuvanVanhaNimi = paivitettavaElokuva.Nimi;
                txt_Elokuvan_NimiP.Text = paivitettavaElokuva.Nimi;
                txt_VuosiP.Text = paivitettavaElokuva.Vuosi.ToString();
                txt_KestoP.Text = paivitettavaElokuva.Kesto.ToString();
                txt_KuvausP.Text = paivitettavaElokuva.Teksti;
            }

            //Käyttäjät - sivu
            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 2)
            {
                if (toiminnonTarkistus(2))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys(); 
                    paivitaKayttajat();
                }
                
            }

            //Ohjeet - sivu
            if (e.Source == YllapidonControl && YllapidonControl.SelectedIndex == 3)
            {
                if (toiminnonTarkistus(3))
                {
                    clearElokuvanLisays();
                    clearElokuvanPaivitys();
                }
            }

            //Kirjaudu ulos - sivu
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

        /// <summary>
        /// Metodi joka tulostaa ilmoituksen haluttuun labeliin sekä ajastaa 
        /// siihen liittyvän animaation
        /// </summary>
        /// <param name="tuloste"></param>
        /// <param name="lbl"></param>
        /// <param name="virheilmoitus"></param>
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

        /// <summary>
        /// Metodi joka antaa toiminnot Enter-painikkeelle
        /// Jos käyttäjä on Kirjaudu Ulos - sivulla niin painamalla Enteriä 
        /// käyttäjä kirjataan ulos ylläpidosta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Kirjaudu_Ulos_Tab.IsSelected)
            {
                btn_kirjaudu_ulos_Click(sender, e);
            }
        }

        /// <summary>
        /// Metodi, joka muotoilee UI-elementin vastaanottamaan vain numeroita
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vainNumeroita(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #endregion UImetodit
        #region etusivu

        //Metodi joka hakee ajantasaiset tiedot elokuvista tietokannasta ja 
        //täyttää elokuvien datagridin niillä
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

        //Metodi, joka hakee elokuvaan liittyvät näytökset tietokannasta ja täyttää
        //näytökset - datagridin niillä sekä antaa elokuvan poisto, muokkaus sekä näytösten  
        //muokkaus - painikkeet käytettäväksi 
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

                btn_Avaa_Elokuvan_Muokkaus.IsEnabled = true;
                btn_Muokkaa_Naytokset.IsEnabled = true;
                btn_Poista_Elokuva.IsEnabled = true;
            }
            else
            {
                dg_Elokuvat.Items.Clear();

                btn_Avaa_Elokuvan_Muokkaus.IsEnabled = false;
                btn_Muokkaa_Naytokset.IsEnabled = false;
                btn_Poista_Elokuva.IsEnabled = false;
            }       
        }

        //Metodi joka avaa Elokuvan lisäys - sivun
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

        //Metodi joka avaa Elokuvan muokkaus - sivun
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

        //Metodi joka poistaa elokuvan järjestelmästä
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

        //Metodi joka avaa Näytösten muokkaus - sivun
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

        /// <summary>
        /// Metodi joka tyhjentää kaikki UI elementit Elokuvan lisays - sivulta
        /// </summary>
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

        /// <summary>
        /// Metodi joka luo uuden Elokuva - olion elokuvan lisäystä varten.
        /// Metodi tarkistaa myös että käyttäjä on antanut kaikki elokuvan tiedot sekä sen
        /// jos elokuva on jo olemassa järjestelmässä. Käyttäjä myös ohjataan Näytösten lisäys - sivulle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metodi, joka ohjaa käyttäjän takaisin elokuvan perustietojen lisäykseen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Takaisin_Lisays_Click(object sender, RoutedEventArgs e)
        {
            Naytokset_Lisays_Grid.Visibility = Visibility.Collapsed;
            Perustiedot_Grid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Metodi joka määrittää sen mikä sali näkyy käyttäjälle kun käyttäjä valitsee teatterin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metodi joka lisää näytöksen Lisättävät näytökset - datagridiin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    sali = new Elokuvasali(cmb_Salit.Text, 18, 10, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri1") && cmb_Salit.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 15, 15, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_Salit.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_Salit.Text, 20, 10, teatteri);
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

        /// <summary>
        /// Metodi joka poistaa valitun näytöksen Lisää näytökset - datagridistä
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metodi joka lisää elokuvan järjestelmään ja ohjaa käyttäjän takaisin etusivulle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// Metodi joka tyhjentää kaikki UI elementit Elokuvan päivitys - sivulta
        /// </summary>
        private void clearElokuvanPaivitys()
        {
            paivitettavaElokuva = null;
            txt_Elokuvan_NimiP.Clear();
            txt_VuosiP.Clear();
            txt_KestoP.Clear();
            txt_KuvausP.Clear();
            txt_Elokuvan_NimiP.Clear();
        }

        /// <summary>
        /// Metodi joka tyhjentää ja hakee ajantasaiset päivitettävät näytökset listasta
        /// </summary>
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
            btn_Paivita_Naytos.IsEnabled = false;
            btn_Poista_Valittu_NaytosP.IsEnabled = false;

        }

        /// <summary>
        /// Metodi joka tyhjentää UI elementit jotka liittyvät
        /// näytöksen lisäämiseen datagridiin Näytösten päivitys - sivulla
        /// </summary>
        private void clearNaytosPLisays()
        {
            cmb_ElokuvateatteriP2.SelectedIndex = -1;
            cmb_SalitP2.SelectedIndex = -1;
            dp_Paivitetty_Aika2.Text = "";
        }

        /// <summary>
        /// Metodi joka lisää näytöksen Päivitettävät näytökset - listaan 
        /// elokuvan näytösten päivittämistä varten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    sali = new Elokuvasali(cmb_SalitP2.Text, 18, 10, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP2.Text.Equals("Sali2"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 15, 15, teatteri);
                }
                else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP2.Text.Equals("Sali1"))
                {
                    sali = new Elokuvasali(cmb_SalitP2.Text, 20, 10, teatteri);
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

        /// <summary>
        /// Metodi joka antaa käyttöön päivitä ja poista näytös - painikkeet kun käyttäjä valitsee 
        /// näytöksen Päivitettävät näytöksen - datagridistä sekä täyttää näytöksen päivitykseen
        /// liittyvät UI elementit valitun näytöksen tiedoilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg_Paivitettavat_Naytokset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clearNaytosPLisays();

            if (dg_Paivitettavat_Naytokset.SelectedIndex != -1)
            {
                btn_Paivita_Naytos.IsEnabled = true;
                btn_Poista_Valittu_NaytosP.IsEnabled = true;

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

        /// <summary>
        /// Metodi joka päivittää käyttäjän valitseman näytöksen uusilla tiedoilla
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Paivita_Naytos_Click(object sender, RoutedEventArgs e)
        {
            Teatteri teatteri = new Teatteri(cmb_ElokuvateatteriP1.Text, "Turku");
            Elokuvasali sali;

            //Luodaan erikokoiset salit teatterin ja salin nimen perusteella
            if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP1.Text.Equals("Sali1"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 18, 10, teatteri);
            }
            else if (teatteri.Nimi.Equals("Teatteri1") && cmb_SalitP1.Text.Equals("Sali2"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 15, 15, teatteri);
            }
            else if (teatteri.Nimi.Equals("Teatteri2") && cmb_SalitP1.Text.Equals("Sali1"))
            {
                sali = new Elokuvasali(cmb_SalitP1.Text, 20, 10, teatteri);
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

        /// <summary>
        /// Metodi joka poistaa näytöksen Päivitettävät näytökset datagridista
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Poista_Valittu_NaytosP_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult varmistus = Xceed.Wpf.Toolkit.MessageBox.Show("Haluatko varmasti poistaa valitun näytöksen?", "Näytöksen poistaminen", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (varmistus == MessageBoxResult.OK)
            {
                paivitettavatNaytokset.RemoveAt(naytoksenIndeksi);
                dg_Paivitettavat_Naytokset.Items.RemoveAt(naytoksenIndeksi);
                paivitaNaytoksetP();
                btn_Paivita_Naytos.IsEnabled = false;
                btn_Poista_Valittu_NaytosP.IsEnabled = false;
            }
        }

        /// <summary>
        /// Metodi joka päivittää elokuvan tiedot järjestelmään
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Paivita_Elokuvan_Perustiedot_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Elokuvan_NimiP.Text.Equals("") || txt_KestoP.Text.Equals("") || txt_VuosiP.Text.Equals("") || txt_KuvausP.Text.Equals(""))
            {
                tulostaIlmoitus("Tarvittavia tietoja puuttuu! Tarkista elokuvan tiedot", lbl_Paivitys_ilmoitus, true);
            }
            else
            {
                Elokuva elokuva = tietokanta.GetElokuva(txt_Elokuvan_NimiP.Text);

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

        /// <summary>
        /// Metodi joka päivittää elokuvan näytökset järjestelmään
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Paivita_Naytokset_Click(object sender, RoutedEventArgs e)
        {
            tietokanta.MuokkaaNaytokset(paivitettavaElokuva, paivitettavatNaytokset);
            tulostaIlmoitus("Näytökset päivitetty. Ladataan...", lbl_Naytokset_Paivitys_Ilmoitus, false);
            await Task.Delay(1000);
            paivitettavaElokuva = null;
            paivitettavatNaytokset = null;
            this.toimintoKesken = false;
            YllapidonEtusivuTab.IsSelected = true;
        }
        #endregion
        #region kayttajat

        /// <summary>
        /// Metodi joka tyhjentää ja hakee ajantasaiset tiedot järjestelmään
        /// rekisteröityneistä käyttäjistä tietokannasta Käyttäjät - datagridiin
        /// </summary>
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

        /// <summary>
        /// Metodi joka ylentää käyttäjän asiakkaasta ylläpitäjäksi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Ylenna_Kayttaja_Click(object sender, RoutedEventArgs e)
        {
            Kayttaja kayttaja = kayttajat[kayttajaIndeksi];
            tietokanta.Ajasql("UPDATE kayttajat SET rooli = 'Admin' WHERE tunnus='" + kayttaja.Tunnus + "'");
            paivitaKayttajat();
            btn_Ylenna_Kayttaja.IsEnabled = false;
        }

        /// <summary>
        /// Metodi joka antaa käyttöön tai ottaa käytöstä Ylennä käyttäjä - painikkeen
        /// Käyttäjät - datagridista valitun käyttäjän mukaan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg_kayttajat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_kayttajat.SelectedIndex != -1)
            {
                kayttajaIndeksi = dg_kayttajat.SelectedIndex;
                Kayttaja kayttaja = kayttajat[kayttajaIndeksi];
                if (kayttaja.Rooli.Equals("User"))
                {
                    btn_Ylenna_Kayttaja.IsEnabled = true;
                }
                else
                {
                    btn_Ylenna_Kayttaja.IsEnabled = false;
                }
            }
        }

        #endregion
        #region kirjauduUlos

        /// <summary>
        /// Metodi joka hoitaa käyttäjän kirjautumisen ulos järjestelmästä ja 
        /// avaa login-formin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

