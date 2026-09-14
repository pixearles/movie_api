namespace WebClient.Features.Movies.Search
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
            public string Title {get;set;} = string.Empty;
            public DateTime ReleaseDate {get;set;}
            public string PosterUrl {get;set;} = string.Empty;
            public decimal VoteAverage {get;set;}
            public string OriginalLanguage {get;set;} = string.Empty;
            public List<string> Genres {get;set;} = [];
        }
    }
}
