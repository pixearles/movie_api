using Microsoft.AspNetCore.Components;
using WebClient.Common;
using MovieDetailsFeature = WebClient.Features.Movies.MovieDetails.MovieDetails;

namespace WebClient.Features.Movies.MovieDetails.Components
{
    public partial class MovieDetails
    {
        [Parameter] public int Id {get;set;}

        [Inject] private MovieDetailsFeature.ApiClient DetailsApiClient {get;set;} = null!;
        [Inject] private SearchState SearchState {get;set;} = null!;
        [Inject] private NavigationManager Navigation {get;set;} = null!;

        private MovieDetailsFeature.Response? _details;

        protected override async Task OnParametersSetAsync() =>
            _details = await DetailsApiClient.GetAsync(Id);

        private void OnGenreClicked(int genreId)
        {
            SearchState.FilterMoviesByGenre(genreId);
            Navigation.NavigateTo("/");
        }

        private void OnActorClicked(MovieDetailsFeature.ActorRef actor)
        {
            SearchState.FilterMoviesByActor(new SelectedActor { Id = actor.Id, Name = actor.Name });
            Navigation.NavigateTo("/");
        }
    }
}
