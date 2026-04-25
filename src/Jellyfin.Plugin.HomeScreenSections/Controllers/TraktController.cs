using Jellyfin.Plugin.HomeScreenSections.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.Controllers
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
        private readonly ILogger<TraktController> _logger;
        private readonly TraktService _traktService;

        public TraktController(
            ILogger<TraktController> logger,
            TraktService traktService)
        {
            _logger = logger;
            _traktService = traktService;
        }

        [HttpPost("Authorize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> Authorize()
        {
            try
            {
                var deviceCode = await _traktService.AuthorizeDevice();
                if (deviceCode == null)
                {
                    return BadRequest(new { error = "Failed to get device code from Trakt. Check Trakt client ID." });
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var success = await _traktService.PollForToken(deviceCode);
                        if (success)
                        {
                            _logger.LogInformation("Trakt authorization completed successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while polling Trakt authorization.");
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
                _logger.LogError(ex, "Error initiating Trakt authorization.");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("AuthorizationStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> GetAuthorizationStatus()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;

            var isAuthorized =
                !string.IsNullOrWhiteSpace(config.TraktAccessToken) &&
                !string.IsNullOrWhiteSpace(config.TraktRefreshToken);

            return Ok(new { isAuthorized });
        }

        [HttpPost("Deauthorize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> Deauthorize()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;

            config.TraktAccessToken = string.Empty;
            config.TraktRefreshToken = string.Empty;
            config.TraktTokenExpiration = DateTime.MinValue;

            HomeScreenSectionsPlugin.Instance.SaveConfiguration();

            _logger.LogInformation("Trakt account deauthorized.");

            return Ok(new { success = true });
        }
    }
}