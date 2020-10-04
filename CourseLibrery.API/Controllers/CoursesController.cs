using AutoMapper;
using CourseLibrary.API.Services;
using CourseLibrery.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CourseLibrery.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        public CoursesController(ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper)
        {

            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentException(nameof(mapper));
        }

        [HttpGet()]
        public ActionResult<IEquatable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (! _courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var coursesForAuhthorFromRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuhthorFromRepo));
        }

        [HttpGet("{courseId:guid}")]
        public ActionResult<IEquatable<CourseDto>> GetCoursesByAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var coursesForAuhthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (coursesForAuhthorFromRepo == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CourseDto>(coursesForAuhthorFromRepo));
        }
    }
}
