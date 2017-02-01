﻿using InnovaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InnovaServiceHost.DTOs {
    public class InnovaDto : IKey {
        public string Key { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OrderCode { get; set; }
    }
}