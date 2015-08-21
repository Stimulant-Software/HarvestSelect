using System.Net.Http.Formatting;
using InnovaServiceHost.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InnovaServiceHost.App_Start {
    public class FormatterConfig {
        public static void RegisterFormatters(MediaTypeFormatterCollection formatters) {
            formatters.Remove(formatters.JsonFormatter);
            formatters.Insert(0, new JsonpMediaTypeFormatter {
                SerializerSettings = new JsonSerializerSettings {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            });
        }
    }
}