using FluentValidation.Results;

namespace SGApp.Models.Validation
{
    /// <summary>
    /// Class represents validation message of any DTO object.
    /// </summary>
    public class ValidationMessage
    {
        #region Constructors

        public ValidationMessage()
        {
            this.Category = ValidationMessageCategories.None;
            this.Code = ValidationCodes.None;
        }

        public ValidationMessage(ValidationMessageCategories category, ValidationCodes code, string propertyName = "")
        {
            this.Category = category;
            this.Code = code;
            this.PropertyName = propertyName;
            //this.Message = 
        }


        #endregion

        #region Public Methods

        public static ValidationMessage Create(ValidationFailure validationFailure)
        {
            var result = new ValidationMessage
            {
                Category = ValidationMessageCategories.Error,
                PropertyName = validationFailure.PropertyName,
                Message = validationFailure.ErrorMessage,
                AttemptedValue = validationFailure.AttemptedValue
            };

            if (validationFailure.CustomState is ValidationCodes)
            {
                result.Code = (ValidationCodes)validationFailure.CustomState;
            }
            else
            {
                result.Code = ValidationCodes.FieldLevelValidationBroken;
            }

            return result;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the property that validation is attached to
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the message category
        /// </summary>
        public ValidationMessageCategories Category { get; set; }

        /// <summary>
        /// Gets or sets the message code
        /// </summary>
        public ValidationCodes Code { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public object AttemptedValue { get; set; }


        #endregion
    }
}
