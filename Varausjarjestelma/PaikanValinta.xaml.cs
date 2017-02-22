using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Varausjarjestelma
{
    /// <summary>
    /// Interaction logic for PaikanValinta.xaml
    /// </summary>
    /// 
    public partial class PaikanValinta : UserControl
    {
        public PaikanValinta()
        {
            InitializeComponent();
            ValitutPaikat = new ObservableCollection<Paikka>();
        }
        
        private class Valinta
        {
            public Paikka Paikka { get; set; }
            public bool Valittu { get; set; }      // CheckBox.IsChecked
            public bool Valittavissa { get; set; } // CheckBox.IsEnabled

            public Valinta(Paikka p) {
                Paikka = p;
            }

            public Valinta(Elokuvasali sali, int paikkaNro, bool valittu) {
                Paikka = new Paikka(sali, paikkaNro);
                Valittu = valittu;
                Valittavissa = true;
            }
        }

        // ajantasainen lista paikoista, jotka ovat valittuina
        // joko käyttäjän toimesta tai käyttämällä Valitse() -metodia
        //
        public ObservableCollection<Paikka> ValitutPaikat { get; set; }

        private Valinta[][] _Valinnat;
        private Elokuvasali Sali; 
        
        private Valinta _Valinta(int nro)
        {
            int rivi = Sali.Rivejä - Sali.RiviNrosta(nro);
            int paikka = Sali.PaikkaRivissäNrosta(nro) - 1;
            // peilikuva: Sali.PaikkojaRivissä - Sali.PaikkaRivissäNrosta(nro);
            return _Valinnat[rivi][paikka];
        }

        private Valinta _Valinta(Paikka p) {
            int rivi = p.Sali.Rivejä - p.Rivi;
            int paikka = p.PaikkaRivissä - 1;
            // peilikuva: Sali.PaikkojaRivissä - p.PaikkaRivissä;
            return _Valinnat[rivi][paikka];
        }

        // poista valinta paikalta p
        //
        public void PoistaValinta(Paikka p)
        {
            _Valinta(p).Valittu = false;
        }

        // lisää valituksi paikka p
        //
        public void Valitse(Paikka p)
        {
            Valinta v = _Valinta(p);
            v.Valittu = true;
            if (!ValitutPaikat.Contains(v.Paikka)) // huom, ref-yhtäsuuruus
            {
                ValitutPaikat.Add(v.Paikka);
            }
        }

        // lisää valituksi paikkanro
        //
        public void Valitse(int paikkaNro)
        {
            Valinta v = _Valinta(paikkaNro);
            v.Valittu = true;
            if (!ValitutPaikat.Contains(v.Paikka)) // huom, ref-yhtäsuuruus
            {
                ValitutPaikat.Add(v.Paikka);
            }
        }

        // paikan valinta: enabled/disabled
        //
        public void Valittavissa(int nro, bool valittavissa)
        {
            _Valinta(nro).Valittavissa = valittavissa;
        }

        // paikan valinta: enabled/disabled
        //
        public void Valittavissa(Paikka p, bool valittavissa)
        {
            _Valinta(p).Valittavissa = true;
        }

        // merkitsee listassa olevat paikat varatuiksi ja
        // Checkbox.IsEnabled = false kyseisille paikoille
        //
        public void MerkitseVaratut(List<Paikka> varaukset)
        {
            foreach (Paikka v in varaukset)
            {
                int rivi = Sali.Rivejä - v.Rivi;
                int paikka = v.PaikkaRivissä - 1; 
                // peilikuva: Sali.PaikkojaRivissä - v.PaikkaRivissä;
                _Valinnat[rivi][paikka] = new Valinta(v);
                _Valinnat[rivi][paikka].Valittu = true; // parempi ilman?
            }
        }

        // Alusta pohjapiirros salin mukaan
        //
        public void AlustaSali(Elokuvasali sali)
        {
            ValitutPaikat.Clear();
            _Valinnat = new Valinta[sali.Rivejä][];
            Sali = sali;

            for (int i = 0; i < sali.Rivejä; i++)
            {
                _Valinnat[i] = new Valinta[sali.PaikkojaRivissä];
            }

            for (int r = 0; r < sali.Rivejä; r++)
            {
                for (int s = 0; s < sali.PaikkojaRivissä; s++)
                {
                    if (_Valinnat[r][s] == null)
                    {
                        // numerointi: ensimmäinen rivi lähinnä valkokangasta
                        //             ensimmäinen paikka valkokankaalta katsottuna vasemmalla
                        //
                        //             kommentoitu pois peilikuva, jossa ensimmäinen paikka on
                        //             valkokankaalta katsottuna oikealla
                        //
                        int rivi = sali.Rivejä - r;
                        int paikka = s + 1;
                        // peilikuva: sali.PaikkojaRivissä - s 
                        _Valinnat[r][s] = new Valinta(sali, sali.IstumapaikkaNro(rivi, paikka), false);
                    }
                }
            }

            Pohjapiirros.ItemsSource = _Valinnat;
        }

        // ks. AlustaSali() ja MerkitseVaraukset()
        //
        public void AlustaVarauksilla(Elokuvasali sali, List<Paikka> varaukset) {
            AlustaSali(sali);
            MerkitseVaratut(varaukset);
        }
        
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PaikanValinta));
        
        private void CheckBox_LisääLippu(object sender, RoutedEventArgs e) {
            ValitutPaikat.Add(((sender as CheckBox).DataContext as Valinta).Paikka);
        }

        private void CheckBox_PoistaLippu(object sender, RoutedEventArgs e) {
            ValitutPaikat.Remove(((sender as CheckBox).DataContext as Valinta).Paikka);
        }
    }
}
