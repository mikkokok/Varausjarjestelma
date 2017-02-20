using System;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;

namespace Varausjarjestelma
{
    public class Tietokanta
    {
        private string _tietokannannimi = "Varausjarjestelma.sqlite";
        private string _sql;
        private SQLiteConnection _kantaYhteys;
        private SQLiteCommand _sqlkomento;
        private SQLiteDataReader _sqllukija;
        public Tietokanta()
        {
            LuoTietokanta();
            YhdistaTietokantaan();
        }

        private void YhdistaTietokantaan()
        {
            if (_kantaYhteys != null) return; // Yhteys on jo olemassa
            _kantaYhteys = new SQLiteConnection($"Data Source={_tietokannannimi};Version=3;");
            _kantaYhteys.Open();

        }
        private void LuoTietokanta()
        {
            if (File.Exists(_tietokannannimi))
                SQLiteConnection.CreateFile(_tietokannannimi); // Luo tietokannan samaan hakemistoon missä .exe ajetaan
            LuoTaulut(); // Tarkista taulut
        }

        private void LuoTaulut()
        {
            if (_kantaYhteys == null) // Alustetaan kantayhteys jos sitä ei ole vielä tehty
                YhdistaTietokantaan();

            _sql = "CREATE TABLE IF NOT EXISTS kayttajat" +           // Taulu kayttajat
                        "(id INTEGER PRIMARY KEY, " +
                        "etunimi VARCHAR(255), " +
                        "sukunimi VARCHAR(255), " +
                        "tunnus VARCHAR(255), " +
                        "salasana VARCHAR(255), " +
                        "rooli VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS elokuvat" +           // Taulu elokuvat
            "(id INTEGER PRIMARY KEY, " +
            "nimi VARCHAR(255), " +
            "vuosi VARCHAR(255), " +
            "ohjelmistossa VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS elokuvateatterit" +           // Taulu elokuvateatterit
            "(id INTEGER PRIMARY KEY, " +
            "nimi VARCHAR(255), " +
            "paikkakunta VARCHAR(255), " +
            "tunnus VARCHAR(255), " +
            "salasana VARCHAR(255), " +
            "rooli VARCHAR(255))";
            Ajasql(_sql);
            _sql = "CREATE TABLE IF NOT EXISTS lippu" +           // Taulu lippu
            "(id INTEGER PRIMARY KEY, " +
            "varaajannimi VARCHAR(255), " +
            "elokuva VARCHAR(255), " +
            "paikka VARCHAR(255), " +
            "elokuvateatteri VARCHAR(255))";
            Ajasql(_sql);
        }

        public List<string> Ajasql(string sql)
        {
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            var palautettava = new List<string>();
            while (_sqllukija.Read())
            {
                palautettava.Add(_sqllukija.GetString(1));
            }
            return palautettava;
        }

        public List<Elokuva> Elokuvat()
        {
            var res = new List<Elokuva>
            {
                new Elokuva("Elokuva 1", 163,
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ligula felis, tincidunt a maximus quis, vestibulum eu magna. Etiam ac dolor at lectus consectetur tempor id quis felis. In vitae vehicula eros, quis tristique urna. Ut tristique odio urna, vel dapibus felis vestibulum sit amet."),
                new Elokuva("Elokuva: II osa", 163,
                    "Etiam pretium, justo posuere pellentesque egestas, eros sem convallis turpis, \n\n sed fermentum justo ante ut turpis. Proin viverra sed lacus at ultrices. Sed fermentum ultricies gravida. Quisque at bibendum ante, quis porta ipsum.")
            };

            return res;
        }

        public List<Näytös> Näytökset(Elokuva elokuva)
        {
            var res = new List<Näytös>();

            var näytös = new Näytös();
            var aika = DateTime.Now.AddDays(2);

            var sali = new Elokuvasali
            {
                Rivejä = 6,
                PaikkojaRivissä = 8
            };

            näytös.Aika = aika;
            näytös.Elokuva = elokuva;
            näytös.Sali = sali;

            näytös.Sali.Teatteri = new Teatteri
            {
                Nimi = "KyberKino",
                Kaupunki = "City 17"
            };

            // koko varmaan parmepi erillään
            // esim. HaeVapaatPaikat(Näytös näytös) ?
            // (jolloin myös täytyy muuttaa Näytös-luokkaa)
            näytös.VaratutPaikat = new List<int> {1, 7, 14, 15, 16, 27, 40};

            res.Add(näytös);
            return res;
        }

        public List<Näytös> Näytökset(int elokuva)
        {
            return new List<Näytös>();
        }

        public void Dispose()
        {
            _kantaYhteys.Close();
        }
    }
}