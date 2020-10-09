using CourseLibrery.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrery.API.Models
{
    [CourseTitleMustBeDifferentFromDescriptionAttribute(ErrorMessage = "The description should be different from the title.")]

    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You sould fill out a title")]
        [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 character")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 character")]
        public virtual string Description { get; set; }
    }
}
