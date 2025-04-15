using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    class Zadanie
    {
        public int Numer { get; set; }
        public string Nazwa { get; set; }
        public DateTime TerminRealizacji { get; set; }
        public int Priorytet { get; set; }

        public Zadanie(int numer, string nazwa, DateTime terminRealizacji, int priorytet)
        {
            Numer = numer;
            Nazwa = nazwa;
            TerminRealizacji = terminRealizacji;
            Priorytet = priorytet;
        }

    }
}
