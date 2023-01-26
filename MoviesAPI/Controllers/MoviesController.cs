using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
     

        private new List<String> _allowedExtentions = new List<String> { ".jpg", ".png"};

        private long _maxAllowedPosterSize = 1048576;

        private readonly IMoviesService _moviesService;

        private readonly IGenresService _genreService;

        private readonly IMapper _mapper;
        public MoviesController(IMoviesService moviesService, IGenresService genreService, IMapper mapper)
        {
            _moviesService = moviesService;
            _genreService = genreService;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            
            var movies = await _moviesService.GetAll();
            
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
          
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if(movie == null)
            {
                return NotFound("This Id isn't Available");
            }

            var dto = _mapper.Map<MovieDetailsDto>(movie);
            return Ok(dto);
        }
        
        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _moviesService.GetAll(genreId);

            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);

            return Ok(data);


        }

        
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm]MovieDto dto)
        {
            if(dto.Poster == null)
            {
                return BadRequest("Poster is Required");
            }

            if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only png and jpg allowed");


            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed Size is 1mb");


            var isValidGenre = await _genreService.IsValidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre ID");

            using var datastream = new MemoryStream();

            await dto.Poster.CopyToAsync(datastream);
            var movie = _mapper.Map<Movie>(dto);
            movie.Poster = datastream.ToArray();

            await _moviesService.Add(movie);

            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,[FromForm] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null) return BadRequest("No movies with this id");

            var isValidGenre = await _genreService.IsValidGenre(dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre ID");

            if(dto.Poster!= null)
            {
                if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only png and jpg allowed");


                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed Size is 1mb");

                using var datastream = new MemoryStream();

                await dto.Poster.CopyToAsync(datastream);

                movie.Poster = datastream.ToArray();

            }

            movie.Title = dto.Title;
            movie.Rate = dto.Rate;
            movie.Storeline = dto.Storeline;
            movie.Year = dto.Year;
            movie.GenreId= dto.GenreId;

            _moviesService.Update(movie);
            return Ok(movie);

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie= await _moviesService.GetById(id);
            if (movie == null) return BadRequest("No Movie With this id");

            _moviesService.Delete(movie);

            return Ok(movie);

        }   
    }
}
