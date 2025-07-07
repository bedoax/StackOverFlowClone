namespace StackOverFlowClone.Models.Entities
{
    public class ErrorResponse
    {
        public Guid ErrorId { get; set; }
        public string Title { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public string Endpoint { get; set; }
        public string ExceptionType { get; set; }
        public string UserId { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string RequestPath { get; set; } 
        }

}
