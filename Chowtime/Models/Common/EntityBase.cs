using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SGApp.Models.Validation;


namespace SGApp.Models.Common {

    /// <summary>
    /// Base class for all of the entity classes.
    /// </summary>
    public abstract class EntityBase : IKey {

        #region IKey Members

        public string Key {
            get ;
            set ;
        }

        #endregion        



        #region Public Properties

        [Column("Id")]
        public int Id {
            get;
            set;
        }

        public virtual string KeyName() {
            return string.Empty;
        }

        /// <summary>
        /// Gets or sets the validation messages.
        /// </summary>
        /// <value>
        /// The validation messages.
        /// </value>
        [NotMapped]
        public List<ValidationMessage> ValidationMessages
        {
            get;
            set;
        }        

        /// <summary>
        /// Flag indicating that record is new
        /// </summary>
        public virtual bool IsNew {
            get {               
                return (int.Parse(this.GetType().GetProperty(this.KeyName()).GetValue(this).ToString()) == 0);
            }
        }

        /// <summary>
        /// Flag indicates that save operation is started
        /// for the object.
        /// The property indicates status of the object after calling
        /// Save operation on Repository
        /// </summary>
        [NotMapped]
        public bool IsDirty {
            get;
            set;
        }

        /// <summary>
        /// conversion function to transform string values from parameter 
        /// object received by a Web API into an object that can be
        /// saved or updated in the database
        /// </summary>        
        public void ProcessRecord(IKey dto) {
            foreach (var inputField in dto.GetType().GetProperties().Where(x => x.Name != "Key" && x.GetValue(dto) != null)) {
                var fieldName = string.Empty;
                Type type;
                if (inputField.Name.StartsWith("Start_")) {
                    fieldName = inputField.Name.Substring(6);
                } else if (inputField.Name.StartsWith("End_")) {
                    fieldName = inputField.Name.Substring(4);
                } else if (inputField.Name.Contains("__")) {
                    //  this represents a field in another table, we aren't going
                    //      to attempt handling recursive searching at this time
                    continue;
                } else {
                    fieldName = inputField.Name;
                }                

                if (fieldName != this.KeyName()) {
                    var matchingField = this.GetType().GetProperty(fieldName);
                    if (matchingField == null) {
                        //  represents a field in another object used in searching
                        continue;
                    }
                    type = GetDataType(fieldName);
                    if (type == typeof(string)) {
                        var value = inputField.GetValue(dto).ToString();                        
                        matchingField.SetValue(this, value);
                        continue;
                    }
                    if (type == typeof(int) || type == typeof(int?)) {
                        var value = int.Parse(inputField.GetValue(dto).ToString());
                        matchingField.SetValue(this, value);
                        continue;
                    }
                    
                    if (type == typeof(bool) || type == typeof(bool?)) {
                        var value = bool.Parse(inputField.GetValue(dto).ToString());
                        matchingField.SetValue(this, value);
                        continue;
                    }
                    
                    if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                        var value = DateTime.Parse(inputField.GetValue(dto).ToString());  
                        matchingField.SetValue(this, value);               
                        continue;
                    }
                    
                    if (type == typeof(decimal) || type == typeof(decimal?)) {
                        var value = decimal.Parse(inputField.GetValue(dto).ToString());
                        matchingField.SetValue(this, value);                         
                    }

                    if (type == typeof(byte[]))
                    {
                        var value = System.Text.Encoding.UTF8.GetBytes(inputField.GetValue(dto).ToString());
                        matchingField.SetValue(this, value);                         
                    } 
                    
                }                                    
            }            
        }


        public virtual Type GetDataType(string fieldName) {
            return typeof(string);
        } 


        #endregion

        

                             
    }
}
