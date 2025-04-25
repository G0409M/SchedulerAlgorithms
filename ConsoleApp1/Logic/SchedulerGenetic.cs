using ConsoleApp1.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1.Logic
{
    class SchedulerGenetic
    {
        private readonly List<Zadanie> zadania;
        private readonly List<Podzadanie> podzadania;
        private readonly List<DostepnyCzas> dostepneDni;
        private readonly Dictionary<int, Zadanie> mapaZadan;
        private readonly ScheduleEvaluator evaluator;
        private readonly Random random = new Random();

        private int populacjaRozmiar = 200;
        private int liczbaPokolen = 1000;
        private double prawdopodobienstwoKrzyzowania = 0.6;
        private double prawdopodobienstwoMutacji = 0.3;

        public SchedulerGenetic(List<Zadanie> zadania, List<Podzadanie> podzadania, List<DostepnyCzas> dostepneDni)
        {
            this.zadania = zadania;
            this.podzadania = podzadania;
            this.dostepneDni = dostepneDni;

            this.mapaZadan = zadania.ToDictionary(z => z.Numer);
            this.evaluator = new ScheduleEvaluator(zadania, podzadania, dostepneDni);
        }


        public List<HarmonogramEntry> Uruchom()
        {
            var populacja = GenerujPopulacjeStartowa();
            double najlepszaKara = double.MaxValue;
            List<HarmonogramEntry> najlepszyGlobalnie = null;

            for (int generacja = 0; generacja < liczbaPokolen; generacja++)
            {
                var oceny = populacja.Select(e => new OcenionyHarmonogram
                {
                    Harmonogram = e,
                    Kara = evaluator.ObliczKare(e)
                }).OrderBy(x => x.Kara).ToList();

                var najlepszyWTejGeneracji = oceny.First();

                if (najlepszyWTejGeneracji.Kara < najlepszaKara)
                {
                    najlepszaKara = najlepszyWTejGeneracji.Kara;
                    najlepszyGlobalnie = najlepszyWTejGeneracji.Harmonogram.Select(h => h.Clone()).ToList();
                }

                Console.WriteLine($"[Gen {generacja + 1}/{liczbaPokolen}] Najlepsza kara: {najlepszyWTejGeneracji.Kara:F2}");

                var nowaPopulacja = new List<List<HarmonogramEntry>>();

                while (nowaPopulacja.Count < populacjaRozmiar - 1)
                {
                    var rodzic1 = SelekcjaTurniejowa(oceny);
                    var rodzic2 = SelekcjaTurniejowa(oceny);

                    List<HarmonogramEntry> potomek;

                    if (random.NextDouble() < prawdopodobienstwoKrzyzowania)
                        potomek = Krzyzowanie(rodzic1, rodzic2);
                    else
                        potomek = rodzic1.Select(h => h.Clone()).ToList();

                    if (random.NextDouble() < prawdopodobienstwoMutacji)
                        Mutacja(potomek);

                    nowaPopulacja.Add(potomek);
                }

                nowaPopulacja.Add(najlepszyWTejGeneracji.Harmonogram.Select(h => h.Clone()).ToList());

                populacja = nowaPopulacja;
            }

            Console.WriteLine($"\nZakończono. Najlepszy harmonogram – kara: {najlepszaKara:F2}");
            return najlepszyGlobalnie;
        }




        private List<List<HarmonogramEntry>> GenerujPopulacjeStartowa()
        {
            var populacja = new List<List<HarmonogramEntry>>();

            for (int i = 0; i < populacjaRozmiar; i++)
            {
                populacja.Add(WygenerujLosowyHarmonogram());
            }

            return populacja;
        }

        private List<HarmonogramEntry> WygenerujLosowyHarmonogram()
        {
            var harmonogram = new List<HarmonogramEntry>();

            var dostep = dostepneDni
                .Select(d => new DostepnyCzas(d.Data, d.IloscDostepnegoCzasu))
                .OrderBy(d => d.Data)
                .ToList();

            var dniDaty = dostep.Select(d => d.Data).ToList();
            var mapaZadan = zadania.ToDictionary(z => z.Numer);
            var shuffled = podzadania.OrderBy(_ => random.Next()).ToList();

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
                    int przydziel = random.Next(1, ileMoznaWziac + 1); 

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
                    var fallbackDays = dostep
                        .OrderBy(d => random.Next()) 
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
                        Console.WriteLine($"[WARN] Nie udało się przypisać {pozostalo}h podzadania: {pod.NazwaPodzadania} ({pod.NumerZadania})");
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





        private List<HarmonogramEntry> SelekcjaTurniejowa(List<OcenionyHarmonogram> oceny)
        {
            int turniejRozmiar = 5;
            var kandydaci = oceny.OrderBy(x => random.Next()).Take(turniejRozmiar).ToList();
            return kandydaci.OrderBy(x => x.Kara).First().Harmonogram;
        }

        private List<HarmonogramEntry> Krzyzowanie(List<HarmonogramEntry> rodzic1, List<HarmonogramEntry> rodzic2)
        {
            var potomek = new List<HarmonogramEntry>();

            var dostep = dostepneDni
                .Select(d => new DostepnyCzas(d.Data, d.IloscDostepnegoCzasu))
                .OrderBy(d => d.Data)
                .ToList();

            var dniDaty = dostep.Select(d => d.Data).ToList();
            var mapaZadan = zadania.ToDictionary(z => z.Numer);

            var grupy1 = rodzic1.Where(h => h.Podzadanie != null)
                .GroupBy(h => h.Podzadanie)
                .ToDictionary(g => g.Key, g => g.ToList());

            var grupy2 = rodzic2.Where(h => h.Podzadanie != null)
                .GroupBy(h => h.Podzadanie)
                .ToDictionary(g => g.Key, g => g.ToList());

            var shuffled = podzadania.OrderBy(_ => random.Next()).ToList();

            foreach (var pod in shuffled)
            {
                var deadline = mapaZadan[pod.NumerZadania].TerminRealizacji;
                int pozostalo = pod.SzacowanyCzas;

                List<HarmonogramEntry> wpisyZrodla = null;

                if (grupy1.ContainsKey(pod) && grupy2.ContainsKey(pod))
                    wpisyZrodla = random.Next(2) == 0 ? grupy1[pod] : grupy2[pod];
                else if (grupy1.ContainsKey(pod))
                    wpisyZrodla = grupy1[pod];
                else if (grupy2.ContainsKey(pod))
                    wpisyZrodla = grupy2[pod];

                if (wpisyZrodla != null)
                {
                    foreach (var wpis in wpisyZrodla.OrderBy(_ => random.Next()))
                    {
                        var dzien = dostep.FirstOrDefault(d => d.Data == wpis.Data);
                        if (dzien != null && dzien.IloscDostepnegoCzasu >= wpis.IloscGodzin)
                        {
                            potomek.Add(new HarmonogramEntry
                            {
                                Data = wpis.Data,
                                Podzadanie = pod,
                                IloscGodzin = wpis.IloscGodzin
                            });

                            dzien.IloscDostepnegoCzasu -= wpis.IloscGodzin;
                            pozostalo -= wpis.IloscGodzin;

                            if (pozostalo <= 0)
                                break;
                        }
                    }
                }

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
                    int przydziel = random.Next(1, ileMoznaWziac + 1); 

                    potomek.Add(new HarmonogramEntry
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
                    var fallbackDays = dostep.OrderBy(_ => random.Next()).ToList();

                    foreach (var dzien in fallbackDays)
                    {
                        if (pozostalo <= 0) break;

                        int przydziel = Math.Min(pozostalo, 4); 
                        potomek.Add(new HarmonogramEntry
                        {
                            Data = dzien.Data,
                            Podzadanie = pod,
                            IloscGodzin = przydziel
                        });

                        pozostalo -= przydziel;
                    }

                    if (pozostalo > 0)
                    {
                        Console.WriteLine($"[WARN] Nie udało się przypisać {pozostalo}h podzadania: {pod.NazwaPodzadania} ({pod.NumerZadania})");
                    }
                }
            }

            var datyZajete = potomek.Select(h => h.Data).ToHashSet();
            foreach (var data in dniDaty)
            {
                if (!datyZajete.Contains(data))
                {
                    potomek.Add(new HarmonogramEntry
                    {
                        Data = data,
                        Podzadanie = null,
                        IloscGodzin = 0
                    });
                }
            }

            return potomek
                .OrderBy(h => h.Data)
                .ThenBy(h => h.Podzadanie?.NumerZadania ?? int.MaxValue)
                .ThenBy(h => h.Podzadanie?.NumerPodzadania ?? int.MaxValue)
                .ToList();
        }




        private void Mutacja(List<HarmonogramEntry> harmonogram)
        {
            var dniZostaloCzasu = dostepneDni.ToDictionary(d => d.Data, d => d.IloscDostepnegoCzasu);

            foreach (var h in harmonogram.Where(h => h.Podzadanie != null))
                dniZostaloCzasu[h.Data] -= h.IloscGodzin;

            var wpisyZadaniowe = harmonogram.Where(h => h.Podzadanie != null).ToList();
            int liczbaMutacji = Math.Max(1, (int)Math.Ceiling(wpisyZadaniowe.Count * 0.05));

            for (int i = 0; i < liczbaMutacji; i++)
            {
                var wpis = wpisyZadaniowe[random.Next(wpisyZadaniowe.Count)];

                dniZostaloCzasu[wpis.Data] += wpis.IloscGodzin;
                harmonogram.Remove(wpis);

                var kandydaci = dniZostaloCzasu
                    .Where(kv => kv.Value > 0)
                    .Select(kv => kv.Key)
                    .ToList();

                if (kandydaci.Count > 0)
                {
                    var nowyDzien = kandydaci[random.Next(kandydaci.Count)];
                    int ileMozna = dniZostaloCzasu[nowyDzien];
                    int nowaIlosc = Math.Min(ileMozna, wpis.IloscGodzin);

                    harmonogram.Add(new HarmonogramEntry
                    {
                        Data = nowyDzien,
                        Podzadanie = wpis.Podzadanie,
                        IloscGodzin = nowaIlosc
                    });

                    dniZostaloCzasu[nowyDzien] -= nowaIlosc;

                    int pozostale = wpis.IloscGodzin - nowaIlosc;
                    if (pozostale > 0)
                    {
                        harmonogram.Add(new HarmonogramEntry
                        {
                            Data = wpis.Data,
                            Podzadanie = wpis.Podzadanie,
                            IloscGodzin = pozostale
                        });
                        dniZostaloCzasu[wpis.Data] -= pozostale;
                    }
                }
                else
                {
                    harmonogram.Add(wpis);
                    dniZostaloCzasu[wpis.Data] -= wpis.IloscGodzin;
                }
            }
        }


    }

    class OcenionyHarmonogram
    {
        public List<HarmonogramEntry> Harmonogram { get; set; }
        public double Kara { get; set; }
    }

    static class HarmonogramEntryExtensions
    {
        public static HarmonogramEntry Clone(this HarmonogramEntry entry)
        {
            return new HarmonogramEntry
            {
                Data = entry.Data,
                IloscGodzin = entry.IloscGodzin,
                Podzadanie = entry.Podzadanie 
            };
        }
    }
}
