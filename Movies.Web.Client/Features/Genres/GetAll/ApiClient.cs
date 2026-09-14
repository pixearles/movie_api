using System.Net.Http.Json;

namespace WebClient.Features.Genres.GetAll
{
    public partial class GetAllGenres
    {
        public class ApiClient(HttpClient http)
        {
            public async Task<List<GenreSummary>> GetAllGenresAsync() =>
                await http.GetFromJsonAsync<List<GenreSummary>>("api/genres/get-all/v1") ?? [];
        }
    }
}
