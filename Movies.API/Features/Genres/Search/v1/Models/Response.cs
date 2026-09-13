namespace Movies.API.Features.Genres.Search.v1
{
    public partial class SearchGenres
    {
        public class Response
        {
            public List<GenreDetails> Genres {get;set;} = [];
            public int TotalCount {get;set;}
            public int PageNumber {get;set;}
            public int PageSize {get;set;}
            public int TotalPages {get;set;}
        }

        public class GenreDetails
        {
            public int Id {get;set;}
            public string Name {get;set;} = string.Empty;
        }
    }
}
