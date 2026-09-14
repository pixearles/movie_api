using System.Net.Http.Json;

namespace WebClient.Features.Movies.MovieDetails
{
    public partial class MovieDetails
    {
        public class ApiClient(HttpClient http)
        {
            public async Task<Response?> GetAsync(int id) =>
                await http.GetFromJsonAsync<Response>($"api/movies/get-movie-details/v1/{id}");
        }
    }
}
