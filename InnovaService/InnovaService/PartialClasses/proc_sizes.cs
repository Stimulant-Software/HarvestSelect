using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace InnovaService {
    [DataContract]
    public partial class proc_sizes {
        [Key]
        [DataMember(IsRequired = true)]
        public int size { get; set; }
        [DataMember(IsRequired = true)]
        public string code { get; set; }
        [DataMember(IsRequired = true)]
        public string name { get; set; }
        [DataMember(IsRequired = true)]
        public string shname { get; set; }
        [DataMember(IsRequired = false)]
        public string extcode { get; set; }
        [DataMember(IsRequired = false)]
        public string pattern { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<int> dimension1 { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<int> dimension2 { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<int> dimension3 { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<int> dimension4 { get; set; }
        [DataMember(IsRequired = false)]
        public string description1 { get; set; }
        [DataMember(IsRequired = false)]
        public string description2 { get; set; }
        [DataMember(IsRequired = false)]
        public string description3 { get; set; }
        [DataMember(IsRequired = false)]
        public string description4 { get; set; }
        [DataMember(IsRequired = false)]
        public string description5 { get; set; }
        [DataMember(IsRequired = false)]
        public string description6 { get; set; }
        [DataMember(IsRequired = false)]
        public string description7 { get; set; }
        [DataMember(IsRequired = false)]
        public string description8 { get; set; }
        [DataMember(IsRequired = false)]
        public string xmldata { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<int> itgrsite { get; set; }
        [DataMember(IsRequired = false)]
        public string itgrstatus { get; set; }
        [DataMember(IsRequired = true)]
        public bool active { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<float> minweight { get; set; }
        [DataMember(IsRequired = false)]
        public Nullable<float> maxweight { get; set; }
    }
}
