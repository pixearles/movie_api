namespace Movies.API.Features.Movies.GetMovieDetails.v1
{
    public partial class GetMovieDetails
    {
        public class Response
        {
            public int Id {get;set;}
            public string Title {get;set;} = string.Empty;
            public DateTime ReleaseDate {get;set;}
            public string Overview {get;set;} = string.Empty;
            public string PosterUrl {get;set;} = string.Empty;
            public decimal AverageVote {get;set;}
            public decimal Popularity {get;set;}
            public string OriginalLanguage {get;set;} = string.Empty;
            public List<GenreRef> Genres {get;set;} = [];
            public List<ActorRef> Actors {get;set;} = [];
        }

        public class GenreRef
        {
            public int Id {get;set;}
            public string Name {get;set;} = string.Empty;
        }

        public class ActorRef
        {
            public int Id {get;set;}
            public string Name {get;set;} = string.Empty;
        }
    }
}
