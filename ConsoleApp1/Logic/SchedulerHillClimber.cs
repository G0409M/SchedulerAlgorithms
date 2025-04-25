using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.Core;

namespace ConsoleApp1.Logic
{
    class SchedulerHillClimber
    {
        private List<Zadanie> zadania;
        private List<Podzadanie> podzadania;
        private List<DostepnyCzas> dniRobocze;
        private Dictionary<int, Zadanie> mapaZadan;
        private Random rnd = new();
        private ScheduleEvaluator evaluator;

        public SchedulerHillClimber(List<Zadanie> zadania, List<Podzadanie> podzadania, List<DostepnyCzas> dniRobocze)
        {
            this.zadania = zadania;
            this.podzadania = podzadania;
            this.dniRobocze = dniRobocze;
            mapaZadan = zadania.ToDictionary(z => z.Numer);
            evaluator = new ScheduleEvaluator(zadania, podzadania, dniRobocze);

        }
        public List<HarmonogramEntry> Start(int maxRestarty = 10000, int maxBrakPoprawy = 20)
        {
            List<HarmonogramEntry> najlepszy = null;
            double najlepszaKara = double.MaxValue;

            for (int restart = 0; restart < maxRestarty; restart++)
            {

                var aktualny = WygenerujLosowyHarmonogram();
                double kara = evaluator.ObliczKare(aktualny);
                int brakPoprawy = 0;
                int liczbaPopraw = 0;

                while (brakPoprawy < maxBrakPoprawy)
                {
                    var sasiad = GenerujSasiada(aktualny);
                    double karaSasiada = evaluator.ObliczKare(sasiad);

                    if (karaSasiada < kara)
                    {
                        aktualny = sasiad;
                        kara = karaSasiada;
                        brakPoprawy = 0;
                        liczbaPopraw++;
                    }
                    else
                    {
                        brakPoprawy++;
                    }
                }

                Console.WriteLine($"[Restart {restart + 1}/{maxRestarty}] liczba popraw: {liczbaPopraw} Najlepsza kara: {kara:F2}");

                if (kara < najlepszaKara)
                {
                    // Głęboka kopia najlepszego harmonogramu
                    najlepszy = aktualny
                        .Select(h => new HarmonogramEntry
                        {
                            Data = h.Data,
                            Podzadanie = h.Podzadanie,
                            IloscGodzin = h.IloscGodzin
                        })
                        .ToList();

                    najlepszaKara = kara;
                }
            }

            Console.WriteLine($"\n Najlepsza znaleziona kara: {najlepszaKara:F2}");
            return najlepszy;
        }



        private List<HarmonogramEntry> WygenerujLosowyHarmonogram()
        {
            var harmonogram = new List<HarmonogramEntry>();

            var dostep = dniRobocze
                .Select(d => new DostepnyCzas(d.Data, d.IloscDostepnegoCzasu))
                .OrderBy(d => d.Data)
                .ToList();

            var dniDaty = dostep.Select(d => d.Data).ToList();
            var shuffled = podzadania.OrderBy(_ => rnd.Next()).ToList();

            foreach (var pod in shuffled)
            {
                var deadline = mapaZadan[pod.NumerZadania].TerminRealizacji;
                int pozostalo = pod.SzacowanyCzas;

                while (pozostalo > 0)
                {
                    var dostepneDni = dostep
                        .Where(d => d.Data <= deadline && d.IloscDostepnegoCzasu > 0)
                        .OrderByDescending(d => d.IloscDostepnegoCzasu)
                        .ToList();

                    if (!dostepneDni.Any())
                        break; 

                    var dzien = dostepneDni.First();
                    int ileMoznaWziac = Math.Min(pozostalo, dzien.IloscDostepnegoCzasu);
                    int przydziel = rnd.Next(1, ileMoznaWziac + 1);

                    harmonogram.Add(new HarmonogramEntry
                    {
                        Data = dzien.Data,
                        Podzadanie = pod,
                        IloscGodzin = przydziel
                    });

                    dzien.IloscDostepnegoCzasu -= przydziel;
                    pozostalo -= przydziel;
                }

                if (pozostalo > 0)
                {
                    Console.WriteLine($" Awaryjne przypisanie: {pod.NazwaPodzadania} ({pod.NumerZadania}) – pozostało {pozostalo}h");

                    var fallbackDays = dostep
                        .OrderBy(_ => rnd.Next()) 
                        .ToList();

                    foreach (var dzien in fallbackDays)
                    {
                        if (pozostalo <= 0) break;

                        int przydziel = Math.Min(pozostalo, 4); 
                        harmonogram.Add(new HarmonogramEntry
                        {
                            Data = dzien.Data,
                            Podzadanie = pod,
                            IloscGodzin = przydziel
                        });

                        pozostalo -= przydziel;
                    }

                    if (pozostalo > 0)
                    {
                        Console.WriteLine($"[ERROR] Nie udało się przypisać {pozostalo}h podzadania: {pod.NazwaPodzadania} ({pod.NumerZadania})");
                    }
                }
            }

            var datyZajete = harmonogram.Select(h => h.Data).ToHashSet();
            foreach (var data in dniDaty)
            {
                if (!datyZajete.Contains(data))
                {
                    harmonogram.Add(new HarmonogramEntry
                    {
                        Data = data,
                        Podzadanie = null,
                        IloscGodzin = 0
                    });
                }
            }

            return harmonogram
                .OrderBy(h => h.Data)
                .ThenBy(h => h.Podzadanie?.NumerZadania ?? int.MaxValue)
                .ThenBy(h => h.Podzadanie?.NumerPodzadania ?? int.MaxValue)
                .ToList();
        }

        private List<HarmonogramEntry> GenerujSasiada(List<HarmonogramEntry> oryginal)
        {
            // Głęboka kopia harmonogramu
            var kopia = oryginal
                .Select(h => new HarmonogramEntry
                {
                    Data = h.Data,
                    Podzadanie = h.Podzadanie,
                    IloscGodzin = h.IloscGodzin
                })
                .ToList();

            var losowePodzadanie = kopia
                .Where(h => h.Podzadanie != null)
                .Select(h => h.Podzadanie)
                .Distinct()
                .OrderBy(_ => rnd.Next())
                .FirstOrDefault();

            if (losowePodzadanie == null)
                return kopia;

            // Usuń wszystkie wpisy tego podzadania
            kopia.RemoveAll(h => h.Podzadanie == losowePodzadanie);

            var deadline = mapaZadan[losowePodzadanie.NumerZadania].TerminRealizacji;
            int pozostalo = losowePodzadanie.SzacowanyCzas;

            // Skopiuj dostępność z dni roboczych
            var dostep = dniRobocze
                .Select(d => new DostepnyCzas(d.Data, d.IloscDostepnegoCzasu))
                .OrderBy(d => d.Data)
                .ToList();

            // Zaktualizuj zajętość dni na podstawie reszty harmonogramu
            foreach (var h in kopia.Where(h => h.Podzadanie != null))
            {
                var dzien = dostep.FirstOrDefault(d => d.Data == h.Data);
                if (dzien != null)
                    dzien.IloscDostepnegoCzasu -= h.IloscGodzin;
            }

            // Przypisz losowo po kawałku do dni przed deadlinem z dostępnym czasem
            while (pozostalo > 0)
            {
                var dostepneDni = dostep
                    .Where(d => d.Data <= deadline && d.IloscDostepnegoCzasu > 0)
                    .OrderByDescending(d => d.IloscDostepnegoCzasu)
                    .ToList();

                if (!dostepneDni.Any())
                    break;

                var dzien = dostepneDni.First();
                int ileMozna = Math.Min(pozostalo, dzien.IloscDostepnegoCzasu);
                int przydziel = rnd.Next(1, ileMozna + 1);

                kopia.Add(new HarmonogramEntry
                {
                    Data = dzien.Data,
                    Podzadanie = losowePodzadanie,
                    IloscGodzin = przydziel
                });

                dzien.IloscDostepnegoCzasu -= przydziel;
                pozostalo -= przydziel;
            }

            // Fallback – jeśli zostało coś, przypisujemy mimo przekroczenia dostępności
            if (pozostalo > 0)
            {
                var fallbackDays = dostep
                    .OrderBy(_ => rnd.Next())
                    .ToList();

                foreach (var dzien in fallbackDays)
                {
                    if (pozostalo <= 0) break;

                    int przydziel = Math.Min(pozostalo, 4); // maksymalny awaryjny kawałek
                    kopia.Add(new HarmonogramEntry
                    {
                        Data = dzien.Data,
                        Podzadanie = losowePodzadanie,
                        IloscGodzin = przydziel
                    });

                    pozostalo -= przydziel;
                }

                if (pozostalo > 0)
                {
                    Console.WriteLine($"[WARN] Nie udało się przypisać {pozostalo}h dla podzadania: {losowePodzadanie.NazwaPodzadania} ({losowePodzadanie.NumerZadania})");
                }
            }

            return kopia
                .OrderBy(h => h.Data)
                .ThenBy(h => h.Podzadanie?.NumerZadania ?? int.MaxValue)
                .ThenBy(h => h.Podzadanie?.NumerPodzadania ?? int.MaxValue)
                .ToList();
        }




    }


}
