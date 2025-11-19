using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Buyly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message = "Request successful")
        {
            return Ok(ApiResponse<T>.SucessResponse(data, message));
        }

        protected IActionResult Success(string message = "Request successful")
        {
            return Ok(ApiResponse.Successful(message));
        }

        protected IActionResult Created<T>(T data, string message = "Resource created successfully")
        {
            return StatusCode(201, ApiResponse<T>.SucessResponse(data, message));
        }

        protected IActionResult Error(string message, string[]? errors = null)
        {
            return BadRequest(ApiResponse.Error(message, errors));
        }

        protected IActionResult NotFound(string message = "Resource not found")
        {
            return base.NotFound(ApiResponse.Error(message));
        }
    
        protected IActionResult Unauthorized(string message = "Unauthorized access")
        {
            return Unauthorized(ApiResponse.Error(message));
        }
        
    }
}