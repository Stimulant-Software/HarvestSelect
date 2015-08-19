using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    [MetadataType(typeof(Role_Metadata))]
    public partial class Role : EntityBase
    {

        public override string KeyName()
        {
            return "RoleId";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members



        #endregion


    }
}

