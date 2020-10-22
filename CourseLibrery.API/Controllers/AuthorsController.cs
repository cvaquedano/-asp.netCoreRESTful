using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using CourseLibrery.API.Helpers;
using CourseLibrery.API.Models;
using CourseLibrery.API.ResourceParameters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CourseLibrery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        public AuthorsController(ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ?? 
                throw new ArgumentException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentException(nameof(mapper));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public ActionResult<IEnumerable<AuthorDto>> GetAuthors( [FromQuery] AuthorResourceParameters authorResourceParameters)
        {
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));


            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }


        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
        [HttpHead]
        public ActionResult<AuthorDto> GetAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor( AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { authorId = authorToReturn.Id }, authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]

        public ActionResult DeleteAuthor(Guid authorId)
        {
            

            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);
            if (authorFromRepo == null)
            {
                return NotFound();

            }

            _courseLibraryRepository.DeleteAuthor(authorFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();

        }

        private string CreateAuthorsResourceUri(AuthorResourceParameters authorResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorResourceParameters.PageNumber - 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.MainCategory,
                            searchQuery = authorResourceParameters.SearchQuery

                        });
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            pageNumber = authorResourceParameters.PageNumber + 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.MainCategory,
                            searchQuery = authorResourceParameters.SearchQuery

                        });
                default:
                    return Url.Link("GetAuthors",
                       new
                       {
                           pageNumber = authorResourceParameters.PageNumber,
                           pageSize = authorResourceParameters.PageSize,
                           mainCategory = authorResourceParameters.MainCategory,
                           searchQuery = authorResourceParameters.SearchQuery

                       });
            }
        }
    }
}
