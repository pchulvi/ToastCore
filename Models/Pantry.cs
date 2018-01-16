using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToastCore.Models
{
    public class Pantry
    {
        public int Id { get; set; }

        public int NumberOfBreads { get; set; }

        public PantryStatus Status { get; set; }

    }
}
