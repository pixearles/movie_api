using Microsoft.AspNetCore.Components;
using WebClient.Common;
using WebClient.Features.Actors.Search;
using WebClient.Features.Movies.Search;

namespace WebClient.Pages
{
    public partial class Home : IDisposable
    {
        [Inject] private SearchState SearchState {get;set;} = null!;
        [Inject] private SearchMovies.ApiClient MoviesApiClient {get;set;} = null!;
        [Inject] private SearchActors.ApiClient ActorsApiClient {get;set;} = null!;

        private SearchMovies.Response? _movieResponse;
        private SearchActors.Response? _actorResponse;
        private string? _loadError;

        protected override async Task OnInitializedAsync()
        {
            SearchState.StateChanged += OnSearchStateChanged;
            await LoadResultsAsync();
        }

        private async void OnSearchStateChanged()
        {
            StateHasChanged();
            await LoadResultsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadResultsAsync()
        {
            try
            {
                if (SearchState.Mode == SearchMode.Films)
                {
                    _movieResponse = await MoviesApiClient.SearchMoviesAsync(new SearchMovies.Request
                    {
                        SearchTerm = SearchState.SearchTerm,
                        Genres = SearchState.SelectedGenreIds.Count > 0 ? [.. SearchState.SelectedGenreIds] : null,
                        Actors = SearchState.SelectedActors.Count > 0 ? SearchState.SelectedActors.Select(a => a.Id).ToList() : null,
                        PageNumber = SearchState.PageNumber,
                        PageSize = SearchState.PageSize
                    });
                }
                else
                {
                    _actorResponse = await ActorsApiClient.SearchActorsAsync(new SearchActors.Request
                    {
                        SearchTerm = SearchState.SearchTerm,
                        PageNumber = SearchState.PageNumber,
                        PageSize = SearchState.PageSize
                    });
                }

                _loadError = null;
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
            }
        }

        private void OnPageNumberChanged(int page) => SearchState.SetPage(page);

        private void OnPageSizeChanged(int size) => SearchState.SetPageSize(size);

        private void OnActorSelected(SearchActors.ActorSummary actor) =>
            SearchState.FilterMoviesByActor(new SelectedActor { Id = actor.Id, Name = actor.Name });

        public void Dispose() => SearchState.StateChanged -= OnSearchStateChanged;
    }
}
