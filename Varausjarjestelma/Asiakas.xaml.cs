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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
namespace Varausjarjestelma
{
    /// Käyttäjän näkymä lipunvarausjärjestelmään
    /// TabControlia käyttäen siirtymät eteen-/taaksepäin
    /// 
    public partial class Asiakas : Window
    {
        private Tietokanta _tietokanta;
        private Kayttaja kayttaja;

        public Asiakas(Kayttaja k)
        {
            kayttaja = k;

            InitializeComponent();

            // toistaiseksi näin, lopullisessa pärjää varmaan yhdellä instanssilla
            _tietokanta = new Tietokanta();

            // välilehdet piiloon
            foreach (TabItem t in tabControl.Items.OfType<TabItem>())
            {
                t.Visibility = Visibility.Collapsed;
            }
        }

        protected void Etusivulle(object sender, EventArgs e)
        {
            etusivu.IsSelected = true;
        }

        // siirry suoraan seuraavaan välilehteen
        private void Button_Seuraava(object sender, RoutedEventArgs e)
        {
            int newIndex = tabControl.SelectedIndex + 1;
            if (newIndex >= tabControl.Items.Count) newIndex = 0;
            tabControl.SelectedIndex = newIndex;
        }

        // siirry suoraan edelliseen välilehteen
        private void Button_Edellinen(object sender, RoutedEventArgs e)
        {
            int newIndex = tabControl.SelectedIndex - 1;
            if (newIndex < 0) newIndex = tabControl.Items.Count - 1;
            tabControl.SelectedIndex = newIndex;
        }

        // Allaolevat suoraan käyttöliittymissä vastaaviin OnClick=""
        // Button_SelaaElokuvia, Button_VaraaNäytös, Button_VaraaPaikat, Button_VahvistaVaraus ja Button_TeeVaraus

        private void Button_SelaaElokuvia(object sender, RoutedEventArgs e)
        {
            // tietokanta: Hae lista elokuvista
            Ohjelmisto.ItemsSource = _tietokanta.GetElokuvat(); // tämä tietokannasta

            Siirry("selaa_elokuvia");
        }

        private void Button_VaraaNäytös(object sender, RoutedEventArgs e)
        {
            // tietokanta: Hae näytökset listausta varten sellaisiin esityksiin jotka ovat tulevaisuudessa

            Elokuva valittu_elokuva = Ohjelmisto.SelectedItem as Elokuva;
            List<Näytös> näytökset = _tietokanta.Näytökset(valittu_elokuva);

            TulevatNäytökset.ItemsSource = näytökset;
            Siirry("varaa_näytös");
        }

        private void Button_VaraaPaikat(object sender, RoutedEventArgs e)
        {
            Näytös n = (TulevatNäytökset.SelectedItem as Näytös);
            Elokuvasali s = n.Sali;

            List<Paikka> varatutPaikat = _tietokanta.VaratutPaikat(n);
            ValitsePaikat.AlustaVarauksilla(s, varatutPaikat);

            Siirry("varaa_paikat");
        }

        private void Button_VahvistaVaraus(object sender, RoutedEventArgs e)
        {
            VahvistaPaikat.ItemsSource = ValitsePaikat.ValitutPaikat.OrderBy(p => p.PaikkaNro); // kivan näköistä
            Siirry("varaa_vahvista");
        }

        private void Button_TeeVaraus(object sender, RoutedEventArgs e)
        {
            Näytös n = TulevatNäytökset.SelectedItem as Näytös;

            foreach (Paikka p in ValitsePaikat.ValitutPaikat)
            {
                _tietokanta.VaraaPaikka(kayttaja, n, p);
                System.Windows.MessageBox.Show("Lippu paikalle: " + p.PaikkaNro.ToString() + "\n(rivi: " + p.Rivi.ToString() + ", paikka: " + p.PaikkaRivissä.ToString() + ")");
            }

            Siirry("varaa_kiitos");
        }

        // Siirry nimettyyn välilehteen
        // nimi siis x:Name="nimi"
        //
        private void Siirry(string nimi)
        {
            TabItem kohde = tabControl.Items.OfType<TabItem>().SingleOrDefault(n => n.Name == nimi);

            Debug.Assert(kohde != null);
            tabControl.SelectedItem = kohde;
        }

        // esim voi käyttää <Button Tag="kohde"/>
        // kohde oltava välilehden (TabItem) x:Name="kohde"
        //
        private void Button_Siirry(object sender, RoutedEventArgs e)
        {
            string nimi = (sender as Button).Tag.ToString();
            Siirry(nimi);
        }

        private void Button_KirjauduUlos(object sender, RoutedEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

    }
}
