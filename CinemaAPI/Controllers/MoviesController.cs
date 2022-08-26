using CinemaApi.Data;
using CinemaApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private CinemaDbContext _dbContext;

        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /api/movies/AllMovies?sort=desc&pageNumber=1&pageSize=10
        //[Authorize]
        [HttpGet("[action]")]
        public IActionResult AllMovies(string sort, int? pageNumber, int? pageSize)
        {
            var currentPageNumber = pageNumber ?? 1;
            var currentPageSize = pageSize ?? 5;

            var movies = from movie in _dbContext.Movies
                         select new
                         {
                             movie.Id,
                             movie.Name,
                             movie.Duration,
                             movie.Language,
                             movie.Rating,
                             movie.Genre,
                             movie.ImageUrl
                         };

            switch (sort)
            {
                case "desc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderByDescending(m => m.Rating));

                case "asc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderBy(m => m.Rating));

                default:
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }

        }

        // GET api/<MoviesController>/5
        //[Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult MovieDetail(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
                return NotFound("Records not found");

            return Ok(movie);
        }

        // GET api/movies/FindMovies?movieName=Frozen
        [HttpGet("[action]")]
        public IActionResult FindMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.ToLower().StartsWith(movieName.ToLower())

                         select new
                         {
                             movie.Id,
                             movie.Name,
                             movie.ImageUrl
                         };

            return Ok(movies);
        }


        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movieObj)
        {
            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");

            if (movieObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movieObj.Image.CopyTo(fileStream);
            }

            movieObj.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movieObj);
            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        // PUT api/<MoviesController>/5
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movieObj)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
                return NotFound("Records not found");

            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");

            if (movieObj.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movieObj.Image.CopyTo(fileStream);
                movie.ImageUrl = filePath.Remove(0, 7);
            }

            movie.Name = movieObj.Name;
            movie.Language = movieObj.Language;
            movie.Description = movieObj.Description;
            movie.Language = movieObj.Language;
            movie.Duration = movieObj.Duration;
            movie.PlayingDate = movieObj.PlayingDate;
            movie.PlayingTime = movieObj.PlayingTime;
            movie.Rating = movieObj.Rating;
            movie.Genre = movieObj.Genre;
            movie.TrailorUrl = movieObj.TrailorUrl;
            movie.TicketPrice = movieObj.TicketPrice;

            _dbContext.SaveChanges();
            return Ok("Record Updated successfully");
        }

        // DELETE api/<MoviesController>/5
        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
                return NotFound("Records not found");

            _dbContext.Movies.Remove(movie);
            _dbContext.SaveChanges();
            return Ok("Record deleted successfully");
        }
    }
}
