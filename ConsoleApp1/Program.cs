using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ConsoleApp1.Core;
using ConsoleApp1.Infrastructure;
using ConsoleApp1.Logic;
using OfficeOpenXml;


namespace ConsoleApp1
{
    
    class HarmonogramEntry
    {
        public DateTime Data { get; set; }
        public Podzadanie Podzadanie { get; set; }
        public int IloscGodzin { get; set; } 
    }



    class Program
    {
        static void Main()
        {
            var zadania = SeedDataProvider.GenerujZadania();
            var podzadania = SeedDataProvider.GenerujPodzadania();
            var dni = SeedDataProvider.GenerujDostepnyCzas();

            Console.WriteLine("Wybierz algorytm do tworzenia harmonogramu:");
            Console.WriteLine("h - Hill Climbing");
            Console.WriteLine("g - Algorytm Genetyczny");
            Console.Write(" Wybór: ");
            var wybor = Console.ReadKey().KeyChar;
            Console.WriteLine();

            List<HarmonogramEntry> harmonogram;
            string nazwaPliku;

            if (wybor == 'g')
            {
                Console.WriteLine("Uruchamianie algorytmu genetycznego...");
                var scheduler = new SchedulerGenetic(zadania, podzadania, dni);
                harmonogram = scheduler.Uruchom();
                nazwaPliku = "harmonogram_genetyczny.xlsx";
            }
            else
            {
                Console.WriteLine("Uruchamianie algorytmu Hill Climbing...");
                var scheduler = new SchedulerHillClimber(zadania, podzadania, dni);
                harmonogram = scheduler.Start();
                nazwaPliku = "harmonogram_hill_climbing.xlsx";
            }

            var exporter = new SchedulerExcelExporter();
            exporter.Eksportuj(harmonogram, zadania, dni, nazwaPliku);

            System.Diagnostics.Process.Start("explorer", nazwaPliku);
        }
    }

}