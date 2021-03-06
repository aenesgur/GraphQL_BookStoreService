﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.API.JWT;
using BookStore.Entities.Models;
using BookStore.Identity.Services.Abstracts;
using BookStore.Log.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IUserService _userService;
        private IConfiguration _configuration;
        private ILogService _logger;
        public AuthenticationController(IUserService userService, IConfiguration configuration, ILogService logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInfo("Home page is shown");
            var entryText = "Hello, welcome to my GraphQL sample app. To get token, firstly register after that login to server :))";
            return Ok(entryText);
        }

        // /api/authentication/register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var registerResult = await _userService.RegisterAsync(model);
                    if (registerResult.IsSuccess)
                    {
                        return Ok(registerResult); 
                    }
                    
                    _logger.LogError(registerResult.Errors.First());
                    return BadRequest(registerResult);
                }

                return BadRequest("Some properties are not valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
            
        }

        // /api/authentication/login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loginResult = await _userService.LoginAsync(model);
                    if (!loginResult.IsSuccess)
                    {
                        _logger.LogError(loginResult.Errors.First());
                        return BadRequest(loginResult);
                    }
                        

                    var jwtHelper = new JWTHelper(_configuration);
                    var tokenResult = jwtHelper.CreateToken(loginResult.Email, loginResult.UserId);

                    if (!tokenResult.IsSuccess)
                    {
                        _logger.LogError(tokenResult.Errors.First());
                        return BadRequest(tokenResult);
                    }

                    return Ok(tokenResult);
                }

                return BadRequest("Some properties are not valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
            
        }
    }
}