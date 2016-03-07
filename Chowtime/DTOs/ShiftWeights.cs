using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class ShiftWeights
    {
        public List<ShiftWeights> shiftweights { get; set; }

    }
    public class ShiftWeight
    {
        //public string code {get; set;}
        public string Station { get; set; }
        public string Nominal { get; set; }
        public string Weight { get; set; }
    }
}