namespace SGApp.Models.Validation
{
    public enum ValidationCodes
    {
        None = 0,

        #region Generic Error Codes
        FieldLevelValidationBroken,
        Required,
        MinLength,
        MaxLength,
        Range,
        LengthRange,
        IncorrectFormat,
        MinValue,
        MaxValue,
        #endregion
    }
}
