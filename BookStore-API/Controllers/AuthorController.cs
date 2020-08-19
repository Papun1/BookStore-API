using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{ /// <summary>
/// End point used to intract with the Author in the BookStore's Database
/// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public class AuthorController : ControllerBase
    {
        private readonly IAuthorRepository _AuthorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _Mapper;
        public AuthorController(IAuthorRepository AuthorRepository, ILoggerService logger,
           IMapper Mapper)
        {
            _AuthorRepository = AuthorRepository;
            _logger = logger;
            _Mapper = Mapper;
        }
        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get all the authors");
                var authors = await _AuthorRepository.FindAll();
                var respose = _Mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Successfully got all the authors");
                return Ok(respose);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");

            }

        }
        /// <summary>
        /// Get An author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Author's record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted Get the authors with id:{id}");
                var authors = await _AuthorRepository.FindById(id);
                if (authors == null)
                {
                    _logger.LogWarn($"Author with id:{id} was not found");
                    return NotFound();
                }
                var respose = _Mapper.Map<AuthorDTO>(authors);
                _logger.LogInfo($"Successfully got the author with id:{id}");
                return Ok(respose);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }

        }
        /// <summary>
        /// Create an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo("Attempted submission attempted");

                if (authorDTO == null)
                {
                    _logger.LogWarn("Empty request submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete");
                    return BadRequest(ModelState);
                }
                var author = _Mapper.Map<Author>(authorDTO);
                var isSuccess = await _AuthorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError("Author creation failed");
                }
                _logger.LogInfo("Author created");
                return Created("Create", new { author });
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }

        }
        /// <summary>
        /// Update author details
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author detail update with id:{id}");
                if (id < 1 || authorDTO == null || id !=authorDTO.Id)
                {
                    _logger.LogWarn("Author update failed with bad data");
                    return BadRequest();
                }
                var isExist = await _AuthorRepository.isExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Author with id:{id} was not found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete");
                    return BadRequest(ModelState);
                }
                var author = _Mapper.Map<Author>(authorDTO);
                var isSuccess = await _AuthorRepository.Update(author);
                if (!isSuccess)
                {
                    return InternalError("Author operation failed");
                }
                _logger.LogInfo("Author updated");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }
        }
        /// <summary>
        /// delete the author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Author detail delete with id:{id}");
                if (id < 1 )
                {
                    _logger.LogWarn("Author details not avaible");
                    return BadRequest();
                }
                var isExist = await _AuthorRepository.isExist(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Author with id:{id} was not found");
                    return NotFound();
                }
                var author = await _AuthorRepository.FindById(id);     
                var isSuccess = await _AuthorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError("Author delete failed");
                }
                _logger.LogInfo("Author delete");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message}-{ex.InnerException}");
            }
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong,Please contact the Adminstrator");
        }
    }
}
