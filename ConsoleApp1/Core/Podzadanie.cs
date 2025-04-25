using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    class Podzadanie
    {
        public int NumerZadania { get; set; }
        public string NazwaPodzadania { get; set; }
        public int NumerPodzadania { get; set; }
        public int SzacowanyCzas { get; set; }
        public int Priorytet { get; set; } 

        public Podzadanie(int numerZadania, string nazwaPodzadania, int numerPodzadania, int szacowanyCzas, int priorytet)
        {
            NumerZadania = numerZadania;
            NazwaPodzadania = nazwaPodzadania;
            NumerPodzadania = numerPodzadania;
            SzacowanyCzas = szacowanyCzas;
            Priorytet = priorytet;
        }
    }
}
