namespace MoviesAPI.Dtos
{
    public class GenreDto
    {
        [MaxLength(100)]
        public String Name { get; set; }
    }
}
