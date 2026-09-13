namespace Movies.API.Features.Genres.GetAll.v1
{
    public partial class GetAllGenres
    {
        public class GenreSummary
        {
            public int Id {get;set;}
            public string Name {get;set;} = string.Empty;
        }
    }
}
