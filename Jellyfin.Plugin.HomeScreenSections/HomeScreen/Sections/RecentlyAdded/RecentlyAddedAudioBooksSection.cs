using Jellyfin.Plugin.TraktHomeSections.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.TraktHomeSections.HomeScreen.Sections.RecentlyAdded
{
    public class RecentlyAddedAudioBooksSection : RecentlyAddedSectionBase
    {
        public override string? Section => "RecentlyAddedAudioBooks";

        public override string? DisplayText { get; set; } = "Recently Added Audiobooks";

        public override string? Route => "books";

        public override string? AdditionalData { get; set; } = "audiobooks";

        protected override BaseItemKind SectionItemKind => BaseItemKind.AudioBook;
        protected override CollectionType CollectionType => CollectionType.books;
        protected override CollectionTypeOptions CollectionTypeOptions => CollectionTypeOptions.books;
        protected override string? LibraryId => Plugin.Instance?.Configuration?.DefaultBooksLibraryId;
        protected override SectionViewMode DefaultViewMode => SectionViewMode.Portrait;

        public RecentlyAddedAudioBooksSection(IUserViewManager userViewManager,
            IUserManager userManager,
            ILibraryManager libraryManager,
            IDtoService dtoService,
            IServiceProvider serviceProvider) : base(userViewManager, userManager, libraryManager, dtoService, serviceProvider)
        {
        }
    }
}
