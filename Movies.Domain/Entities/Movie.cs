using System.Dynamic;

namespace Movies.Domain.Entities
{
    public class Movie
    {
        public int Id {get;set;}
        public string Title {get;set;}
        public DateTime ReleaseDate {get;set;}
        public string Overview {get;set;}
        public string PosterUrl {get;set;}
        public int VoteCount {get;set;}
        public decimal Popularity {get;set;}
        public decimal VoteAverage {get;set;}
        
        public ICollection<MovieGenre> MovieGenres {get;set;}
    }
}