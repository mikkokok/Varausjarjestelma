using System;
using System.Collections.Generic;
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

        private readonly int loginWidth = 300;
        private readonly int loginHeight = 218;

        //Annetut käyttäjänimi ja salasana
        private string username;
        private string password;
        private string repeatedPassword;

        private int rooli = 1; // 0 = asiakas 1 = ylläpitäjä, Testausta varten

        public Login()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            state = ProgramState.YllapidonControl;
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
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.White);
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
                lbl_ilmoitus.Foreground = new SolidColorBrush(Colors.Red);
                lbl_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_ilmoitus.Visibility = Visibility.Collapsed;
                
            }
        }

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
            if (e.Key== Key.Enter && state == ProgramState.Kirjautuminen)
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

        private async void btn_rekisteroi_Click(object sender, RoutedEventArgs e)
        {
            this.username = txt_kayttajaNimiR.Text;
            this.password = txt_salasanaR.Password;
            this.repeatedPassword = txt_salasanan_vahvistus.Password;

            if (this.password == this.repeatedPassword)
            {
                //Rekisteöi käyttäjäjän tietokantaan
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
                lbl_ilmoitusR.Foreground = new SolidColorBrush(Colors.Red);
                lbl_ilmoitusR.Content = "Salasanat eivät täsmää!";
                lbl_ilmoitusR.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_ilmoitusR.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_takaisinR_Click(object sender, RoutedEventArgs e)
        {
            txt_kayttajaNimiR.Clear();
            txt_salasanaR.Clear();
            txt_salasanan_vahvistus.Clear();

            Register_Grid.Visibility = Visibility.Collapsed;
            Login_Grid.Visibility = Visibility.Visible;
            this.Title = "Kirjaudu Sisään";
        }

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
            }
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void resetYllapidonControl()
        {
            YllapidonEtusivu.IsSelected = true;
            lbl_logout_ilmoitus.Visibility = Visibility.Collapsed;
        }

        private async void btn_Lisaa_Elokuva_Click(object sender, RoutedEventArgs e)
        {
            if (txtElokuva_Nimi.Text.Equals("") || txt_Kesto.Text.Equals("") || txt_Kuvaus.Text.Equals("") || cmb_Elokuvateatterit.Text.Equals(""))
            {
                lbl_lisays_ilmoitus.Foreground = new SolidColorBrush(Colors.Red);
                lbl_lisays_ilmoitus.Content = "Vaadittavia tietoja puuttuu!Tarkista tiedot";
                lbl_lisays_ilmoitus.Visibility = Visibility.Visible;
                await Task.Delay(3000);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
            }
            else
            {
                lbl_lisays_ilmoitus.Foreground = new SolidColorBrush(Colors.White);
                lbl_lisays_ilmoitus.Visibility = Visibility.Collapsed;
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
            dg_Lisattavat_Naytokset.Items.Add(new { Sijainti = txt_Paikkakunta.Text,
                Elokuvateatteri = txt_Elokuvateatteri.Text,
                Pvm = datep_Naytoksen_pvm.Text,
                Klo = txt_Aika.Text
            });
        }
    }
}
