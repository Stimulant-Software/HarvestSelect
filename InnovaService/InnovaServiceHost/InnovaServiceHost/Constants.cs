using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InnovaServiceHost {
    public class Constants {
        public const string hash = "U*r#c15@";
        public const string salt = "@8^c3&@c";
        public const string VIKey = "@n5$5-#49+mE&4m$";
        public const string Max1 = "Maximum field size is 1 character";
        public const string Max2 = "Maximum field size is 2 characters";
        public const string Max3 = "Maximum field size is 3 characters";
        public const string Max5 = "Maximum field size is 5 characters";
        public const string Max6 = "Maximum field size is 6 characters";
        public const string Max8 = "Maximum field size is 8 characters";
        public const string Max10 = "Maximum field size is 10 characters";
        public const string Max14 = "Maximum field size is 14 characters";
        public const string Max15 = "Maximum field size is 15 characters";
        public const string Max16 = "Maximum field size is 16 characters";
        public const string Max20 = "Maximum field size is 20 characters";
        public const string Max25 = "Maximum field size is 25 characters";
        public const string Max30 = "Maximum field size is 30 characters";
        public const string Max40 = "Maximum field size is 40 characters";
        public const string Max50 = "Maximum field size is 50 characters";
        public const string Max100 = "Maximum field size is 100 characters";
        public const string Max200 = "Maximum field size is 200 characters";
        public const string Max250 = "Maximum field size is 250 characters";
        public const string Max300 = "Maximum field size is 300 characters";
        public const string Max500 = "Maximum field size is 500 characters";
        public const string Max5000 = "Maximum field size is 5000 characters";

        //Date format for security token
        public const string SecurityTokenDateFormat = "MM/dd/yyyy HH:mm";

        public static string MonthAbbreviation(int monthId) {
            switch(monthId) {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
            }
            return "Unknown";
        }
    }
}