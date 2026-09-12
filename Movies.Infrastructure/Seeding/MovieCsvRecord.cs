using CsvHelper.Configuration.Attributes;

namespace Movies.Infrastructure.Seeding;

public class MovieCsvRecord
{
    [Name("Release_Date")]
    public DateTime ReleaseDate { get; set; }

    [Name("Title")]
    public string Title { get; set; } = string.Empty;

    [Name("Overview")]
    public string Overview { get; set; } = string.Empty;

    [Name("Popularity")]
    public decimal Popularity { get; set; }

    [Name("Vote_Count")]
    public int VoteCount { get; set; }

    [Name("Vote_Average")]
    public decimal VoteAverage { get; set; }

    [Name("Original_Language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    [Name("Genre")]
    public string Genre { get; set; } = string.Empty;

    [Name("Poster_Url")]
    public string PosterUrl { get; set; } = string.Empty;
}