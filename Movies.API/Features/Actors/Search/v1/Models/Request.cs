namespace Movies.API.Features.Actors.Search.v1
{
    public partial class Search
    {
        public class Request
        {
            public string? SearchTerm {get;set;}
            public bool SortByDescending {get;set;} = false;
            public int PageNumber {get;set;} = 1;
            public int PageSize {get;set;} = 20;
        }
    }
}
