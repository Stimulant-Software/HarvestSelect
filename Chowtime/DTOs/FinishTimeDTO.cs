using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class FinishTimeDTO : IKey
    {
        public string Key { get; set; }
        public string FinishTimeID { get; set; }
        public string FinishDateTime { get; set; }
        public string Start_FinishDateTime { get; set; }
        public string End_FinishDateTime { get; set; }
        public string DepartmentID { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }

    }
}