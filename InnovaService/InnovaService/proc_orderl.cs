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
    
    public partial class proc_orderl
    {
        public proc_orderl()
        {
            this.proc_packs = new HashSet<proc_packs>();
            this.proc_packs1 = new HashSet<proc_packs>();
            this.proc_packs2 = new HashSet<proc_packs>();
        }
    
        public int id { get; set; }
        public int order { get; set; }
        public Nullable<int> sequence { get; set; }
        public string extcode { get; set; }
        public int material { get; set; }
        public Nullable<int> linegroup { get; set; }
        public short olstatus { get; set; }
        public short assigntype { get; set; }
        public short unittype { get; set; }
        public Nullable<float> maxamount { get; set; }
        public Nullable<float> amount { get; set; }
        public Nullable<float> curamount { get; set; }
        public Nullable<float> pndamount { get; set; }
        public short amountum { get; set; }
        public bool nolimit { get; set; }
        public Nullable<float> completepercentage { get; set; }
        public Nullable<float> allowpercentage { get; set; }
        public Nullable<int> lclayout { get; set; }
        public Nullable<int> expriority { get; set; }
        public string description1 { get; set; }
        public string description2 { get; set; }
        public string description3 { get; set; }
        public string description4 { get; set; }
        public string description5 { get; set; }
        public string description6 { get; set; }
        public string description7 { get; set; }
        public string description8 { get; set; }
        public Nullable<int> exstatus { get; set; }
        public bool isupdated { get; set; }
        public Nullable<int> sourceinventory { get; set; }
        public Nullable<int> sourcepo { get; set; }
        public Nullable<int> sourcesupplier { get; set; }
        public Nullable<int> sourcesite { get; set; }
        public bool useco { get; set; }
        public Nullable<int> expire1 { get; set; }
        public Nullable<int> expire1method { get; set; }
        public Nullable<int> expire2 { get; set; }
        public Nullable<int> expire2method { get; set; }
        public Nullable<int> expire3 { get; set; }
        public Nullable<int> expire3method { get; set; }
        public Nullable<int> itpackaging { get; set; }
        public Nullable<int> pkpackaging { get; set; }
        public Nullable<int> stpackaging { get; set; }
        public Nullable<int> papackaging { get; set; }
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
        public Nullable<int> itcontentspecs { get; set; }
        public Nullable<int> pkcontentspecs { get; set; }
        public Nullable<int> bom { get; set; }
        public Nullable<System.DateTime> prdaymin { get; set; }
        public Nullable<System.DateTime> prdaymax { get; set; }
        public Nullable<System.DateTime> expire1min { get; set; }
        public Nullable<System.DateTime> expire1max { get; set; }
        public Nullable<System.DateTime> expire2min { get; set; }
        public Nullable<System.DateTime> expire2max { get; set; }
        public Nullable<System.DateTime> expire3min { get; set; }
        public Nullable<System.DateTime> expire3max { get; set; }
        public bool allowchange { get; set; }
        public Nullable<float> minpieceweight { get; set; }
        public Nullable<float> maxpieceweight { get; set; }
        public short planstatus { get; set; }
        public Nullable<int> itgrsite { get; set; }
        public Nullable<int> itgrid { get; set; }
        public Nullable<float> shippedamount { get; set; }
        public Nullable<int> orderprocess { get; set; }
        public Nullable<int> sourceowner { get; set; }
        public Nullable<int> sourcelot { get; set; }
        public Nullable<float> curnominal { get; set; }
        public Nullable<float> pndnominal { get; set; }
        public Nullable<float> receivedamount { get; set; }
    
        public virtual base_companies base_companies { get; set; }
        public virtual base_companies base_companies1 { get; set; }
        public virtual proc_lots proc_lots { get; set; }
        public virtual proc_materials proc_materials { get; set; }
        public virtual proc_materials proc_materials1 { get; set; }
        public virtual proc_materials proc_materials2 { get; set; }
        public virtual proc_materials proc_materials3 { get; set; }
        public virtual proc_materials proc_materials4 { get; set; }
        public virtual proc_orders proc_orders { get; set; }
        public virtual ICollection<proc_packs> proc_packs { get; set; }
        public virtual ICollection<proc_packs> proc_packs1 { get; set; }
        public virtual ICollection<proc_packs> proc_packs2 { get; set; }
    }
}
