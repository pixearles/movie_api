namespace WebClient.Features.Actors.Search
{
    public partial class SearchActors
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
