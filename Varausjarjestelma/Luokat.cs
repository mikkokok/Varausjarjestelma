using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Varausjarjestelma
{
    public class Elokuva
    {
        public int Id { get; set; } 
        public string Nimi { get; set; }
        public int Kesto { get; set; } // minuutteja
        public string Teksti { get; set; }

        public Elokuva(string nimi, int kesto, string teksti)
        {
            this.Nimi = nimi;
            this.Kesto = kesto;
            this.Teksti = teksti;
        }
    }

    public class Teatteri
    {
        public string Nimi { get; set; }
        public string Kaupunki { get; set; }

        public Teatteri(string nimi, string kaupunki)
        {
            this.Nimi = nimi;
            this.Kaupunki = kaupunki;
        }
    }
    
    public class Elokuvasali
    {
        public Teatteri Teatteri { set; get; }
        public string Nimi { get; set; }

        // numerointi alkaa 1:stä
        // eli ensimmäinen rivi ja ensimmäinen paikka jne
        //
        // 1. rivi on edessä, ja 1. paikka vasemmalla, edestä katsottuna
        //
        public int Rivejä { set; get; }
        public int PaikkojaRivissä { set; get; } // "sarake"

        // sovitaan vaikka, että paikkojen numerointi alkaa 1:stä
        //
        public int PaikkojaYhteensä => this.Rivejä * this.PaikkojaRivissä;

        public int IstumapaikkaNro(int rivi, int rivipaikka)
        {
            return ((rivi - 1) * PaikkojaRivissä) + rivipaikka;
        }

        public int RiviNrosta(int nro)
        {
            return (nro + PaikkojaRivissä - 1) / PaikkojaRivissä;
        }

        public int PaikkaRivissäNrosta(int nro)
        {
            return nro - ((RiviNrosta(nro) - 1) * PaikkojaRivissä);
        }

        public Elokuvasali(string nimi, int paikkojarivissa, int riveja, Teatteri teatteri)
        {
            this.Nimi = nimi;
            this.PaikkojaRivissä = paikkojarivissa;
            this.Rivejä = riveja;
            this.Teatteri = teatteri;
        }


    }

    public class Paikka
    {
        public Elokuvasali Sali { get; set; }
        public int PaikkaNro { get; set; }

        public int Rivi => Sali.RiviNrosta(PaikkaNro);

        public int PaikkaRivissä => Sali.PaikkaRivissäNrosta(PaikkaNro);

        public Paikka(Elokuvasali sali, int paikkaNro)
        {
            Sali = sali;
            PaikkaNro = paikkaNro;
        }
    }
    
    public class Näytös
    {
        public Elokuva Elokuva { get; set; }
        public Elokuvasali Sali { get; set; }
        public Teatteri Teatteri => Sali.Teatteri;

        public int VapaitaPaikkoja { get; set; }

        public DateTime Aika { set; get; }

        // siiretään tietokantaluokkaan?
        //
        //public list<int> varatutpaikat { get; set; }
        //public list<int> vapaatpaikat {
        //    get {
        //        list<int> paikat = new list<int>();

        //        for (int i = 1; i <= sali.paikkojayhteensä; i += 1)
        //        {
        //            if (!varatutpaikat.contains(i)) {
        //                paikat.add(i);
        //            }
        //        }

        //        return paikat;
        //    }
        //}
    }

    public class Kayttaja
    {
        public string Etunimi;
        public string Sukunimi;
        public string Tunnus;
        public string Salasana;
        public string Rooli;

        public Kayttaja(string etunimi, string sukunimi, string tunnus, string salasana, string rooli)
        {
            this.Etunimi = etunimi;
            this.Sukunimi = sukunimi;
            this.Tunnus = tunnus;
            this.Salasana = salasana;
            this.Rooli = rooli;
        }
    }
}
