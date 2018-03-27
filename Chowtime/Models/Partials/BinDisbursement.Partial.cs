using System.ComponentModel.DataAnnotations;
using SGApp.Models.Common;
using SGApp.Models.Validation;

namespace SGApp.Models.EF {
	[MetadataType(typeof(BinDisbursement_Metadata))]
	public partial class BinDisbursement: EntityBase {
		public override string KeyName() {
			return "BinDisbursementID";
		}

		public override System.Type GetDataType(string fieldName) {
			return GetType().GetProperty(fieldName).PropertyType;
		}
	}
}