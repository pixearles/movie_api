namespace Movies.API.Features.Movies.Search.v1
{
    public partial class SearchMovies
    {
        public class Request
        {
            public string? SearchTerm {get;set;}
            public List<int>? Genres {get;set;}
            public List<int>? Actors {get;set;}
            public string? SortBy {get;set;}
            public bool SortByDescending {get;set;} = false;
            public int PageNumber {get;set;} = 1;
            public int PageSize {get;set;} = 20;
        }
    }
}