using System.ComponentModel.DataAnnotations;
using SGApp.Models.Validation;
using SGApp.Models.Common;
namespace SGApp.Models.EF
{
    public partial class FinishTime : EntityBase
    {

        public override string KeyName()
        {
            return "FinishTimeID";
        }

        public override System.Type GetDataType(string fieldName)
        {
            return GetType().GetProperty(fieldName).PropertyType;
        }


    }
}
