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
        public Asiakas()
        {
            InitializeComponent();

            // välilehdet piiloon
            //
            foreach (TabItem t in tabControl.Items.OfType<TabItem>())
            {
                t.Visibility = Visibility.Collapsed;
            }
        }

        protected void etusivulle(object sender, EventArgs e)
        {
            etusivu.IsSelected = true;
        }

        private void seuraava(object sender, RoutedEventArgs e)
        {
            int newIndex = tabControl.SelectedIndex + 1;
            if (newIndex >= tabControl.Items.Count) newIndex = 0;
            tabControl.SelectedIndex = newIndex;
        }
        

        // esim voi käyttää <Button Tag="kohde"/>
        // kohde oltava välilehden (TabItem) x:Name="kohde"
        //
        private void siirry(object sender, RoutedEventArgs e)
        {
            string nimi = (sender as Button).Tag.ToString();
            TabItem kohde = tabControl.Items.OfType<TabItem>().SingleOrDefault(n => n.Name == nimi);

            Debug.Assert(kohde != null);
            tabControl.SelectedItem = kohde;

            // mikäli on tarpeen luoda lomakkeen alustamista varten erikseen
            //
            //System.Reflection.MethodInfo alusta = this.GetType().GetMethod("alusta_" + nimi);
            //
            //if (alusta != null)
            //{
            //    alusta.Invoke(this, null);
            //}
        }
    }
}
