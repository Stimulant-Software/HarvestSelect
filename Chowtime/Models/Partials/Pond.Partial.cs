using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    [MetadataType(typeof(Pond_Metadata))]
    public partial class Pond : EntityBase {

        public override string KeyName() {
            return "PondId";
        }

        public override System.Type GetDataType(string fieldName) {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members



        #endregion

        
    }
}

