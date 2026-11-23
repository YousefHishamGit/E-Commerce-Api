namespace Application.Exceptions
{
    /// <summary>An external dependency (e.g. the payment gateway) is not configured or failed. Maps to HTTP 503.</summary>
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message) : base(message) { }
    }
}
