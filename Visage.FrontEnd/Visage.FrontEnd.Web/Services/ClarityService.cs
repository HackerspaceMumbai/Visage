namespace Visage.FrontEnd.Web.Services
{
    /// <summary>
    /// Implementation of Microsoft Clarity service for user behavior tracking
    /// </summary>
    public class ClarityService : IClarityService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the ClarityService class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public ClarityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the Clarity tracking ID from configuration
        /// </summary>
        /// <returns>The Clarity tracking ID</returns>
        public string GetTrackingId()
        {
            return _configuration["Clarity:TrackingId"] ?? string.Empty;
        }
    }
}