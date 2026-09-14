using System.Net.Http.Json;

namespace WebClient.Features.Actors.Search
{
    public partial class SearchActors
    {
        public class ApiClient(HttpClient http)
        {
            public async Task<Response?> SearchActorsAsync(Request request) =>
                await http.GetFromJsonAsync<Response>($"api/actors/search/v1{BuildQueryString(request)}");

            private static string BuildQueryString(Request request)
            {
                var parameters = new List<string>();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    parameters.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

                parameters.Add($"sortByDescending={request.SortByDescending}");
                parameters.Add($"pageNumber={request.PageNumber}");
                parameters.Add($"pageSize={request.PageSize}");

                return $"?{string.Join("&", parameters)}";
            }
        }
    }
}
