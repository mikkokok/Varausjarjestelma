﻿using System;
using System.Data.SQLite;
using System.IO;

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

        }
    }
}