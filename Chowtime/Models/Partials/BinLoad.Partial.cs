using System.ComponentModel.DataAnnotations;
using SGApp.Models.Common;
using SGApp.Models.Validation;

namespace SGApp.Models.EF {
	[MetadataType(typeof(BinLoad_Metadata))]
	public partial class BinLoad: EntityBase {
		public override string KeyName() {
			return "BinTicketID";
		}

		public override System.Type GetDataType(string fieldName) {
			return GetType().GetProperty(fieldName).PropertyType;
		}
	}
}