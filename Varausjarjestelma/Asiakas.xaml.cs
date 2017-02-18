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
    /// Miten yhteys tietokantaan?
    /// Onko järkevää mielekästä käyttää UserControlia tässä?
    /// 
    public partial class Asiakas : UserControl
    {
        public List<Elokuva> Elokuvat = new List<Elokuva>();

        public Asiakas()
        {
            InitializeComponent();

            // välilehdet piiloon
            //
            foreach (TabItem t in tabControl.Items.OfType<TabItem>())
            {
                t.Visibility = Visibility.Collapsed;
            }
            Ohjelmisto.ItemsSource = this.Elokuvat;
            this.Elokuvat.Add(new Elokuva("Elokuva 1", 163, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ligula felis, tincidunt a maximus quis, vestibulum eu magna. Etiam ac dolor at lectus consectetur tempor id quis felis. In vitae vehicula eros, quis tristique urna. Ut tristique odio urna, vel dapibus felis vestibulum sit amet."));
            this.Elokuvat.Add(new Elokuva("Elokuva: II osa", 163, "Etiam pretium, justo posuere pellentesque egestas, eros sem convallis turpis, \n\n sed fermentum justo ante ut turpis. Proin viverra sed lacus at ultrices. Sed fermentum ultricies gravida. Quisque at bibendum ante, quis porta ipsum."));

        }

        protected void Etusivulle(object sender, EventArgs e)
        {
            etusivu.IsSelected = true;
        }

        private void Seuraava(object sender, RoutedEventArgs e)
        {
            int newIndex = tabControl.SelectedIndex + 1;
            if (newIndex >= tabControl.Items.Count) newIndex = 0;
            tabControl.SelectedIndex = newIndex;
        }

        private void Button_VaraaNäytös(object sender, RoutedEventArgs e)
        {
            // Hae näytökset listausta varten
            Elokuva valittu_elokuva = Ohjelmisto.SelectedItem as Elokuva;

            Siirry("varaa_näytös");
        }

        private void Siirry(string nimi)
        {
            TabItem kohde = tabControl.Items.OfType<TabItem>().SingleOrDefault(n => n.Name == nimi);

            Debug.Assert(kohde != null);
            tabControl.SelectedItem = kohde;

            // mikäli on tarpeen luoda lomakkeen alustamista varten erikseen
            //
            // System.Reflection.MethodInfo alusta = this.GetType().GetMethod("alusta_" + nimi);
            //
            // if (alusta != null)
            // {
            //    alusta.Invoke(this, null);
            // }
        }

        // esim voi käyttää <Button Tag="kohde"/>
        // kohde oltava välilehden (TabItem) x:Name="kohde"
        //
        private void Button_Siirry(object sender, RoutedEventArgs e)
        {
            string nimi = (sender as Button).Tag.ToString();
            Siirry(nimi);
        }

    }
}
