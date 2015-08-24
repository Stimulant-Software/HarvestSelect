//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InnovaService
{
    using System;
    using System.Collections.Generic;
    
    public partial class proc_materials
    {
        public proc_materials()
        {
            this.proc_materials1 = new HashSet<proc_materials>();
            this.proc_materials11 = new HashSet<proc_materials>();
            this.proc_materials12 = new HashSet<proc_materials>();
            this.proc_materials13 = new HashSet<proc_materials>();
            this.proc_materials14 = new HashSet<proc_materials>();
            this.proc_materials15 = new HashSet<proc_materials>();
            this.proc_materials16 = new HashSet<proc_materials>();
            this.proc_packs = new HashSet<proc_packs>();
            this.proc_packs1 = new HashSet<proc_packs>();
            this.proc_packs2 = new HashSet<proc_packs>();
        }
    
        public int material { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string shname { get; set; }
        public string extcode { get; set; }
        public string pattern { get; set; }
        public Nullable<int> dimension1 { get; set; }
        public Nullable<int> dimension2 { get; set; }
        public Nullable<int> dimension3 { get; set; }
        public Nullable<int> dimension4 { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public string description3 { get; set; }
        public string description4 { get; set; }
        public string description5 { get; set; }
        public string description6 { get; set; }
        public string description7 { get; set; }
        public string description8 { get; set; }
        public string xmldata { get; set; }
        public Nullable<int> itgrsite { get; set; }
        public string itgrstatus { get; set; }
        public bool active { get; set; }
        public short systemtype { get; set; }
        public short um { get; set; }
        public Nullable<int> condition { get; set; }
        public Nullable<int> quality { get; set; }
        public Nullable<int> conservation { get; set; }
        public Nullable<int> size { get; set; }
        public Nullable<int> materialtype { get; set; }
        public Nullable<int> @base { get; set; }
        public Nullable<int> parent { get; set; }
        public Nullable<float> stdyield { get; set; }
        public Nullable<float> minyield { get; set; }
        public Nullable<float> maxyield { get; set; }
        public Nullable<int> invkeepmin { get; set; }
        public Nullable<int> invkeepmax { get; set; }
        public Nullable<int> invkeepmethod { get; set; }
        public Nullable<int> prdaymin { get; set; }
        public Nullable<int> prdaymax { get; set; }
        public Nullable<int> expire1 { get; set; }
        public Nullable<int> expire1method { get; set; }
        public Nullable<int> expire1min { get; set; }
        public Nullable<int> expire1max { get; set; }
        public Nullable<int> expire2 { get; set; }
        public Nullable<int> expire2method { get; set; }
        public Nullable<int> expire2min { get; set; }
        public Nullable<int> expire2max { get; set; }
        public Nullable<int> expire3 { get; set; }
        public Nullable<int> expire3method { get; set; }
        public Nullable<int> expire3min { get; set; }
        public Nullable<int> expire3max { get; set; }
        public Nullable<int> itpackaging { get; set; }
        public Nullable<int> pkpackaging { get; set; }
        public Nullable<int> stpackaging { get; set; }
        public Nullable<int> papackaging { get; set; }
        public string fabcode1 { get; set; }
        public string fabcode2 { get; set; }
        public string fabcode3 { get; set; }
        public Nullable<float> pkweight { get; set; }
        public Nullable<float> coolant { get; set; }
        public Nullable<int> itlayout { get; set; }
        public Nullable<int> itlabels { get; set; }
        public Nullable<int> pklayout { get; set; }
        public Nullable<int> pklabels { get; set; }
        public Nullable<int> stlayout { get; set; }
        public Nullable<int> stlabels { get; set; }
        public Nullable<int> palayout { get; set; }
        public Nullable<int> palabels { get; set; }
        public Nullable<float> packsize { get; set; }
        public short packsizeum { get; set; }
        public Nullable<float> stacksize { get; set; }
        public short stacksizeum { get; set; }
        public Nullable<float> palletpsize { get; set; }
        public short palletpsizeum { get; set; }
        public Nullable<float> palletssize { get; set; }
        public short palletssizeum { get; set; }
        public Nullable<float> value { get; set; }
        public Nullable<int> currency { get; set; }
        public Nullable<float> price { get; set; }
        public Nullable<float> fprice { get; set; }
        public Nullable<float> dprice { get; set; }
        public Nullable<float> fdprice { get; set; }
        public string barcode { get; set; }
        public Nullable<int> itcontentspecs { get; set; }
        public Nullable<int> pkcontentspecs { get; set; }
        public Nullable<int> bom { get; set; }
        public Nullable<float> pklength { get; set; }
        public Nullable<float> pkheight { get; set; }
        public Nullable<float> pkwidth { get; set; }
        public Nullable<int> idealyieldgroup { get; set; }
        public Nullable<float> pieceweight { get; set; }
        public Nullable<int> recordas { get; set; }
        public Nullable<int> defaultinventory { get; set; }
    
        public virtual ICollection<proc_materials> proc_materials1 { get; set; }
        public virtual proc_materials proc_materials2 { get; set; }
        public virtual ICollection<proc_materials> proc_materials11 { get; set; }
        public virtual proc_materials proc_materials3 { get; set; }
        public virtual ICollection<proc_materials> proc_materials12 { get; set; }
        public virtual proc_materials proc_materials4 { get; set; }
        public virtual ICollection<proc_materials> proc_materials13 { get; set; }
        public virtual proc_materials proc_materials5 { get; set; }
        public virtual ICollection<proc_materials> proc_materials14 { get; set; }
        public virtual proc_materials proc_materials6 { get; set; }
        public virtual ICollection<proc_materials> proc_materials15 { get; set; }
        public virtual proc_materials proc_materials7 { get; set; }
        public virtual proc_sizes proc_sizes { get; set; }
        public virtual ICollection<proc_materials> proc_materials16 { get; set; }
        public virtual proc_materials proc_materials8 { get; set; }
        public virtual ICollection<proc_packs> proc_packs { get; set; }
        public virtual ICollection<proc_packs> proc_packs1 { get; set; }
        public virtual ICollection<proc_packs> proc_packs2 { get; set; }
    }
}