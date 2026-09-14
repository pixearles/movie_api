using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using WebClient.Common;
using WebClient.Features.Actors.Search;
using WebClient.Features.Genres.GetAll;
using WebClient.Features.Movies.MovieDetails;
using WebClient.Features.Movies.Search;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddSingleton<SearchState>();

var apiBaseUri = new Uri(builder.Configuration["ApiBaseUrl"]!);

// feature slice registration
// Named explicitly because AddHttpClient<TClient>() keys its typed client by TClient's short
// name (ignoring namespace), and every slice's client class is named plainly "ApiClient".
builder.Services.AddHttpClient<SearchMovies.ApiClient>("SearchMovies.ApiClient", c => c.BaseAddress = apiBaseUri);
builder.Services.AddHttpClient<SearchActors.ApiClient>("SearchActors.ApiClient", c => c.BaseAddress = apiBaseUri);
builder.Services.AddHttpClient<GetAllGenres.ApiClient>("GetAllGenres.ApiClient", c => c.BaseAddress = apiBaseUri);
builder.Services.AddHttpClient<MovieDetails.ApiClient>("MovieDetails.ApiClient", c => c.BaseAddress = apiBaseUri);

await builder.Build().RunAsync();
