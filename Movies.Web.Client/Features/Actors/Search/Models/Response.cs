namespace WebClient.Features.Actors.Search
{
    public partial class SearchActors
    {
        public class Response
        {
            public List<ActorSummary> Actors {get;set;} = [];
            public int TotalCount {get;set;}
            public int PageNumber {get;set;}
            public int PageSize {get;set;}
            public int TotalPages {get;set;}
        }

        public class ActorSummary
        {
            public int Id {get;set;}
            public string Name {get;set;} = string.Empty;
        }
    }
}
