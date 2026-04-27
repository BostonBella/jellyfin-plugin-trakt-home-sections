using Jellyfin.Plugin.TraktHomeSections.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TraktHomeSections.Controllers
{
    /// <summary>
    /// API controller for Trakt authorization.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Administrator")]
    public class TraktController : ControllerBase
    {
        private readonly ILogger<TraktController> m_logger;
        private readonly TraktService m_traktService;

        public TraktController(
            ILogger<TraktController> logger,
            TraktService traktService)
        {
            m_logger = logger;
            m_traktService = traktService;
        }

        [HttpPost("Authorize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<object>> Authorize()
        {
            if (m_traktService.IsPolling)
            {
                return Conflict(new { error = "Authorization is already in progress. Please wait for the current attempt to complete or expire." });
            }

            try
            {
                var deviceCode = await m_traktService.AuthorizeDevice();
                if (deviceCode == null)
                {
                    return BadRequest(new { error = "Failed to get device code from Trakt. Check Trakt client ID." });
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var success = await m_traktService.PollForToken(deviceCode);
                        if (success)
                        {
                            m_logger.LogInformation("Trakt authorization completed successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        m_logger.LogError(ex, "Error while polling Trakt authorization.");
                    }
                });

                return Ok(new
                {
                    userCode = deviceCode.UserCode,
                    verificationUrl = deviceCode.VerificationUrl
                });
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error initiating Trakt authorization.");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("AuthorizationStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> GetAuthorizationStatus()
        {
            var config = Plugin.Instance.Configuration;

            var isAuthorized =
                !string.IsNullOrWhiteSpace(config.TraktAccessToken) &&
                !string.IsNullOrWhiteSpace(config.TraktRefreshToken);

            return Ok(new { isAuthorized, isPolling = m_traktService.IsPolling });
        }

        [HttpPost("Deauthorize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> Deauthorize()
        {
            var config = Plugin.Instance.Configuration;

            config.TraktAccessToken = string.Empty;
            config.TraktRefreshToken = string.Empty;
            config.TraktTokenExpiration = DateTime.MinValue;

            Plugin.Instance.SaveConfiguration();

            m_logger.LogInformation("Trakt account deauthorized.");

            return Ok(new { success = true });
        }
    }
}
