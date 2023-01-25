using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers;
using ocpa.ro.api.Models;
using System;
using System.Collections.Generic;

namespace ocpa.ro.api.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class TokenController : ApiControllerBase
    {
        private const int count = 5;
        private static readonly Random rnd = new Random();

        public TokenController(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, ITokenUtility tokenUtility) 
			: base(hostingEnvironment, authHelper, tokenUtility)
        {
        }

		[HttpGet("seed")]
		public IActionResult GetTokenSeed()
		{
			try
			{
				var reqAuthHeader = _authHelper.Authorize(Request);
				var seed = _tokenUtility.GetNewToken(reqAuthHeader).Seed;
				return Ok(seed);
			}
			catch (UnauthorizedAccessException uae)
			{
				return Unauthorized(uae.Message);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
