namespace Jellyfin.Plugin.HomeScreenSections.Model.Trakt
{
    /// <summary>
    /// Represents Trakt item IDs.
    /// </summary>
    public class TraktIds
    {
        public int Trakt { get; set; }

        public int? Tmdb { get; set; }

        public string? Imdb { get; set; }

        public int? Tvdb { get; set; }
    }

    /// <summary>
    /// Represents a Trakt movie.
    /// </summary>
    public class TraktMovie
    {
        public string Title { get; set; } = string.Empty;

        public int? Year { get; set; }

        public TraktIds Ids { get; set; } = new TraktIds();

        public string[]? Genres { get; set; }
    }

    /// <summary>
    /// Represents a Trakt show.
    /// </summary>
    public class TraktShow
    {
        public string Title { get; set; } = string.Empty;

        public int? Year { get; set; }

        public TraktIds Ids { get; set; } = new TraktIds();

        public string[]? Genres { get; set; }

        public string? Status { get; set; }
    }

    /// <summary>
    /// Represents an item in a Trakt list.
    /// </summary>
    public class TraktListItem
    {
        public string Type { get; set; } = string.Empty;

        public TraktMovie? Movie { get; set; }

        public TraktShow? Show { get; set; }
    }

    /// <summary>
    /// Represents a Trakt watchlist movie item.
    /// </summary>
    public class TraktWatchlistMovieItem
    {
        public TraktMovie Movie { get; set; } = new TraktMovie();
    }

    /// <summary>
    /// Represents a Trakt watchlist show item.
    /// </summary>
    public class TraktWatchlistShowItem
    {
        public TraktShow Show { get; set; } = new TraktShow();
    }

    /// <summary>
    /// Represents a Trakt trending movie item.
    /// </summary>
    public class TraktTrendingMovieItem
    {
        public TraktMovie Movie { get; set; } = new TraktMovie();
    }

    /// <summary>
    /// Represents a Trakt trending show item.
    /// </summary>
    public class TraktTrendingShowItem
    {
        public TraktShow Show { get; set; } = new TraktShow();
    }

    /// <summary>
    /// Represents a Trakt device code response.
    /// </summary>
    public class TraktDeviceCode
    {
        public string DeviceCode { get; set; } = string.Empty;

        public string UserCode { get; set; } = string.Empty;

        public string VerificationUrl { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public int Interval { get; set; }
    }

    /// <summary>
    /// Represents a Trakt access token response.
    /// </summary>
    public class TraktAccessToken
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }
    }
}