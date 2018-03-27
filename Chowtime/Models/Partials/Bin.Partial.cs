using System.ComponentModel.DataAnnotations;
using SGApp.Models.Common;
using SGApp.Models.Validation;

namespace SGApp.Models.EF {
	[MetadataType(typeof(Bin_Metadata))]
	public partial class Bin: EntityBase {
		public override string KeyName() {
			return "BinID";
		}

		public override System.Type GetDataType(string fieldName) {
			return GetType().GetProperty(fieldName).PropertyType;
		}
	}
}