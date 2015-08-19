using FluentValidation;
using FluentValidation.Attributes;
using SGApp.Models.Common;
using SGApp.Models.Validation;
using System;
using System.Diagnostics.Contracts;

namespace SGApp.BusinessLogic

{
    public abstract class BusinessDomainBase<T> where T: EntityBase
    {
        #region Constructors

        protected BusinessDomainBase()
        {
            
        }

        #endregion

        #region Public Methods

        public bool Validate(T entity)
        {
            Contract.Requires(entity != null);

            // cleanup current messages
            entity.ValidationMessages.Clear();

            // perform field level validation and
            // if everything is ok -> business validation
            bool result = this.FieldValidation(entity);
            if (result)
            {
                result = this.BusinessValidation(entity);
            }

            return result;

        }


        /// <summary>
        /// Override the method to implement custom
        /// field level validation.
        /// By default FluentValidation is applied
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>
        /// True is object is valid. False if not. In addition validator
        /// should populate collection of ValidationMessages
        /// </returns>
        protected virtual bool FieldValidation(T entity)
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(ValidatorAttribute), true);

            foreach (var attr in attrs)
            {
                var valattr = attr as ValidatorAttribute;
                if (valattr != null)
                {
                    var validator = Activator.CreateInstance(valattr.ValidatorType) as IValidator;
                    if (validator != null)
                    {
                        var validationResult = validator.Validate(entity);
                        var errors = validationResult.Errors;
                        foreach (var validationFailure in errors)
                        {
                            entity.ValidationMessages.Add(ValidationMessage.Create(validationFailure));
                        }
                    }
                }
            }

            return (entity.ValidationMessages.Count == 0);
        }

        /// <summary>
        /// Override the method to implement custom
        /// business validation.
        /// Executed after field-validation only if correct.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>
        /// True is object is valid. False if not. In addition validator
        /// should populate collection of ValidationMessages
        /// </returns>
        protected virtual bool BusinessValidation(T entity)
        {
            return true;
        }

        #endregion
    }
}
