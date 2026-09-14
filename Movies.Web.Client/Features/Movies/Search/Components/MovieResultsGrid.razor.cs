using Microsoft.AspNetCore.Components;

namespace WebClient.Features.Movies.Search.Components
{
    public partial class MovieResultsGrid
    {
        [Parameter, EditorRequired] public SearchMovies.Response? Response {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageNumberChanged {get;set;}
        [Parameter, EditorRequired] public EventCallback<int> PageSizeChanged {get;set;}
    }
}
