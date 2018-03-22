using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    [MetadataType(typeof(Bin_Metadata))]
    public partial class Bin : EntityBase
    {

        public override string KeyName()
        {
            return "BinID";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members



        #endregion


    }
}

