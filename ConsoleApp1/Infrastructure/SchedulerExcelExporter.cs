using ConsoleApp1.Core;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Infrastructure
{
    class SchedulerExcelExporter
    {
        public void Eksportuj(List<HarmonogramEntry> harmonogram, List<Zadanie> zadania, List<DostepnyCzas> dniRobocze, string sciezka)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var mapaZadan = zadania.ToDictionary(z => z.Numer);
            var mapaDni = dniRobocze.ToDictionary(d => d.Data, d => d.IloscDostepnegoCzasu);

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Harmonogram");

               
                ws.Cells[1, 1].Value = "Data";
                ws.Cells[1, 2].Value = "Zadeklarowany czas [h]";
                ws.Cells[1, 3].Value = "Numer Zadania";
                ws.Cells[1, 4].Value = "Nazwa Zadania";
                ws.Cells[1, 5].Value = "Priorytet Zadania";
                ws.Cells[1, 6].Value = "Numer Podzadania";
                ws.Cells[1, 7].Value = "Nazwa Podzadania";
                ws.Cells[1, 8].Value = "Priorytet Podzadania";
                ws.Cells[1, 9].Value = "Czas [h]";

                int row = 2;

                var grupyPoDacie = harmonogram
                    .GroupBy(h => h.Data)
                    .OrderBy(g => g.Key);

                foreach (var grupa in grupyPoDacie)
                {
                    var wpisy = grupa.Where(h => h.Podzadanie != null).ToList();
                    int zadeklarowanyCzas = mapaDni.ContainsKey(grupa.Key) ? mapaDni[grupa.Key] : 0;

                    if (wpisy.Any())
                    {
                        var grupyWpisow = wpisy
                            .GroupBy(h => new
                            {
                                h.Data,
                                h.Podzadanie.NumerZadania,
                                h.Podzadanie.NumerPodzadania
                            })
                            .OrderBy(g => g.Key.NumerZadania)
                            .ThenBy(g => g.Key.NumerPodzadania);

                        foreach (var wpisGrupa in grupyWpisow)
                        {
                            var h = wpisGrupa.First();
                            var zadanie = mapaZadan[h.Podzadanie.NumerZadania];
                            int sumaGodzin = wpisGrupa.Sum(x => x.IloscGodzin);

                            ws.Cells[row, 1].Value = wpisGrupa.Key.Data.ToShortDateString();
                            ws.Cells[row, 2].Value = zadeklarowanyCzas;
                            ws.Cells[row, 3].Value = h.Podzadanie?.NumerZadania.ToString() ?? "-";
                            ws.Cells[row, 4].Value = zadanie?.Nazwa ?? "-";
                            ws.Cells[row, 5].Value = zadanie?.Priorytet.ToString() ?? "-";
                            ws.Cells[row, 6].Value = h.Podzadanie?.NumerPodzadania.ToString() ?? "-";
                            ws.Cells[row, 7].Value = h.Podzadanie?.NazwaPodzadania ?? "-";
                            ws.Cells[row, 8].Value = h.Podzadanie?.Priorytet.ToString() ?? "-";
                            ws.Cells[row, 9].Value = sumaGodzin;
                            row++;
                        }
                    }
                    else
                    {
                        ws.Cells[row, 1].Value = grupa.Key.ToShortDateString();
                        ws.Cells[row, 2].Value = zadeklarowanyCzas;
                        ws.Cells[row, 3].Value = "-";
                        ws.Cells[row, 4].Value = "-";
                        ws.Cells[row, 5].Value = "-";
                        ws.Cells[row, 6].Value = "-";
                        ws.Cells[row, 7].Value = "-";
                        ws.Cells[row, 8].Value = "-";
                        ws.Cells[row, 9].Value = 0;
                        row++;
                    }
                }

                // Styl nagłówka
                using (var rng = ws.Cells[1, 1, 1, 9])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                package.SaveAs(new FileInfo(sciezka));
            }

            Console.WriteLine($"Zapisano harmonogram do pliku: {sciezka}");
        }



    }

}
