using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp1.Core;

namespace ConsoleApp1.Logic
{
    class ScheduleEvaluator
    {
        private readonly Dictionary<int, Zadanie> zadania;
        private readonly List<Podzadanie> podzadania;
        private readonly List<DostepnyCzas> dniRobocze;

        public ScheduleEvaluator(List<Zadanie> zadania, List<Podzadanie> podzadania, List<DostepnyCzas> dniRobocze)
        {
            this.zadania = zadania.ToDictionary(z => z.Numer);
            this.podzadania = podzadania;
            this.dniRobocze = dniRobocze;
        }

        public double ObliczKare(List<HarmonogramEntry> harmonogram)
        {
            double kara = 0;

            var grupyPoPodzadaniu = harmonogram
                .Where(h => h.Podzadanie != null)
                .GroupBy(h => h.Podzadanie)
                .ToDictionary(g => g.Key, g => g.OrderBy(h => h.Data).ToList());

            var wszystkiePodzadania = podzadania;

            // 1. Kara za przekroczenie deadline'u lub brak przydzielenia zadania
            foreach (var zad in zadania.Values)
            {
                var wpisyZadania = harmonogram
                    .Where(h => h.Podzadanie != null && h.Podzadanie.NumerZadania == zad.Numer)
                    .ToList();

                if (wpisyZadania.Any())
                {
                    var maxData = wpisyZadania.Max(h => h.Data);
                    if (maxData > zad.TerminRealizacji)
                    {
                        kara += zad.Priorytet * 500; // kara za przekroczenie terminu
                    }
                }
                else
                {
                    kara += zad.Priorytet * 1000; // zadanie w ogóle nieprzypisane
                }
            }

            // 2. Kara za nieprzypisanie lub nadmiar czasu podzadania
            foreach (var pod in wszystkiePodzadania)
            {
                var wpisy = grupyPoPodzadaniu.ContainsKey(pod)
                    ? grupyPoPodzadaniu[pod]
                    : new List<HarmonogramEntry>();

                var suma = wpisy.Sum(w => w.IloscGodzin);
                var roznica = Math.Abs(pod.SzacowanyCzas - suma);

                if (roznica > 0)
                {
                    kara += roznica * pod.Priorytet * zadania[pod.NumerZadania].Priorytet * 200;
                }
            }

            // 3. Kolejność podzadań w każdym zadaniu
            var zadaniaGrupowane = wszystkiePodzadania.GroupBy(p => p.NumerZadania);
            foreach (var grupa in zadaniaGrupowane)
            {
                var podz = grupa.OrderBy(p => p.NumerPodzadania).ToList();
                DateTime? lastEnd = null;

                foreach (var pod in podz)
                {
                    if (!grupyPoPodzadaniu.ContainsKey(pod))
                        continue;

                    var czas = grupyPoPodzadaniu[pod];
                    var najwczesniejszy = czas.First().Data;

                    if (lastEnd.HasValue && najwczesniejszy < lastEnd.Value)
                    {
                        kara += pod.Priorytet * 300; // naruszenie kolejności
                    }

                    lastEnd = czas.Last().Data;
                }
            }

            // 4A. Kara za przekroczenie dostępności dnia
            var dniZPrzypisaniami = harmonogram
                .GroupBy(h => h.Data)
                .Select(g => new
                {
                    Data = g.Key,
                    Wykorzystane = g.Sum(x => x.IloscGodzin),
                    Dostepne = dniRobocze.FirstOrDefault(d => d.Data == g.Key)?.IloscDostepnegoCzasu ?? 0
                })
                .Where(x => x.Dostepne > 0)
                .ToList();

            foreach (var dzien in dniZPrzypisaniami)
            {
                if (dzien.Wykorzystane > dzien.Dostepne)
                {
                    double nadmiar = dzien.Wykorzystane - dzien.Dostepne;
                    kara += nadmiar *500; // kara za przekroczenie dostępnego czasu
                }
            }
            // 4B. Kara za nierównomierne obciążenie dni
            if (dniZPrzypisaniami.Any())
            {
                double sredniProcent = dniZPrzypisaniami.Average(x => (double)x.Wykorzystane / x.Dostepne);
                double sumaOdchylek = dniZPrzypisaniami.Sum(x => Math.Abs((double)x.Wykorzystane / x.Dostepne - sredniProcent));
                kara += sumaOdchylek * 20; // subtelna kara za brak równomierności
            }


            return kara;
        }
    }
}
