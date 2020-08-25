using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Interact with Book table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _BookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _Mapper;
        public BooksController(IBookRepository BookRepository, ILoggerService logger,
           IMapper Mapper)
        {
            _BookRepository = BookRepository;
            _logger = logger;
            _Mapper = Mapper;
        }
        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>List of Books</returns>

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetCollectActionNames();
            try
            {
                _logger.LogInfo($"{location}:attempt call");
                var Books = await _BookRepository.FindAll();
                var respose = _Mapper.Map<IList<BookDTO>>(Books);
                _logger.LogInfo($"{location}:successful");
                return Ok(respose);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}-{ex.Message}-{ex.InnerException}");

            }

        }
        /// <summary>
        /// Get a Book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Book's record</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetCollectActionNames();
            try
            {
                _logger.LogInfo($"{location}:Attempted Get the Books with id:{id}");
                var Books = await _BookRepository.FindById(id);
                if (Books == null)
                {
                    _logger.LogWarn($"Book with id:{id} was not found");
                    return NotFound();
                }
                var respose = _Mapper.Map<BookDTO>(Books);
                _logger.LogInfo($"{location}:Successfully got the Book with id:{id}");
                return Ok(respose);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}-{ex.Message}-{ex.InnerException}");
            }

        }
        /// <summary>
        /// Create a Book
        /// </summary>
        /// <param name="BookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO BookDTO)
        {
            var location = GetCollectActionNames();
            try
            {
                _logger.LogInfo($"{location}:Attempted submission attempted");

                if (BookDTO == null)
                {
                    _logger.LogWarn($"{location}:Empty request submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}:Book data was incomplete");
                    return BadRequest(ModelState);
                }
                var book = _Mapper.Map<Book>(BookDTO);
                var isSuccess = await _BookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}:Book creation failed");
                }
                _logger.LogInfo($"{location}:Book created");
                return Created("Create", new { book });
            }
            catch (Exception ex)
            {
                return InternalError($"{location}-{ex.Message}-{ex.InnerException}");
            }

        }
        /// <summary>
        /// Update Book details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="BookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO BookDTO)
        {
            var location = GetCollectActionNames();
            try
            {
                _logger.LogInfo($"{location}:Book detail update with id:{id}");
                if (id < 1 || BookDTO == null || id != BookDTO.Id)
                {
                    _logger.LogWarn($"{location}:Book update failed with bad data");
                    return BadRequest();
                }
                var isExist = await _BookRepository.isExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}:Book with id:{id} was not found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}:Book data was incomplete");
                    return BadRequest(ModelState);
                }
                var Book = _Mapper.Map<Book>(BookDTO);
                var isSuccess = await _BookRepository.Update(Book);
                if (!isSuccess)
                {
                    return InternalError($"{location}:Book operation failed");
                }
                _logger.LogInfo($"{location}:Book updated");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}-{ex.Message}-{ex.InnerException}");
            }
        }
        /// <summary>
        /// delete the book by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Book detail delete with id:{id}");
                if (id < 1)
                {
                    _logger.LogWarn("Book details not avaible");
                    return BadRequest();
                }
                var isExist = await _BookRepository.isExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Book with id:{id} was not found");
                    return NotFound();
                }
                var Book = await _BookRepository.FindById(id);
                var isSuccess = await _BookRepository.Delete(Book);
                if (!isSuccess)
                {
                    return InternalError("Book delete failed");
                }
                _logger.LogInfo("Book delete");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }
        }
        private string GetCollectActionNames()
        {
            var controllerName = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controllerName}-{action}";

        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong,Please contact the Adminstrator");
        }
    }
}
