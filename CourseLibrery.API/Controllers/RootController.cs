﻿using System.Collections.Generic;
using CourseLibrery.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrery.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name ="GetRoot")]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(Url.Link("GetRoot", new { }),
                "self",
                "GET"));

            links.Add(new LinkDto(Url.Link("GetAuthors", new { }),
               "authors",
               "GET"));

            links.Add(new LinkDto(Url.Link("CreateAuthor", new { }),
                "create_author",
                "POST"));

            return Ok(links);
        }
    }
}
