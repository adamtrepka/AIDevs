using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Exercises.People
{
    public class PersonInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("imie")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("nazwisko")]
        public string Surname { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("wiek")]
        public int Age { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("o_mnie")]
        public string Info { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ulubiona_postac_z_kapitana_bomby")]
        public string FavoriteCharacter { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ulubiony_serial")]
        public string FavoriteSeries { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ulubiony_film")]
        public string FavoriteMovie { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ulubiony_kolor")]
        public string FavoriteColor { get; set; }

        public string Key => $"{Name} {Surname}";
        public float[] Embedding { get; set; }
    }
}

/*
     {
        "imie": "Katarzyna",
        "nazwisko": "Rumcajs",
        "wiek": 32,
        "o_mnie": "lubie zjadać lody. Mieszkam w Łodzi. Interesuję mnie polikyka oraz samochody",
        "ulubiona_postac_z_kapitana_bomby": "nie oglądam",
        "ulubiony_serial": "Big Bang Theory",
        "ulubiony_film": "The Lord of the Rings",
        "ulubiony_kolor": "magenta"
    }
 */