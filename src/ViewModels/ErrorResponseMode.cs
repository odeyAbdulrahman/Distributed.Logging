using Newtonsoft.Json;

namespace Distributed.Logging.ViewModels
{

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ErrorResponseMode
    {

        public string? TraceId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Message { get; set; }
    }   
}
