using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WebClient.Common;

namespace WebClient.Layout
{
    public partial class MainLayout
    {
        [Inject] private SearchState SearchState {get;set;} = null!;
        [Inject] private NavigationManager Navigation {get;set;} = null!;

        private string? _searchTerm;

        private void OnModeChanged(SearchMode mode) => SearchState.SetMode(mode);

        private void OnSearchTermChanged(string? term)
        {
            _searchTerm = term;

            if (IsOnHomePage())
                SearchState.SetSearchTerm(term);
        }

        private void OnSearchKeyDown(KeyboardEventArgs e)
        {
            if (e.Key != "Enter")
                return;

            SearchState.SetSearchTerm(_searchTerm);

            if (!IsOnHomePage())
                Navigation.NavigateTo("/");
        }

        private bool IsOnHomePage() => new Uri(Navigation.Uri).AbsolutePath == "/";

        private void OnLogoClicked()
        {
            _searchTerm = null;
            SearchState.Reset();
            SearchState.SetMode(SearchMode.Films);
            Navigation.NavigateTo("/");
        }
    }
}
