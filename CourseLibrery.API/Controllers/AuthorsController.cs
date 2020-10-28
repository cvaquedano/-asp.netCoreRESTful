using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Services;
using CourseLibrery.API.Helpers;
using CourseLibrery.API.Models;
using CourseLibrery.API.ResourceParameters;
using CourseLibrery.API.Services;
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
        private readonly IPropertyMappingService _propertyMappingService;

        public IPropertyCheckerService _propertyCheckerService { get; }

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository ?? 
                throw new ArgumentException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentException(nameof(mapper));
           _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors( [FromQuery] AuthorResourceParameters authorResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistisFor<AuthorDto,Author>(authorResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorResourceParameters.Fields))
            {
                return BadRequest();
            }
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorResourceParameters);

         
            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
           
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var links = CreateLinksForAuthors(authorResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

            var shapeAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(authorResourceParameters.Fields);

            var shapedAuthorsWithLinks = shapeAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;

            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };


            return Ok(linkedCollectionResource);
        }


        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
      
        public IActionResult GetAuthor(Guid authorId,string fields)
        {
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            var links = CreateLinksForAuthor(authorId, fields);

            var linkedResourceToReturn =
                _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor( AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            var links = CreateLinksForAuthor(authorToReturn.Id, null);

            var linkedResourceToReturn =
                authorToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAuthor", 
                new { authorId = linkedResourceToReturn["Id"] }, 
                linkedResourceToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}",Name = "DeleteAuthor")]

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
                            fields = authorResourceParameters.Fields,
                            orderBy = authorResourceParameters.OrderBy,                            
                            pageNumber = authorResourceParameters.PageNumber - 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.MainCategory,
                            searchQuery = authorResourceParameters.SearchQuery

                        });
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = authorResourceParameters.Fields,
                            orderBy = authorResourceParameters.OrderBy,
                            pageNumber = authorResourceParameters.PageNumber + 1,
                            pageSize = authorResourceParameters.PageSize,
                            mainCategory = authorResourceParameters.MainCategory,
                            searchQuery = authorResourceParameters.SearchQuery

                        });

                case ResourceUriType.Current:
                default:
                    return Url.Link("GetAuthors",
                       new
                       {
                           fields = authorResourceParameters.Fields,
                           orderBy = authorResourceParameters.OrderBy,
                           pageNumber = authorResourceParameters.PageNumber,
                           pageSize = authorResourceParameters.PageSize,
                           mainCategory = authorResourceParameters.MainCategory,
                           searchQuery = authorResourceParameters.SearchQuery

                       });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(Url.Link("GetAuthor", new { authorId }), "self", "GET"));
            }
            else
            {
                links.Add(new LinkDto(Url.Link("GetAuthor", new { authorId, fields }), "self", "GET"));
            }

            links.Add(new LinkDto(Url.Link("DeleteAuthor", new { authorId }), "delete_author", "DELETE"));

            links.Add(new LinkDto(Url.Link("CreateCourseForAuthor", new { authorId }), "create_course_for_author", "POST"));

            links.Add(new LinkDto(Url.Link("GetCoursesForAuthor", new { authorId }), "courses", "GET"));

            return links;

        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorResourceParameters authorResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.Current),
                "self",
                "GET"));

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.PreviousPage),
                "previousPage",
                "GET"));
            }
            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.NextPage),
                "nextPage",
                "GET"));
            }


            return links;
        }
    }
}
