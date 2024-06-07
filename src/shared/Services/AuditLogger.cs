

using Microsoft.Extensions.Logging;

namespace shared.Services
{
    public class AuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(ILogger<AuditLogger> logger)
        {
            _logger = logger;
        }

        public void LogAuditEvent(string userId, string action, string details)
        {
            _logger.LogInformation("Audit Event: User {UserId} performed {Action} - {Details}", userId, action, details);  
        }
    }
}