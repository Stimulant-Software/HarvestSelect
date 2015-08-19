using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    [MetadataType(typeof(Harvest_Metadata))]
    public partial class Harvest : EntityBase
    {

        public override string KeyName()
        {
            return "HarvestId";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members



        #endregion


    }
}

