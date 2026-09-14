using Microsoft.AspNetCore.Components;

namespace WebClient.Features.Movies.Search.Components
{
    public partial class MovieTile
    {
        [Parameter, EditorRequired] public SearchMovies.MovieSummary Movie {get;set;} = null!;

        [Inject] private NavigationManager Navigation {get;set;} = null!;

        private void OnClicked() => Navigation.NavigateTo($"/movies/{Movie.Id}");
    }
}
