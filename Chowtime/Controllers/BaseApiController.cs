using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SGApp.DTOs;
using SGApp.Models.EF;
using SGApp.Models.Common;
using SGApp.Filters;
using SGApp.Repository.Application;

namespace SGApp.Controllers {
    [ModelStateValidator]
    public abstract class BaseApiController : ApiController {

        protected static HttpResponseMessage ProcessValidationErrors(HttpRequestMessage request, List<DbValidationError> validationErrors, string key) {
            var col = new Collection<Dictionary<string, string>>();

            foreach (var dic in validationErrors.Select(err => new Dictionary<string, string> {
                {"FieldWithError", err.PropertyName},
                {"Error", err.ErrorMessage}
                })) {
                col.Add(dic);
            }
            var retVal = new GenericDTO {
                Key = key,
                ReturnData = col
            };
            return request.CreateResponse(HttpStatusCode.BadRequest, retVal);
        }

        protected static HttpResponseMessage ProcessValidationErrors(HttpRequestMessage request, Collection<Dictionary<string, string>> validationErrors, string key) {            
            var retVal = new GenericDTO {
                Key = key,
                ReturnData = validationErrors
            };
            return request.CreateResponse(HttpStatusCode.BadRequest, retVal);  
        }
    
        protected static int ValidateUser(string oldKey, out string newKey, ref int companyId) {
            var ur = new AppUserRepository();
            return ur.ValidateUser(oldKey, out newKey, ref companyId);
        }

        protected static Collection<Dictionary<string, string>> ValidateDtoData(IKey dto, EntityBase entity) {
            var retVal = new Collection<Dictionary<string, string>>(); 
            var fieldName = string.Empty;
            foreach (var inputField in dto.GetType().GetProperties().Where(x => x.Name != "Key" && x.GetValue(dto) != null)) {
                if (inputField.Name.StartsWith("Start_")) {
                    fieldName = inputField.Name.Substring(6);                    
                } else if(inputField.Name.StartsWith("End_")){
                    fieldName = inputField.Name.Substring(4);
                } else if(inputField.Name.Contains("__")) {
                    //  using field from a different table
                    //      not currently handling this case
                } else {
                    fieldName = inputField.Name;
                }
                //  insure the entity contains the field represented by fieldName
                var entityFieldName = entity.GetType().GetProperty(fieldName);
                if (entityFieldName == null || entityFieldName.Name.Length == 0) {
                    //  entity does not contain this field
                    continue;
                }
                if (entity.GetDataType(fieldName) == typeof(string)) {
                    //  should be ok
                    continue;
                }
                if (entity.GetDataType(fieldName) == typeof(int) || entity.GetDataType(fieldName) == typeof(int?)) {
                    var nbr = 0;
                    if (!int.TryParse(inputField.GetValue(dto).ToString(), out nbr)) {
                        var dic = new Dictionary<string, string> {{inputField.Name, "Invalid data type cast"}};
                        retVal.Add(dic);
                    }
                    if (entity.GetDataType(fieldName) == typeof(bool) || entity.GetDataType(fieldName) == typeof(bool?)) {
                        if (inputField.GetValue(dto).ToString().ToLower() != "true" && inputField.GetValue(dto).ToString().ToLower() != "false") {
                            var dic = new Dictionary<string, string> { { inputField.Name, "Invalid data type cast" } };
                            retVal.Add(dic);
                        }
                    }
                    if (entity.GetDataType(fieldName) == typeof(DateTime) || entity.GetDataType(fieldName) == typeof(DateTime?)) {
                        DateTime dt;
                        if (!DateTime.TryParse(inputField.GetValue(dto).ToString(), out dt)) {
                            var dic = new Dictionary<string, string> { { inputField.Name, "Invalid data type cast" } };
                            retVal.Add(dic);
                        }
                    }
                    if (entity.GetDataType(fieldName) == typeof(decimal) || entity.GetDataType(fieldName) == typeof(decimal?)) {
                        decimal d;
                        if (!decimal.TryParse(inputField.GetValue(dto).ToString(), out d)) {
                            var dic = new Dictionary<string, string> { { inputField.Name, "Invalid data type cast" } };
                            retVal.Add(dic);
                        }
                    }
                }
            }
            return retVal;
        }
    }
}