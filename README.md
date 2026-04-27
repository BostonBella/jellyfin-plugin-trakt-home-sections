<h1 align="center">Trakt Home Sections</h1>
<h2 align="center">A Jellyfin Plugin</h2>
<p align="center">
	<img alt="Logo" src="https://raw.githubusercontent.com/BostonBella/jellyfin-plugin-trakt-home-sections/trakt-integration-tab/logo.png" />
</p>

## Introduction

Trakt Home Sections is a Jellyfin plugin that gives you a dynamic, fully customizable home screen with deep integration for [Trakt](https://trakt.tv), [Jellyseerr](https://github.com/Fallenbagel/jellyseerr), and the *arr ecosystem. Sections are organized by category and configurable from the admin dashboard.

It is a fork of [Home Screen Sections](https://github.com/IAmParadox27/jellyfin-plugin-home-sections) by IAmParadox27, extended with native Trakt integration and a redesigned admin interface.

> **Note:** This plugin is a community project and is not affiliated with Trakt or the Jellyfin project.

---

## Sections

The plugin replaces the default Jellyfin home screen with configurable sections organized into four categories. Each category has its own admin tab.

### Default Home

Standard Jellyfin sections, fully configurable:

- **My Media** - same as vanilla Jellyfin
- **Continue Watching** - resume where you left off
- **Next Up** - next unwatched episodes in your in-progress shows
- **Recently Added** - movies, shows, albums, artists, books, audiobooks, music videos
- **Latest** - latest movies, shows, albums, books, audiobooks, music videos
- **Because You Watched** - recommendations based on viewing history
- **Watch Again** - completed movies and shows surfaced for a rewatch, starting from the first in a collection or series
- **Genre** - a weighted random genre section drawn from your watch history
- **My List** - your personal saved list
- **Top Ten** - your top ten most watched content
- **Live TV** - same as vanilla Jellyfin

### Jellyseerr

Sections powered by your Jellyseerr instance, showing content not yet in your library with one-click requesting. **Requires Jellyseerr to be configured.**

- **Discover Movies**
- **Discover TV**
- **Trending**
- **My Requests** - all of your pending and fulfilled requests in one section

### Trakt

Sections powered by your linked Trakt account. **Requires both Trakt authorization and Jellyseerr to be configured.**

- **Watchlist** - movies and shows on your Trakt watchlist
- **Movie Recommendations** - personalized movie recommendations from Trakt
- **Show Recommendations** - personalized show recommendations from Trakt
- **Trending Movies** - what is trending on Trakt right now
- **Trending Shows** - trending shows on Trakt right now
- **Custom Lists** - add any public or private Trakt list as its own home screen section

### Upcoming

Sections pulled from your *arr calendars. **Requires arr API keys to be configured.**

- **Upcoming Movies** - from Radarr
- **Upcoming Shows** - from Sonarr
- **Upcoming Music** - from Lidarr
- **Upcoming Books** - from Readarr

---

## Installation

### Prerequisites

- Jellyfin **10.10.7** or **10.11.x**
- [Jellyseerr](https://github.com/Fallenbagel/jellyseerr) - required for Jellyseerr sections and all Trakt sections
- The following plugins must also be installed:
  - [File Transformation](https://github.com/IAmParadox27/jellyfin-plugin-file-transformation)
  - [Plugin Pages](https://github.com/IAmParadox27/jellyfin-plugin-pages)

### Installation via Plugin Repository

1. In the Jellyfin dashboard go to **Plugins -> Repositories**.
2. Add the following repository URL:
   ```
   https://raw.githubusercontent.com/BostonBella/jellyfin-plugin-trakt-home-sections/main/manifest.json
   ```
3. Go to **Plugins -> Catalogue** and install **Trakt Home Sections**.
4. Restart Jellyfin.

### First-Time Setup

1. From the Jellyfin dashboard go to **Plugins -> Trakt Home Sections** to open the admin config.
2. Use the tabs to configure each section category:
   - **Default Home** - enable and order your standard Jellyfin sections
   - **Jellyseerr** - enter your Jellyseerr URL and API key, then enable the sections you want
   - **Trakt** - enter your Trakt Client ID and Secret first, then link your account and enable the sections you want; add custom Trakt lists here
   - **Upcoming** - enter your *arr URLs and API keys
   - **Advanced** - additional plugin settings
3. On the Jellyfin home screen open the hamburger menu and click **Home Sections**.
4. Enable the sections you want and save.
5. Force-refresh your browser (Ctrl+Shift+R / Cmd+Shift+R) to see your new home screen.

---

## Trakt Authorization

Before linking your Trakt account you must first create a Trakt API application and enter your Client ID and Secret in the **Trakt Configuration** section of the admin config.

1. Go to [trakt.tv/oauth/applications](https://trakt.tv/oauth/applications) and create a new application.
2. Copy the Client ID and Secret into the **Trakt Configuration** section in the plugin admin config and save.
3. Go to the **Trakt Authorization** section and click **Link Trakt Account**.
4. Follow the device authorization flow on trakt.tv.
5. Once authorized your Trakt sections will become active.

To find the ID for a custom Trakt list: go to the list on trakt.tv, click the **Copy Link** icon, and paste the URL. The numeric ID is at the end of that URL.

---

## Troubleshooting

**No sections or changes after install**

Clear your browser cache and cookies, then restart Jellyfin. Jellyfin applies plugin updates on restart and cached files may cause the old home screen to appear.

**Jellyseerr sections are not appearing**

Make sure your Jellyseerr URL and API key are entered in the Jellyseerr tab of the admin config. Without these, Jellyseerr sections will not display any content.

**Trakt sections are not appearing**

Trakt sections require all of the following to work:

- A Trakt API application with Client ID and Secret entered in the Trakt Configuration section
- Your Trakt account linked via the Trakt Authorization section
- Jellyseerr configured with a valid URL and API key

Trakt uses Jellyseerr to fetch card images and metadata. If any of these are missing the sections will appear empty.

**Home Sections menu not appearing in the hamburger menu**

If the admin has disabled user override, the Home Sections option will not appear in the hamburger menu. Contact your Jellyfin administrator.

**Trakt sections only show portrait cards**

View mode selection for Trakt sections is not yet available. Support for portrait, landscape, and square card modes is planned for a future release.

---

## Credits

This plugin is built on top of [Home Screen Sections](https://github.com/IAmParadox27/jellyfin-plugin-home-sections) by [IAmParadox27](https://github.com/IAmParadox27), licensed under GPL 3.0. All original sections and plugin architecture originate from that project.

---

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
