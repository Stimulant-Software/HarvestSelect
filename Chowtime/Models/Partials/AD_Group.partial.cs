using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    //[MetadataType(typeof(Email_Metadata))]
    public partial class AD_Group : EntityBase
    {

        public override string KeyName()
        {
            return "AD_GroupId";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members



        #endregion


    }
}

