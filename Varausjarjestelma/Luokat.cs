using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Varausjarjestelma
{
    public class Elokuva
    {
        public int id { get; set; } 
        public string Nimi { get; set; }
        public int Kesto { get; set; } // minuutteja
        public string Teksti { get; set; }

        public Elokuva(string nimi, int kesto, string teksti) {
            this.Nimi = nimi;
            this.Kesto = kesto;
            this.Teksti = teksti;
        }
    }

    public class Teatteri
    {
        public string Nimi { get; set; }
        public string Kaupunki { get; set; }
    }
    
    public class Elokuvasali
    {
        public Teatteri Teatteri { set; get; }

        // numerointi alkaa 1:stä
        // eli ensimmäinen rivi ja ensimmäinen paikka jne
        //
        // 1. rivi on edessä, ja 1. paikka vasemmalla, edestä katsottuna
        //
        public int Rivejä { set; get; }
        public int PaikkojaRivissä { set; get; } // "sarake"

        // sovitaan vaikka, että paikkojen numerointi alkaa 1:stä
        //
        public int PaikkojaYhteensä { get
            {
                return this.Rivejä * this.PaikkojaRivissä;
            }
        }
        
        // return type? Lippu/Istumapaikka -luokka tarpeellinen/hyödyllinen?
        //
        public Tuple<int, int> Istumapaikka(int nro) {
            int rivi = (nro + PaikkojaRivissä - 1) / PaikkojaRivissä;
            int rivipaikka = nro - ((rivi - 1) * PaikkojaRivissä);

            return new Tuple<int, int>(rivi, rivipaikka);
        }

        public int IstumapaikkaNro(int rivi, int rivipaikka)
        {
            return ((rivi - 1) * PaikkojaRivissä) + rivipaikka;
        }
    }
    
    public class Näytös
    {
        public Elokuva Elokuva { get; set; }
        public Elokuvasali Sali { get; set; }
        public Teatteri Teatteri { get {
                return Sali.Teatteri;
            }
        }

        public DateTime Aika { set; get; }
        public List<int> VaratutPaikat { get; set; }
        public List<int> VapaatPaikat {
            get {
                List<int> paikat = new List<int>();

                for (int i = 1; i <= Sali.PaikkojaYhteensä; i += 1)
                {
                    if (!VaratutPaikat.Contains(i)) {
                        paikat.Add(i);
                    }
                }

                return paikat;
            }
        }

        public int VapaitaPaikkojaYht {
            get { return VapaatPaikat.Count; }
        }
    }
}
