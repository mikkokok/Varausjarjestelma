using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
            ValitutPaikat = new ObservableCollection<PaikkaVaraus>();
        }
        
        private class Valinta
        {
            public PaikkaVaraus Paikka { get; set; }
            public bool Valittu { get; set; }
            public bool Valittavissa { get; set; }

            public Valinta(PaikkaVaraus p) {
                Paikka = p;
                Valittu = p.Varattu;
                Valittavissa = !p.Varattu;
            }

            public Valinta(Elokuvasali sali, int paikkaNro, bool valittu) {
                Paikka = new PaikkaVaraus(sali, paikkaNro, valittu);
                Valittu = valittu;
                Valittavissa = true;
            }
        }

        public ObservableCollection<PaikkaVaraus> ValitutPaikat { get; set; }
        private Valinta[][] _Valinnat;
        private Elokuvasali Sali; 
        
        private Valinta _Valinta(int nro)
        {
            int rivi = Sali.Rivejä - Sali.RiviNrosta(nro);
            int paikka = Sali.PaikkojaRivissä - Sali.PaikkaRivissäNrosta(nro);
            return _Valinnat[rivi][paikka];
        }

        private Valinta _Valinta(PaikkaVaraus p) {
            int rivi = p.Sali.Rivejä - p.Rivi;
            int paikka = p.Sali.PaikkojaRivissä - p.PaikkaRivissä;
            return _Valinnat[rivi][paikka];
        }

        public void PoistaValinta(PaikkaVaraus p)
        {
            _Valinta(p).Valittu = false;
        }

        public void Valitse(PaikkaVaraus p)
        {
           _Valinta(p).Valittu = true;
            ValitutPaikat.Add(p);
        }

        public void Valittavissa(int nro, bool valittavissa)
        {
            _Valinta(nro).Valittavissa = valittavissa;
        }

        public void Valittavissa(PaikkaVaraus p, bool valittavissa)
        {
            _Valinta(p).Valittavissa = true;
        }

        public void MerkitseVaratut(List<PaikkaVaraus> varaukset)
        {
            foreach (PaikkaVaraus v in varaukset)
            {
                int rivi = Sali.Rivejä - v.Rivi;
                int paikka = Sali.PaikkojaRivissä - v.PaikkaRivissä;
                _Valinnat[rivi][paikka] = new Valinta(v);
            }
        }

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
                        int rivi = sali.Rivejä - r;
                        int paikka = sali.PaikkojaRivissä - s;
                        _Valinnat[r][s] = new Valinta(sali, sali.IstumapaikkaNro(rivi, paikka), false);
                    }
                }
            }

            Pohjapiirros.ItemsSource = _Valinnat;
        }

        public void AlustaVarauksilla(Elokuvasali sali, List<PaikkaVaraus> varaukset) {
            AlustaSali(sali);
            MerkitseVaratut(varaukset);
        }
        
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PaikanValinta));
   

        private void CheckBox_LisääLippu(object sender, RoutedEventArgs e) {
            ValitutPaikat.Add(((sender as CheckBox).DataContext as Valinta).Paikka);
            //System.Windows.MessageBox.Show(((sender as CheckBox).DataContext as Valinta).Paikka.PaikkaNro.ToString());
        }

        private void CheckBox_PoistaLippu(object sender, RoutedEventArgs e) {
            ValitutPaikat.Remove(((sender as CheckBox).DataContext as Valinta).Paikka);
        }
    }
}
