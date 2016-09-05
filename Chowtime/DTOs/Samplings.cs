using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGApp.DTOs
{
    public class Samplings
    {
        public List<Sampling> samplings { get; set; }                             
                          
    }
    public class Sampling
    {
        //public string code {get; set;}
        public string farm {get; set;}
        public string pond {get; set;}
        public string farmPond {get; set;}
        public string date {get; set;}
        public string rangeName {get; set;}
        public string rangeValue {get; set;}
        public string weight {get; set;}
        public string count { get; set; }
    }
}