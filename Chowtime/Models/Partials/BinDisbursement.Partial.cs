using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SGApp.Models.Common;
using SGApp.Models.Validation;

namespace SGApp.Models.EF {
	[MetadataType(typeof(BinDisbursement_Metadata))]
	public partial class BinDisbursement: EntityBase, IValidatableObject {
		public override string KeyName() {
			return "BinDisbursementID";
		}

		public override System.Type GetDataType(string fieldName) {
			return GetType().GetProperty(fieldName).PropertyType;
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
			if (DateCreated < new DateTime(2017, 12, 31)) {
				yield return new ValidationResult(
					"DateCreated is not valid. Notify Tech Support.",
					new[] { "DateCreated" }
				);
			}
			if (UserID < 1) {
				yield return new ValidationResult(
					"UserID was not set. Notify Tech Support",
					new[] { "UserID" }
				);
			}
			if (DisbursementDate < new DateTime(2017, 12, 31)) {
				yield return new ValidationResult(
					"DisbursementDate is not valid. Notify Tech Support.",
					new[] { "DisbursementDate" }
				);
			}
			if (Pounds < 1) {
				yield return new ValidationResult(
					"Pounds disbursed must be greater than zero.",
					new[] { "Pounds" }
				);
			}
			if (TicketNumber < 0) {
				yield return new ValidationResult(
					"Ticket Number must be greater than zero.",
					new[] { "TicketNumber" }
				);
			}
			if (BinID < 1) {
				yield return new ValidationResult(
					"BinID was not specified. Notify Tech Support.",
					new[] { "BinID" }
				);
			}
		}
	}
}