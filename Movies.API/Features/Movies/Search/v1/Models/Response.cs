namespace Movies.API.Features.Movies.Search.v1
{
    public partial class SearchMovies
    {
        public class Response
        {
            public List<MovieSummary> Movies {get;set;} = [];
            public int TotalCount {get;set;}
            public int PageNumber {get;set;}
            public int PageSize {get;set;}
            public int TotalPages {get;set;}
        }

        public class MovieSummary
        {
            public int Id {get;set;}
            public string Title {get;set;} = String.Empty;
            public DateTime ReleaseDate {get;set;}
            public string PosterUrl {get;set;} = String.Empty;
            public decimal VoteAverage {get;set;}
            public List<string> Genres {get;set;} = [];
        }
    }
}