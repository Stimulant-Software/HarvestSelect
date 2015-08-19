using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;


namespace SGApp.Models.EF
{
    [MetadataType(typeof(Company_Metadata))]
    public partial class Company : EntityBase, IValidatableObject
    {

        public override string KeyName()
        {
            return "CompanyId";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


        #region IValidatableObject Members
        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CompanyId <= -2)
            {
                yield return new ValidationResult(
                    "CompanyID is required.",
                    new[] { "CompanyID" }
                );

            }

        }


        #endregion


    }
}

