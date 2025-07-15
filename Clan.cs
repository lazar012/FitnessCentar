using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnesCentar38cet
{
    // Klasa koja predstavlja člana fitnes centra
    internal class Clan
    {
        // Svojstva (Properties) koja opisuju člana
        public string ImePrezime { get; set; }      // Ime i prezime člana
        public string TipClanarine { get; set; }    // Tip članarine (mesečna, kvartalna, itd.)
        public string Telefon { get; set; }         // Kontakt telefon člana
        public DateTime DatumUpisa { get; set; }    // Datum kada se član učlanio
        public DateTime DatumIsteka { get; set; }   // Datum kada članarina ističe

        // Konstruktor klase - poziva se prilikom kreiranja novog objekta
        public Clan(string imePrezime, string tipClanarine, string telefon, DateTime datum, DateTime datumIsteka)
        {
            // Inicijalizacija svojstava sa prosleđenim vrednostima
            ImePrezime = imePrezime;
            TipClanarine = tipClanarine;
            Telefon = telefon;
            DatumUpisa = datum;
            DatumIsteka = datumIsteka;
        }
    }
}