namespace Visage.FrontEnd.Web.Services
{
    /// <summary>
    /// Interface for Microsoft Clarity service to track user behavior
    /// </summary>
    public interface IClarityService
    {
        /// <summary>
        /// Gets the Clarity tracking ID from configuration
        /// </summary>
        string GetTrackingId();
    }
}