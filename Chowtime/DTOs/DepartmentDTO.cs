using SGApp.Models.Common;
using System.Collections.Generic;

namespace SGApp.DTOs
{
    public class DepartmentDTO : IKey
    {
        public string Key { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }

    }
}