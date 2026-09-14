using System.Net.Http.Json;

namespace WebClient.Features.Movies.Search
{
    public partial class SearchMovies
    {
        public class ApiClient(HttpClient http)
        {
            public async Task<Response?> SearchMoviesAsync(Request request) =>
                await http.GetFromJsonAsync<Response>($"api/movies/search/v1{BuildQueryString(request)}");

            private static string BuildQueryString(Request request)
            {
                var parameters = new List<string>();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    parameters.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

                if (request.Genres is { Count: > 0 })
                    parameters.AddRange(request.Genres.Select(id => $"genres={id}"));

                if (request.Actors is { Count: > 0 })
                    parameters.AddRange(request.Actors.Select(id => $"actors={id}"));

                if (!string.IsNullOrWhiteSpace(request.SortBy))
                    parameters.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

                parameters.Add($"sortByDescending={request.SortByDescending}");
                parameters.Add($"pageNumber={request.PageNumber}");
                parameters.Add($"pageSize={request.PageSize}");

                return $"?{string.Join("&", parameters)}";
            }
        }
    }
}
