using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Varausjarjestelma
{
    public class Elokuva
    {
        public string Nimi { get; set; }
        public int Kesto { get; set; } // minuutteja
        public string Teksti { get; set; }

        public Elokuva(string nimi, int kesto, string teksti) {
            this.Nimi = nimi;
            this.Kesto = kesto;
            this.Teksti = teksti;
        }
    }

    
}
