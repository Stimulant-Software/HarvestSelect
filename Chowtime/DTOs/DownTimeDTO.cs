using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class DownTimeDTO : IKey
    {
        public string Key { get; set; }
        public string DownTimeID { get; set; }
        public string DownTimeDate { get; set; }
        public string Start_DownTimeDate { get; set; }
        public string End_DownTimeDate { get; set; }
        public string DownTimeTypeID { get; set; }
        public string Minutes { get; set; }
        public string DownTimeNote { get; set; }
        public string StartDateMonth { get; set; }
        public string StartDateYear { get; set; }
        public string DepartmentTypeID { get; set; }
        public string DepartmentID { get; set; }
    }
}