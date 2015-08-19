using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Reflection;
using System.Text;
using System;
using System.Linq;
using SGApp.Models.Common;

using SGApp.Models.EF;

namespace SGApp.Repository {
    public abstract class RepositoryBase<T> where T : EntityBase {
        #region Properties

        public abstract IQueryable<T> EntityCollection {
            get;
        }
        protected AppEntities DbContext {
            get;
            private set;
        }

        #endregion

        #region Constructors

        protected RepositoryBase() {
            this.DbContext = new AppEntities();
        }

        #endregion


        #region Private Methods

        private Type GetDataType(string entityType, string fieldName) {
            if (entityType == "Project") {
                if (fieldName == "ProjectID")
                    return typeof(int);
                if (fieldName == "Private")
                    return typeof(bool?);
                if (fieldName == "OwnerID")
                    return typeof(int?);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "ProjectName")
                    return typeof(string);
                if (fieldName == "ProjectTypeID")
                    return typeof(int);
                if (fieldName == "StatusID")
                    return typeof(int);
                if (fieldName == "Start_StatusChangeDate")
                    return typeof(DateTime);
                if (fieldName == "End_StatusChangeDate")
                    return typeof(DateTime);
                if (fieldName == "SourceID")
                    return typeof(int);
                if (fieldName == "Active")
                    return typeof(bool);
                if (fieldName == "Address1")
                    return typeof(string);
                if (fieldName == "Address2")
                    return typeof(string);
                if (fieldName == "City")
                    return typeof(string);
                if (fieldName == "County")
                    return typeof(string);
                if (fieldName == "State")
                    return typeof(string);
                if (fieldName == "ZipCode")
                    return typeof(string);
                if (fieldName == "PlantID")
                    return typeof(int?);
                if (fieldName == "SalespersonID")
                    return typeof(int);
                if (fieldName == "Start_BidDate")
                    return typeof(DateTime?);
                if (fieldName == "End_BidDate")
                    return typeof(DateTime?);
                if (fieldName == "LostReasonID")
                    return typeof(int?);
                if (fieldName == "LostToID")
                    return typeof(int?);
                if (fieldName == "LostPrice")
                    return typeof(decimal?);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime?);
                if (fieldName == "End_Modified")
                    return typeof(DateTime?);
                if (fieldName == "PrevStatus")
                    return typeof(int);
                if (fieldName == "Exported")
                    return typeof(bool?);
                if (fieldName == "WinningProjectBidderID")
                    return typeof(int?);
                if (fieldName == "Start_StartDate")
                    return typeof(DateTime?);
                if (fieldName == "End_StartDate")
                    return typeof(DateTime?);
                if (fieldName == "Start_EndDate")
                    return typeof(DateTime?);
                if (fieldName == "End_EndDate")
                    return typeof(DateTime?);
                if (fieldName == "TotalYards")
                    return typeof(int?);
                if (fieldName == "CustCode")
                    return typeof(string);
                if (fieldName == "ConnectionCode")
                    return typeof(string);
                if (fieldName == "ProjectCode")
                    return typeof(string);
                if (fieldName == "Longitude")
                    return typeof(decimal?);
                if (fieldName == "Latitude")
                    return typeof(decimal?);
            }
            if (entityType == "Contact") {
                if (fieldName == "ContactID")
                    return typeof(int);
                if (fieldName == "Private")
                    return typeof(bool?);
                if (fieldName == "OwnerID")
                    return typeof(int?);
                if (fieldName == "FirstName")
                    return typeof(string);
                if (fieldName == "LastName")
                    return typeof(string);
                if (fieldName == "ProspectID")
                    return typeof(int);
                if (fieldName == "Email")
                    return typeof(string);
                if (fieldName == "MobilePhone")
                    return typeof(string);
                if (fieldName == "Phone")
                    return typeof(string);
                if (fieldName == "Fax")
                    return typeof(string);
                if (fieldName == "Address")
                    return typeof(string);
                if (fieldName == "FullName")
                    return typeof(string);
            }
            if (entityType == "Event") {
                if (fieldName == "CustomerEventID")
                    return typeof(int);
                if (fieldName == "Private")
                    return typeof(bool?);
                if (fieldName == "OwnerID")
                    return typeof(int?);
                if (fieldName == "EventTypeID")
                    return typeof(int);
                if (fieldName == "Start_EventStartDate") {
                    return typeof(DateTime);
                }
                if (fieldName == "End_EventStartDate") {
                    return typeof(DateTime);
                }
                if (fieldName == "Start_EventEndDate") {
                    return typeof(DateTime?);
                }
                if (fieldName == "End_EventEndDate") {
                    return typeof(DateTime?);
                }
                if (fieldName == "Start_CompletedDate") {
                    return typeof(DateTime?);
                }
                if (fieldName == "End_CompletedDate") {
                    return typeof(DateTime?);
                }
                if (fieldName == "Description")
                    return typeof(string);
                if (fieldName == "ContactID")
                    return typeof(int?);
                if (fieldName == "UserID")
                    return typeof(int);
                if (fieldName == "EventFrequencyID")
                    return typeof(int);
                if (fieldName == "ProjectID")
                    return typeof(int?);
                if (fieldName == "MasterEventID")
                    return typeof(int?);
                if (fieldName == "EventStatusID")
                    return typeof(int);
                if (fieldName == "Subject")
                    return typeof(string);
                if (fieldName == "ProspectID")
                    return typeof(int?);
                if (fieldName == "Start_RecurrenceEndDate") {
                    return typeof(DateTime?);
                }
                if (fieldName == "End_RecurrenceEndDate") {
                    return typeof(DateTime?);
                }
            }

            if (entityType == "Prospect") {
                if (fieldName == "ProspectID")
                    return typeof(int);
                if (fieldName == "Private")
                    return typeof(bool?);
                if (fieldName == "OwnerID")
                    return typeof(int?);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "ProspectName")
                    return typeof(string);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime);
                if (fieldName == "End_Modified")
                    return typeof(DateTime);
                if (fieldName == "ProspectCode")
                    return typeof(string);
                if (fieldName == "ConnectionCode")
                    return typeof(string);
                if (fieldName == "LocalCustNumber")
                    return typeof(string);
                if (fieldName == "LocalCustName")
                    return typeof(string);
                if (fieldName == "Credit_Status_ID")
                    return typeof(int?);
                if (fieldName == "CustomerStatus")
                    return typeof(string);
                if (fieldName == "Sales")
                    return typeof(decimal?);
                if (fieldName == "Margin")
                    return typeof(decimal?);
                if (fieldName == "MaterialMargin")
                    return typeof(decimal?);
                if (fieldName == "JobCount")
                    return typeof(int?);
                if (fieldName == "Address1")
                    return typeof(string);
                if (fieldName == "Address2")
                    return typeof(string);
                if (fieldName == "City")
                    return typeof(string);
                if (fieldName == "State")
                    return typeof(string);
                if (fieldName == "Zip")
                    return typeof(string);
                if (fieldName == "Phone")
                    return typeof(string);
                if (fieldName == "HighestBalance")
                    return typeof(decimal?);
                if (fieldName == "AvgInvoiceAmt")
                    return typeof(decimal?);
                if (fieldName == "AvgDaysToPay")
                    return typeof(int?);
                if (fieldName == "AvgDaysPastDue")
                    return typeof(int?);
                if (fieldName == "TotalDue")
                    return typeof(decimal?);
                if (fieldName == "ZeroTo30Days")
                    return typeof(decimal?);
                if (fieldName == "ThirtyOneTo60Days")
                    return typeof(decimal?);
                if (fieldName == "SixtyOneTo90Days")
                    return typeof(decimal?);
                if (fieldName == "NinetyOneTo120Days")
                    return typeof(decimal?);
                if (fieldName == "Over120Days")
                    return typeof(decimal?);
                if (fieldName == "CustomerTypeID")
                    return typeof(int?);
                if (fieldName == "StatementFrequency")
                    return typeof(string);
                if (fieldName == "FinanceRate")
                    return typeof(decimal?);
                if (fieldName == "PaymentTermsID")
                    return typeof(int?);
                if (fieldName == "SalesmanCode")
                    return typeof(string);
            }

            if (entityType == "Product") {
                if (fieldName == "ProductID")
                    return typeof(int);
                if (fieldName == "ProductName")
                    return typeof(string);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "CompanyName")
                    return typeof(string);
                if (fieldName == "Active")
                    return typeof(bool?);
                if (fieldName == "AU_ID")
                    return typeof(int?);
                if (fieldName == "UnitOfMeasureID")
                    return typeof(int?);
                if (fieldName == "Imperial_Description")
                    return typeof(string);
                if (fieldName == "ProductTypeID")
                    return typeof(int?);
                if (fieldName == "ProductTypeName")
                    return typeof(string);
                if (fieldName == "ConnectionCode")
                    return typeof(string);
                if (fieldName == "ITEM_CAT")
                    return typeof(string);
                if (fieldName == "Product_Code")
                    return typeof(string);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime?);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime?);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime?);
                if (fieldName == "End_Modified")
                    return typeof(DateTime?);
                if (fieldName == "extProduct_Code")
                    return typeof(string);
            }

            if (entityType == "ProductLine") {
                if (fieldName == "ProductLineID")
                    return typeof(int);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "ProductLineCode")
                    return typeof(string);
                if (fieldName == "ProductLineName")
                    return typeof(string);
                if (fieldName == "Active")
                    return typeof(bool);
                if (fieldName == "extProductLineCode")
                    return typeof(string);
            }

            if (entityType == "ProductType") {
                if (fieldName == "ProductTypeID")
                    return typeof(int);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "CSItemType")
                    return typeof(string);
                if (fieldName == "ProductTypeName")
                    return typeof(string);
                if (fieldName == "ProductLineID")
                    return typeof(int?);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime);
                if (fieldName == "End_Modified")
                    return typeof(DateTime);
                if (fieldName == "Active")
                    return typeof(bool);
                if (fieldName == "ProductTypeCode")
                    return typeof(string);
            }

            if (entityType == "Plant") {
                if (fieldName == "PlantID")
                    return typeof(int);
                if (fieldName == "Description")
                    return typeof(string);
                if (fieldName == "SystechCompNbr")
                    return typeof(string);
                if (fieldName == "CSPlantCode")
                    return typeof(string);
                if (fieldName == "CSShortName")
                    return typeof(string);
                if (fieldName == "CSPlantName")
                    return typeof(string);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime);
                if (fieldName == "End_Modified")
                    return typeof(DateTime);
                if (fieldName == "Inactive_Flag")
                    return typeof(bool);
                if (fieldName == "DivisionID")
                    return typeof(int);
                if (fieldName == "PlantCost")
                    return typeof(decimal?);
                if (fieldName == "DeliveryCost")
                    return typeof(decimal?);
                if (fieldName == "MaterialCost")
                    return typeof(decimal?);
                if (fieldName == "SGACost")
                    return typeof(decimal?);
                if (fieldName == "Address1")
                    return typeof(string);
                if (fieldName == "Address2")
                    return typeof(string);
                if (fieldName == "City")
                    return typeof(string);
                if (fieldName == "State")
                    return typeof(string);
                if (fieldName == "Zip")
                    return typeof(string);
                if (fieldName == "Longitude")
                    return typeof(decimal?);
                if (fieldName == "Latitude")
                    return typeof(decimal?);
            }

            if (entityType == "Competitor") {
                if (fieldName == "CompetitorID")
                    return typeof(int);
                if (fieldName == "CompanyID")
                    return typeof(int);
                if (fieldName == "CompetitorName")
                    return typeof(string);
                if (fieldName == "Start_Inserted")
                    return typeof(DateTime);
                if (fieldName == "End_Inserted")
                    return typeof(DateTime);
                if (fieldName == "Start_Modified")
                    return typeof(DateTime);
                if (fieldName == "End_Modified")
                    return typeof(DateTime);
                if (fieldName == "Inactive")
                    return typeof(bool);
                if (fieldName == "UserID")
                    return typeof(int?);
                if (fieldName == "Address1")
                    return typeof(string);
                if (fieldName == "Address2")
                    return typeof(string);
                if (fieldName == "City")
                    return typeof(string);
                if (fieldName == "State")
                    return typeof(string);
                if (fieldName == "Zip")
                    return typeof(string);
                if (fieldName == "Longitude")
                    return typeof(decimal?);
                if (fieldName == "Latitude")
                    return typeof(decimal?);
            }

            return null;
        }




        #endregion

        #region Public Methods

        /// <summary>
        /// Override the method to implement default values for the entity
        /// returned
        /// </summary>
        /// <returns></returns>
        public virtual T Create() {
            return Activator.CreateInstance<T>();
        }

        /// <summary>
        /// Override the method to implement custom
        /// retrieve entity from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract T GetById(int id);


        /// <summary>
        /// Save record to database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Save(T entity) {
            if (entity == null) {
                throw new ArgumentNullException("entity");
            }

            entity.IsDirty = true;

            // implement chain of command
            if (entity.IsNew) {
                // 1. check can insert
                if (this.OnBeforeInsert(entity)) {
                    // 2. Perform actual insert
                    return this.InsertRecord(entity);
                }

                // escape: return unmodified / unsaved entity
                return entity;
            } else {

                // 2. check can update
                if (this.OnBeforeUpdate(entity)) {
                    // 3. Perform actual update
                    return this.UpdateRecord(entity);
                }

                return entity;

            }
        }

        /// <summary>
        /// Delete record from the database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Delete(T entity) {
            if (entity == null) {
                throw new ArgumentNullException("entity");
            }
            // new entity is not in the database
            if (entity.IsNew)
                return entity;


            // 2. check can update
            if (this.OnBeforeDelete(entity)) {
                return this.DeleteRecord(entity);
            }

            return entity;

        }

        /// <summary>
        /// Deletes the record.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected abstract T DeleteRecord(T entity);

        /// <summary>
        /// Called when [before delete].
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected virtual bool OnBeforeDelete(T entity) {
            return true;
        }

        /// <summary>
        /// Override the method to handle before insert
        /// functionality
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool OnBeforeInsert(T entity) {
            return true;
        }

        /// <summary>
        /// Implement the method which inserts new entity
        /// into the database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract T InsertRecord(T entity);

        /// <summary>
        /// Override the method to handle before update
        /// functionality
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool OnBeforeUpdate(T entity) {
            return true;
        }

        /// <summary>
        /// Implement the method which inserts new entity
        /// into the database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract T UpdateRecord(T entity);

        public string GetPredicate(IKey dto, EntityBase entity, int companyId) {
            var sb = new StringBuilder();
            var fld = dto.GetType().GetProperty("CompanyID");
            if (fld != null) {
                var type = entity.GetDataType("CompanyID");
                if (type == typeof(int)) {
                    sb.Append(string.Format("{0}{1}{2}", "CompanyID", "==", companyId));
                }
                if (type == typeof(int?)) {
                    sb.Append(string.Format("{0}{1}{2}{3}", "CompanyID", ".Value", "==", companyId));
                }
            }
            foreach (var field in dto.GetType().GetProperties().Where(x => x.Name != "Key" && x.GetValue(dto) != null)) {
                var value = field.GetValue(dto).ToString();
                if (!string.IsNullOrEmpty(value)) {
                    if (sb.Length > 0) {
                        sb.Append(" && ");
                    }
                    if (field.Name.Contains("__")) {
                        //  embedded table.field reference
                        sb.Append(FormatEmbeddedReference(field, field.GetValue(dto).ToString()));
                    } else {
                        var fieldName = string.Empty;
                        if (field.Name.StartsWith("Start_")) {
                            fieldName = field.Name.Substring(6);
                        } else if (field.Name.StartsWith("End_")) {
                            fieldName = field.Name.Substring(4);
                        } else {
                            fieldName = field.Name;
                        }
                        var type = entity.GetDataType(fieldName);
                        if (type == typeof(string)) {
                            sb.Append(fieldName + ".Contains('");
                            sb.Append(String.Format("{0}{1}", field.GetValue(dto), "')"));
                            //sb.Append(field.GetValue(dto) + ")");
                            continue;
                        }
                        if (type == typeof(int)) {
                            sb.Append(string.Format("{0}{1}{2}", fieldName, "==", int.Parse(field.GetValue(dto).ToString())));
                            continue;
                        }
                        if (type == typeof(int?)) {
                            sb.Append(string.Format("{0}{1}{2}{3}", fieldName, ".Value", "==", int.Parse(field.GetValue(dto).ToString())));
                            continue;
                        }
                        if (type == typeof(bool)) {
                            sb.Append(string.Format("{0}{1}{2}", fieldName, "==", bool.Parse(field.GetValue(dto).ToString())));
                            continue;
                        }
                        if (type == typeof(bool?)) {
                            sb.Append(string.Format("{0}{1}{2}{3}", fieldName, ".Value", "==", bool.Parse(field.GetValue(dto).ToString())));
                            continue;
                        }
                        if (type == typeof(DateTime?)) {
                            var comparer = string.Empty;
                            var date = string.Empty;
                            if (field.Name.StartsWith("Start_")) {
                                comparer = ">";
                                date = DateTime.Parse(field.GetValue(dto).ToString()).AddDays(-1).ToShortDateString();
                            } else {
                                comparer = "<";
                                date = DateTime.Parse(field.GetValue(dto).ToString()).AddDays(1).ToShortDateString();
                            }
                            fieldName = fieldName + ".Value";
                            sb.Append(String.Format("{0}{1}{2}{3}{2}", fieldName, comparer, "'", date));
                            continue;
                        }
                        if (type == typeof(DateTime)) {
                            var comparer = string.Empty;
                            var date = string.Empty;
                            if (field.Name.StartsWith("Start_")) {
                                comparer = ">";
                                date = DateTime.Parse(field.GetValue(dto).ToString()).AddDays(-1).ToShortDateString();
                            } else {
                                comparer = "<";
                                date = DateTime.Parse(field.GetValue(dto).ToString()).AddDays(1).ToShortDateString();
                            }
                            sb.Append(String.Format("{0}{1}{2}{3}{2}", fieldName, comparer, "'", date));
                            continue;
                        }
                        if (type == typeof(decimal)) {
                            sb.Append(string.Format("{0}{1}{2}", fieldName, "==", decimal.Parse(field.GetValue(dto).ToString())));
                        }
                        if (type == typeof(decimal?)) {
                            sb.Append(string.Format("{0}{1}{2}{3}", fieldName, ".Value", "==", decimal.Parse(field.GetValue(dto).ToString())));
                        }
                    }
                }
            }
            return sb.ToString();
        }


        protected string FormatEmbeddedReference(PropertyInfo field, string fieldValue) {
            var entityName = field.Name.Substring(0, field.Name.IndexOf("__"));
            var fieldName = field.Name.Substring(field.Name.IndexOf("__") + 2);
            var dataType = GetDataType(entityName, fieldName);
            if (dataType == typeof(string)) {
                return String.Format("{0}{1}{2}{3}{4}{5}", entityName, ".", fieldName, ".Contains('", fieldValue, "')");
            }
            if (dataType == typeof(int)) {
                return string.Format("{0}{1}{2}{3}{4}", entityName, ".", fieldName, "==", int.Parse(fieldValue));
            }
            if (dataType == typeof(int?)) {
                return string.Format("{0}{1}{2}{3}{4}{5}", entityName, ".", fieldName, ".Value", "==", int.Parse(fieldValue));
            }
            if (dataType == typeof(bool)) {
                return string.Format("{0}{1}{2}{3}{4}", entityName, ".", fieldName, "==", bool.Parse(fieldValue));
            }
            if (dataType == typeof(bool?)) {
                return string.Format("{0}{1}{2}{3}{4}{5}", entityName, ".", fieldName, ".Value", "==", bool.Parse(fieldValue));
            }
            if (dataType == typeof(DateTime?)) {
                var fldName = string.Empty;
                var comparer = string.Empty;
                var date = string.Empty;
                if (fieldName.StartsWith("Start_")) {
                    fieldName = fieldName.Substring(6);
                    comparer = ">";
                    date = DateTime.Parse(fieldValue).AddDays(-1).ToShortDateString();
                } else {
                    fieldName = fieldName.Substring(4);
                    comparer = "<";
                    date = DateTime.Parse(fieldValue).AddDays(1).ToShortDateString();
                }
                fldName = fldName + ".Value";
                return String.Format("{0}{1}{2}{3}{4}{5}{4}", entityName, ".", fldName, comparer, "'", date);
            }
            if (dataType == typeof(DateTime)) {
                var fldName = string.Empty;
                var comparer = string.Empty;
                var date = string.Empty;
                if (fieldName.StartsWith("Start_")) {
                    fldName = fieldName.Substring(6);
                    comparer = ">";
                    date = DateTime.Parse(fieldValue).AddDays(-1).ToShortDateString();
                } else {
                    fldName = fieldName.Substring(4);
                    comparer = "<";
                    date = DateTime.Parse(fieldValue).AddDays(1).ToShortDateString();
                }
                return String.Format("{0}{1}{2}{3}{4}{5}{4}", entityName, ".", fldName, comparer, "'", date);
            }
            if (dataType == typeof(decimal)) {
                return string.Format("{0}{1}{2}{3}{4}", entityName, ".", fieldName, "==", decimal.Parse(fieldValue));
            }
            if (dataType == typeof(decimal?)) {
                return string.Format("{0}{1}{2}{3}{4}{5}", entityName, ".", fieldName, ".Value", "==", decimal.Parse(fieldValue));
            }
            return null;
        }

        public abstract List<T> GetByPredicate(string predicate);

        public List<DbValidationError> Validate(EntityBase entity) {
            return DbContext.Entry(entity).GetValidationResult().ValidationErrors.ToList();
        }


        

        #endregion
    }
}
