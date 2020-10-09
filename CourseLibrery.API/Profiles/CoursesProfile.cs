using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrery.API.Models;

namespace CourseLibrery.API.Profiles
{
    public class CoursesProfile: Profile
    {
        public CoursesProfile()
        {
            CreateMap<Course, CourseDto>();

            CreateMap<CourseForCreationDto, Course>();
            CreateMap<CourseForUpdateDto, Course>();
        }
    }
}
