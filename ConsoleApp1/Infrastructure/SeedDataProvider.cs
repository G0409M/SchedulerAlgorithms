using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Core;

namespace ConsoleApp1.Infrastructure
{
    class SeedDataProvider
    {
        public static List<Zadanie> GenerujZadania()
        {
            return new List<Zadanie>
        {
            new Zadanie(1, "Zadanie 1", new DateTime(2024, 4, 2), 3),
            new Zadanie(2, "Zadanie 2", new DateTime(2024, 4, 15), 2),
            new Zadanie(3, "Zadanie 3", new DateTime(2024, 5, 3), 4),
            new Zadanie(4, "Zadanie 4", new DateTime(2024, 5, 22), 1)
        };
        }

        public static List<Podzadanie> GenerujPodzadania()
        {
            return new List<Podzadanie>
        {
            new Podzadanie(1, "Przygotuj dane", 1, 2, 2),
            new Podzadanie(1, "Wykonaj wstępny grafik", 2, 3, 3),
            new Podzadanie(1, "Przeanalizuj wyniki", 3, 1, 1),
            new Podzadanie(1, "Wykonaj prezentację", 4, 2, 2),

            new Podzadanie(2, "Przygotuj dane", 1, 2, 2),
            new Podzadanie(2, "Wykonaj analizę", 2, 1, 1),
            new Podzadanie(2, "Wykonaj prezentację", 3, 5, 3),

            new Podzadanie(3, "Przeanalizuj wyniki", 1, 1, 2),
            new Podzadanie(3, "Wykonaj wstępny grafik", 2, 2, 1),
            new Podzadanie(3, "Przygotuj dane", 3, 3, 3),
            new Podzadanie(3, "Wykonaj prezentację", 4, 1, 2),

            new Podzadanie(4, "Wykonaj analizę", 1, 6, 1),
            new Podzadanie(4, "Wykonaj prezentację", 2, 2, 2)
        };
        }

        public static List<DostepnyCzas> GenerujDostepnyCzas()
        {
            return new List<DostepnyCzas>
        {
            new DostepnyCzas(new DateTime(2024, 3, 10), 2),
            new DostepnyCzas(new DateTime(2024, 3, 11), 3),
            new DostepnyCzas(new DateTime(2024, 3, 12), 4),
            new DostepnyCzas(new DateTime(2024, 3, 15), 2),
            new DostepnyCzas(new DateTime(2024, 3, 17), 1),
            new DostepnyCzas(new DateTime(2024, 3, 21), 2),
            new DostepnyCzas(new DateTime(2024, 3, 29), 1),
            new DostepnyCzas(new DateTime(2024, 4, 1), 2),
            new DostepnyCzas(new DateTime(2024, 4, 3), 4),
            new DostepnyCzas(new DateTime(2024, 4, 5), 2),
            new DostepnyCzas(new DateTime(2024, 4, 7), 4),
            new DostepnyCzas(new DateTime(2024, 4, 19), 5),
            new DostepnyCzas(new DateTime(2024, 4, 25), 2),
            new DostepnyCzas(new DateTime(2024, 5, 1), 3),
            new DostepnyCzas(new DateTime(2024, 5, 5), 3),
            new DostepnyCzas(new DateTime(2024, 5, 15), 5),
            new DostepnyCzas(new DateTime(2024, 5, 19), 3),
            new DostepnyCzas(new DateTime(2024, 5, 20), 3)
        };
        }
    }

}
