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
    
    public partial class proc_packs
    {
        public proc_packs()
        {
            this.proc_lots = new HashSet<proc_lots>();
        }
    
        public int id { get; set; }
        public int site { get; set; }
        public int number { get; set; }
        public string sscc { get; set; }
        public Nullable<int> conum { get; set; }
        public Nullable<int> productnum { get; set; }
        public Nullable<int> processnum { get; set; }
        public Nullable<int> ordernum { get; set; }
        public Nullable<int> invnum { get; set; }
        public string fixedcode { get; set; }
        public Nullable<int> extnum { get; set; }
        public string extcode { get; set; }
        public Nullable<int> prperiod { get; set; }
        public Nullable<int> sprperiod { get; set; }
        public Nullable<int> shift { get; set; }
        public Nullable<int> po { get; set; }
        public Nullable<int> poline { get; set; }
        public Nullable<int> cday { get; set; }
        public Nullable<int> batch { get; set; }
        public Nullable<int> lot { get; set; }
        public Nullable<int> plot { get; set; }
        public Nullable<int> rmarea { get; set; }
        public Nullable<System.DateTime> prday { get; set; }
        public Nullable<System.DateTime> exday { get; set; }
        public Nullable<System.DateTime> exday2 { get; set; }
        public Nullable<int> origin { get; set; }
        public Nullable<int> employee { get; set; }
        public Nullable<int> device { get; set; }
        public Nullable<int> station { get; set; }
        public Nullable<int> manufacturer { get; set; }
        public Nullable<System.DateTime> invkeepmin { get; set; }
        public Nullable<System.DateTime> invkeepmax { get; set; }
        public Nullable<System.DateTime> expire1 { get; set; }
        public Nullable<System.DateTime> expire2 { get; set; }
        public Nullable<System.DateTime> expire3 { get; set; }
        public Nullable<int> order { get; set; }
        public Nullable<int> orderline { get; set; }
        public Nullable<int> rorder { get; set; }
        public Nullable<int> rsequence { get; set; }
        public Nullable<int> porder { get; set; }
        public Nullable<int> porderline { get; set; }
        public Nullable<int> inventory { get; set; }
        public Nullable<int> invlocation { get; set; }
        public Nullable<System.DateTime> invtime { get; set; }
        public short invstatus { get; set; }
        public Nullable<int> stack { get; set; }
        public Nullable<int> pallet { get; set; }
        public Nullable<int> container { get; set; }
        public Nullable<int> shipment { get; set; }
        public int material { get; set; }
        public Nullable<int> packaging { get; set; }
        public Nullable<int> layout { get; set; }
        public Nullable<float> weight { get; set; }
        public Nullable<float> curweight { get; set; }
        public Nullable<float> nominal { get; set; }
        public Nullable<float> curnominal { get; set; }
        public string nominall { get; set; }
        public short nominalu { get; set; }
        public Nullable<float> gross { get; set; }
        public Nullable<float> target { get; set; }
        public Nullable<float> tare { get; set; }
        public short taretype { get; set; }
        public Nullable<float> extra { get; set; }
        public Nullable<float> vgiveaway { get; set; }
        public Nullable<float> fgiveaway { get; set; }
        public Nullable<float> fat { get; set; }
        public Nullable<float> value { get; set; }
        public Nullable<float> curvalue { get; set; }
        public Nullable<int> currency { get; set; }
        public Nullable<float> price { get; set; }
        public Nullable<float> basisprice { get; set; }
        public Nullable<float> dprice { get; set; }
        public Nullable<float> basisdprice { get; set; }
        public Nullable<int> pieces { get; set; }
        public Nullable<int> curpieces { get; set; }
        public Nullable<int> piecesl { get; set; }
        public short checkstatus { get; set; }
        public Nullable<int> aseconds { get; set; }
        public Nullable<float> curamount { get; set; }
        public Nullable<float> maxamount { get; set; }
        public short amountum { get; set; }
        public short itemtype { get; set; }
        public short status { get; set; }
        public int nregs { get; set; }
        public System.DateTime regtime { get; set; }
        public System.DateTime xacttime { get; set; }
        public Nullable<int> alibi { get; set; }
        public short rtype { get; set; }
        public Nullable<int> rstat { get; set; }
        public Nullable<int> recordedby { get; set; }
        public string extbatchcode { get; set; }
        public Nullable<int> qamark { get; set; }
        public Nullable<int> recordingmaterial { get; set; }
        public Nullable<int> rorderline { get; set; }
    
        public virtual base_companies base_companies { get; set; }
        public virtual ICollection<proc_lots> proc_lots { get; set; }
        public virtual proc_lots proc_lots1 { get; set; }
        public virtual proc_materials proc_materials { get; set; }
        public virtual proc_materials proc_materials1 { get; set; }
        public virtual proc_materials proc_materials2 { get; set; }
    }
}
