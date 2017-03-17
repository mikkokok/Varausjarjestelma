using System;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Varausjarjestelma
{
    public class Tietokanta
    {
        private const string Tietokannannimi = "Varausjarjestelma.sqlite";
        private string _sql;
        private static SQLiteConnection _kantaYhteys;
        private SQLiteCommand _sqlkomento;
        private SQLiteDataReader _sqllukija;
        public Tietokanta()
        {
            LuoTietokanta();
            YhdistaTietokantaan();
        }

        private static void YhdistaTietokantaan()
        {
            if (_kantaYhteys != null) return; // Yhteys on jo olemassa
            _kantaYhteys = new SQLiteConnection($"Data Source={Tietokannannimi};Version=3;");
            _kantaYhteys.Open();

        }
        private void LuoTietokanta()
        {
            if (File.Exists(Tietokannannimi)) return;
            SQLiteConnection.CreateFile(Tietokannannimi); // Luo tietokannan samaan hakemistoon missä .exe ajetaan
            LuoTaulut(); // Luo taulut vain jos kantaa ei ole olemassa
        }

        private void LuoTaulut()
        {
            if (_kantaYhteys == null) // Alustetaan kantayhteys jos sitä ei ole vielä tehty
                YhdistaTietokantaan();

            _sql = "CREATE TABLE IF NOT EXISTS kayttajat" +   // Taulu kayttajat
            "(id INTEGER PRIMARY KEY, " +
            "etunimi VARCHAR(255), " +
            "sukunimi VARCHAR(255), " +
            "tunnus VARCHAR(255), " +
            "salasana VARCHAR(255), " +
            "rooli VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS elokuvat" +  // Taulu elokuvat
            "(id INTEGER PRIMARY KEY, " +
            "elokuvannimi VARCHAR(255), " +
            "vuosi VARCHAR(255), " +
            "kesto VARCHAR(255), " +
            "kuvaus VARCHAR(255), " +
            "ohjelmistossa VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS elokuvasalit" +   // Taulu elokuvasalit
            "(id INTEGER PRIMARY KEY, " +
            "nimi VARCHAR(255), " +
            "paikkojarivissa VARCHAR(255), " +
            "riveja VARCHAR(255), " +
            "teatterinnimi VARCHAR(255), " +
            "teatterinkaupunki VARCHAR(255))";
            _sql = "CREATE TABLE IF NOT EXISTS varaus" + // Taulu varaus
            "(id INTEGER PRIMARY KEY, " +
            "varaajannimi VARCHAR(255), " +
            "elokuvannimi VARCHAR(255), " +
            "paikka VARCHAR(255), " +
            "teatteri VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS naytokset" +   // Taulu naytokset
            "(id INTEGER PRIMARY KEY, " +
            "elokuvannimi VARCHAR(255), " +
            "aika VARCHAR(255), " +
            "sali VARCHAR(255), " +
            "teatteri VARCHAR(255))";
            Ajasql(_sql);
            // Luo pari kayttajaa tauluun
            Ajasql($"INSERT INTO kayttajat VALUES (null, 'Ylla', 'Pitaja', 'yllapitaja', 'nimda', 'Admin')");
            Ajasql($"INSERT INTO kayttajat VALUES (null, 'Antti', 'Virtanen', 'vantti', 'anttiv', 'User')");
            // Luo muutama elokuva
            Ajasql("INSERT INTO elokuvat VALUES(null, 'Paras elokuva', '2005', '120', 'Kissoja ja koiria', 'Kylla')");
            Ajasql("INSERT INTO elokuvat VALUES(null, 'Huono elokuva', '2002', '145', 'Kirahveja ja elefantteja', 'Ei')");
            // Luo muutama elokuvasali ja teatteri
            Ajasql("INSET INTO elokuvasalit VALUES(null, 'Sali1', '20', '10', 'Teatteri1', 'Kaupunki1')");
            Ajasql("INSET INTO elokuvasalit VALUES(null, 'Sali2', '10', '5', 'Teatteri1', 'Kaupunki1')");
        }

        public List<string> Ajasql(string sql)
        {
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            var palautettava = new List<string>();
            if (_sqllukija.FieldCount == 0) return palautettava; // Kysely on tyhjä
            var sb = new StringBuilder();
            while (_sqllukija.Read())
            {
                for (var i = 1; i < _sqllukija.FieldCount; i++)
                {
                    sb.Append(_sqllukija.GetString(i));
                    sb.Append(", ");
                }
                palautettava.Add(sb.ToString());
            }
            return palautettava;
        }
        #region käyttäjäkyselyt
        public List<Kayttaja> GetKayttajat()
        {
            var res = new List<Kayttaja>();
            const string sql = "SELECT * FROM kayttajat";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res.Add(new Kayttaja(_sqllukija.GetString(1), _sqllukija.GetString(2), _sqllukija.GetString(3), _sqllukija.GetString(4), _sqllukija.GetString(5)));
            }
            return res;
        }

        //Metodi joka etsii ja palauttaa käyttäjän tietokannasta
        //käyttäjänimen perusteella
        public Kayttaja getKayttaja(String kayttajatunnus)
        {
            var res = new Kayttaja("","","","","");
            string sql = $"SELECT * FROM kayttajat WHERE tunnus= {kayttajatunnus}";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res = new Kayttaja(_sqllukija.GetString(1), _sqllukija.GetString(2), _sqllukija.GetString(3), _sqllukija.GetString(4), _sqllukija.GetString(5));
            }
            return res;
        }

        public void SetKayttaja(Kayttaja kayttaja)
        {
            Ajasql($"INSERT INTO kayttajat VALUES (null, '{kayttaja.Etunimi}', '{kayttaja.Sukunimi}', '{kayttaja.Tunnus}', '{kayttaja.Salasana}', '{kayttaja.Rooli}')");
        }
        #endregion
        #region elokuvakyselyt
        public List<Elokuva> GetElokuvat()
        {
            var res = new List<Elokuva>();
            const string sql = "SELECT * FROM elokuvat";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res.Add(new Elokuva(_sqllukija.GetString(1), int.Parse(_sqllukija.GetString(2)), int.Parse(_sqllukija.GetString(3)), _sqllukija.GetString(4), _sqllukija.GetString(5)));
            }
            return res;
        }

        //Etsii ja palauttaa halutun elokuvan tietokannasta
        public Elokuva GetElokuva(string elokuvaNimi)
        {
            var res = new Elokuva("",1,1,"","");
            string sql = $"SELECT * FROM elokuvat WHERE nimi='{elokuvaNimi}'";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res = new Elokuva(_sqllukija.GetString(1), int.Parse(_sqllukija.GetString(2)), int.Parse(_sqllukija.GetString(3)), _sqllukija.GetString(4), _sqllukija.GetString(5));
            }
            return res;
        }

        public void SetElokuva(Elokuva elokuva)
        {
            Ajasql($"INSERT INTO elokuvat VALUES (null, '{elokuva.Nimi}', '{elokuva.Vuosi}','{elokuva.Kesto}', '{elokuva.Teksti}', '{elokuva.Ohjelmistossa}')");
        }

        public void DelElokuva(Elokuva elokuva)
        {
            Ajasql($"DELETE FROM elokuvat WHERE elokuvannimi='{elokuva.Nimi}'");
        }

        #endregion

        #region näytöskyselyt

        //Palauttaa elokuvaan kuuluvat näytökset
        public List<Näytös> getElokuvanNaytokset(Elokuva elokuva)
        {
            var res = new List<Näytös>();
            var salit = GetElokuvasalit();
            string sql = $"SELECT * FROM naytokset WHERE elokuvannimi='{elokuva.Nimi}'";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res.Add(new Näytös(elokuva, DateTime.Parse(_sqllukija.GetString(2)), haeElokuvasali(salit, _sqllukija.GetString(3)), haeElokuvasali(salit, _sqllukija.GetString(3)).Teatteri));
            }
            return res;
        }
        private Elokuvasali haeElokuvasali(List<Elokuvasali> salit, string salinnimi)
        {
            return salit.First(s => s.Nimi == salinnimi);
        }

        //Päivittää elokuvaan liittyvät näytökset
        public void muokkaaNaytokset(Elokuva elokuva, List<Näytös> naytokset)
        {
            // Tyhjennä aikaisemmat näytökset
            foreach (var naytos in naytokset)
            {
                DelNaytos(naytos);
            }

        }
        #endregion

        public void DelNaytos(Näytös naytos)
        {
            Ajasql($"DELETE FROM naytokset WHERE elokuvannimi='{naytos.Elokuva.Nimi}'");
        }

        #region Elokuvasalit
        public List<Elokuvasali> GetElokuvasalit()
        {
            var res = new List<Elokuvasali>();
            const string sql = "SELECT * FROM elokuvasalit";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res.Add(new Elokuvasali(_sqllukija.GetString(1), int.Parse(_sqllukija.GetString(2)), int.Parse(_sqllukija.GetString(3)), new Teatteri(_sqllukija.GetString(4), _sqllukija.GetString(5))));
            }
            return res;
        }

        public void SetElokuvasali(Elokuvasali elokuvasali)
        {
            Ajasql($"INSERT INTO elokuvasalit VALUES (null, '{elokuvasali.Nimi}', '{elokuvasali.PaikkojaRivissä}', '{elokuvasali.Rivejä}', '{elokuvasali.Teatteri.Nimi}, '{elokuvasali.Teatteri.Kaupunki}')");
        }


        #endregion
        #region Varaukset
        public List<Elokuvasali> GetVaraukset()
        {
            var res = new List<Elokuvasali>();
            const string sql = "(SELECT * FROM elokuvasalit)";
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            if (_sqllukija.FieldCount == 0) return res; // Taulu on tyhja
            while (_sqllukija.Read())
            {
                res.Add(new Elokuvasali(_sqllukija.GetString(1), int.Parse(_sqllukija.GetString(2)), int.Parse(_sqllukija.GetString(3)), new Teatteri(_sqllukija.GetString(4), _sqllukija.GetString(5))));
            }
            return res;
        }

        public void SetVaraukset(Elokuvasali elokuvasali)
        {
            Ajasql($"INSERT INTO elokuvasalit VALUES (null, '{elokuvasali.Nimi}', '{elokuvasali.PaikkojaRivissä}', '{elokuvasali.Rivejä}', '{elokuvasali.Teatteri.Nimi}, '{elokuvasali.Teatteri.Kaupunki}')");
        }


        #endregion
        public List<Näytös> Näytökset(Elokuva elokuva)
        {
            var res = new List<Näytös>();

            var näytös = new Näytös();
            var aika = DateTime.Now.AddDays(2);

            var sali = new Elokuvasali("Suurin ja kaunein -sali", 6, 8, new Teatteri("City 17", "KyberKino"));

            näytös.Aika = aika;
            näytös.Elokuva = elokuva;
            näytös.Sali = sali;

            //koko varmaan parmepi erillään
            // esim.HaeVapaatPaikat(Näytös näytös) ?
            // (jolloin myös täytyy muuttaa Näytös - luokkaa)

            res.Add(näytös);
            return res;
        }

        public List<Paikka> VaratutPaikat(Näytös n)
        {
            // Tässä placeholderissa salissa siis ainakin 48 paikkaa
            return new List<Paikka>() {
                new Paikka(n.Sali, 1),
                new Paikka(n.Sali, 2),
                new Paikka(n.Sali, 3),
                new Paikka(n.Sali, 9),
                new Paikka(n.Sali, 10),
                new Paikka(n.Sali, 28),
                new Paikka(n.Sali, 29),
                new Paikka(n.Sali, 48)
            };
        }

        public List<Näytös> Näytökset(int elokuva)
        {
            return new List<Näytös>();
        }
        public void VaraaPaikka(Kayttaja kayttaja, Näytös naytos, Paikka paikka)
        {

        }

        public void Dispose()
        {
            _kantaYhteys.Close();
        }
    }
}