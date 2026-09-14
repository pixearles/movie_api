namespace WebClient.Common
{
    public enum SearchMode
    {
        Films,
        Actors
    }

    public class SelectedActor
    {
        public int Id {get;set;}
        public string Name {get;set;} = string.Empty;
    }

    public class SearchState
    {
        public SearchMode Mode {get;private set;} = SearchMode.Films;
        public string? SearchTerm {get;private set;}
        public List<int> SelectedGenreIds {get;} = [];
        public List<SelectedActor> SelectedActors {get;} = [];
        public int PageNumber {get;private set;} = 1;
        public int PageSize {get;private set;} = 20;

        public event Action? StateChanged;

        public void SetMode(SearchMode mode)
        {
            if (Mode == mode)
                return;

            Mode = mode;
            Reset();
        }

        public void SetSearchTerm(string? term)
        {
            SearchTerm = term;
            PageNumber = 1;
            NotifyChanged();
        }

        public void ToggleGenre(int genreId)
        {
            if (!SelectedGenreIds.Remove(genreId))
                SelectedGenreIds.Add(genreId);

            PageNumber = 1;
            NotifyChanged();
        }

        public void AddActorFilter(SelectedActor actor)
        {
            if (SelectedActors.Any(a => a.Id == actor.Id))
                return;

            SelectedActors.Add(actor);
            PageNumber = 1;
            NotifyChanged();
        }

        public void RemoveActorFilter(int actorId)
        {
            SelectedActors.RemoveAll(a => a.Id == actorId);
            PageNumber = 1;
            NotifyChanged();
        }

        public void SetPage(int pageNumber)
        {
            PageNumber = pageNumber;
            NotifyChanged();
        }

        public void SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            PageNumber = 1;
            NotifyChanged();
        }

        public void Reset()
        {
            SearchTerm = null;
            SelectedGenreIds.Clear();
            SelectedActors.Clear();
            PageNumber = 1;
            NotifyChanged();
        }

        public void FilterMoviesByActor(SelectedActor actor)
        {
            Mode = SearchMode.Films;
            SearchTerm = null;
            SelectedGenreIds.Clear();
            SelectedActors.Clear();
            SelectedActors.Add(actor);
            PageNumber = 1;
            NotifyChanged();
        }

        public void FilterMoviesByGenre(int genreId)
        {
            Mode = SearchMode.Films;
            SearchTerm = null;
            SelectedActors.Clear();
            SelectedGenreIds.Clear();
            SelectedGenreIds.Add(genreId);
            PageNumber = 1;
            NotifyChanged();
        }

        private void NotifyChanged() => StateChanged?.Invoke();
    }
}
