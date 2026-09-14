using Microsoft.AspNetCore.Components;
using WebClient.Common;
using WebClient.Features.Actors.Search;
using WebClient.Features.Genres.GetAll;

namespace WebClient.Features.Movies.Search.Components
{
    public partial class FilterSidebar : IDisposable
    {
        [Inject] private GetAllGenres.ApiClient GenresApiClient {get;set;} = null!;
        [Inject] private SearchActors.ApiClient ActorsApiClient {get;set;} = null!;
        [Inject] private SearchState SearchState {get;set;} = null!;

        private List<GetAllGenres.GenreSummary>? _genres;
        private SearchActors.ActorSummary? _actorSearchValue;

        protected override async Task OnInitializedAsync()
        {
            SearchState.StateChanged += OnSearchStateChanged;
            _genres = await GenresApiClient.GetAllGenresAsync();
        }

        private void OnSearchStateChanged() => InvokeAsync(StateHasChanged);

        private void OnGenreToggled(int genreId) => SearchState.ToggleGenre(genreId);

        private async Task<IEnumerable<SearchActors.ActorSummary>> SearchActorsAsync(string searchTerm, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return [];

            var response = await ActorsApiClient.SearchActorsAsync(new SearchActors.Request { SearchTerm = searchTerm, PageSize = 10 });
            return response?.Actors ?? [];
        }

        private void OnActorSelected(SearchActors.ActorSummary? actor)
        {
            _actorSearchValue = null;

            if (actor is null)
                return;

            SearchState.AddActorFilter(new SelectedActor { Id = actor.Id, Name = actor.Name });
        }

        private void OnActorFilterRemoved(int actorId) => SearchState.RemoveActorFilter(actorId);

        public void Dispose() => SearchState.StateChanged -= OnSearchStateChanged;
    }
}
