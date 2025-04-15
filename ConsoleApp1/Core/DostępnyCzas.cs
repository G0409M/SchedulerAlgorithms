using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Core
{
    class DostepnyCzas
    {
        public DateTime Data { get; set; }
        public int IloscDostepnegoCzasu { get; set; }

        public DostepnyCzas(DateTime data, int iloscDostepnegoCzasu)
        {
            Data = data;
            IloscDostepnegoCzasu = iloscDostepnegoCzasu;
        }
    }
}
