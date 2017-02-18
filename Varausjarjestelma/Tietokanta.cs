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
        }

        private void YhdistaTietokantaan()
        {
            LuoTietokanta(); 
            _kantaYhteys = new SQLiteConnection($"Data Source={_tietokannannimi};Version=3;");
            _kantaYhteys.Open();

        }
        private void LuoTietokanta()
        {
            if (File.Exists(_tietokannannimi)) return;
            SQLiteConnection.CreateFile(_tietokannannimi); // Luo tietokannan samaan hakemistoon missä .exe ajetaan
        }

        private void LuoTaulut()
        {
            _sql = "CREATE TABLE person" +           // Table person
                        "(id INTEGER PRIMARY KEY, " +
                        "firstname VARCHAR(50), " +
                        "lastname VARCHAR(255))";
        }

        public string Ajasql(string sql)
        {
            _sqlkomento = new SQLiteCommand(sql, _kantaYhteys);
            _sqllukija = _sqlkomento.ExecuteReader();
            return null;
        }

        public List<Elokuva> Elokuvat() {
            List<Elokuva> res = new List<Elokuva>();

            res.Add(new Elokuva("Elokuva 1", 163, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ligula felis, tincidunt a maximus quis, vestibulum eu magna. Etiam ac dolor at lectus consectetur tempor id quis felis. In vitae vehicula eros, quis tristique urna. Ut tristique odio urna, vel dapibus felis vestibulum sit amet."));
            res.Add(new Elokuva("Elokuva: II osa", 163, "Etiam pretium, justo posuere pellentesque egestas, eros sem convallis turpis, \n\n sed fermentum justo ante ut turpis. Proin viverra sed lacus at ultrices. Sed fermentum ultricies gravida. Quisque at bibendum ante, quis porta ipsum."));
            return res;
        }

        public List<Näytös> Näytökset(Elokuva elokuva)
        {
            List<Näytös> res = new List<Näytös>();

            Näytös näytös = new Näytös();
            DateTime aika = DateTime.Now.AddDays(2);

            Elokuvasali sali = new Elokuvasali();
            sali.Rivejä = 6;
            sali.PaikkojaRivissä = 8;

            näytös.Aika = aika;
            näytös.Elokuva = elokuva;
            näytös.Sali = sali;
            
            näytös.Sali.Teatteri = new Teatteri();
            näytös.Sali.Teatteri.Nimi = "KyberKino";
            näytös.Sali.Teatteri.Kaupunki = "City 17";
            
            // koko varmaan parmepi erillään
            // esim. HaeVapaatPaikat(Näytös näytös) ?
            // (jolloin myös täytyy muuttaa Näytös-luokkaa)
            näytös.VaratutPaikat = new List<int>();
            näytös.VaratutPaikat.Add(1);
            näytös.VaratutPaikat.Add(7);
            näytös.VaratutPaikat.Add(14);
            näytös.VaratutPaikat.Add(15);
            näytös.VaratutPaikat.Add(16);
            näytös.VaratutPaikat.Add(27);
            näytös.VaratutPaikat.Add(40);

            res.Add(näytös);
            return res;
        }

        public List<Näytös> Näytökset(int elokuva)
        {
            return new List<Näytös>();
        }
    }
}