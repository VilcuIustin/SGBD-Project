﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGBD_Project.Dtos;
using SGBD_Project.Services;

namespace SGBD_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QueriesController : ControllerBase
    {
        private readonly QueriesService _queries;
        public QueriesController(QueriesService queries)
        {
            _queries = queries;
        }

        [HttpPost("save")]
        public async Task<ActionResult> SaveQuery(QueriesDto queriesDto)
        {
            try
            {
                return Ok(await _queries.SaveQuery(queriesDto, new Guid(User.Claims.FirstOrDefault(c => c.Type == "id").Value)));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return new ObjectResult("Something went wrong! Please try again later");
            }
        }


    }
}
