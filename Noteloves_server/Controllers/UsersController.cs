﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noteloves_server.Data;
using Noteloves_server.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;
using Noteloves_server.Messages.Requests;
using Noteloves_server.Services;
using Noteloves_server.Messages.Responses;
using Noteloves_server.JWTProvider.Services;

namespace Noteloves_server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private IUserService _userService;
        private IJWTService _jWTService;

        public UsersController(DatabaseContext context, IUserService userService, IJWTService jWTService)
        {
            _context = context;
            _userService = userService;
            _jWTService = jWTService;
        }

        // GET: api/Users/ListUser
        [HttpGet]
        [Route("ListUser")]
        public IActionResult GetListUsers()
        {
            return Ok(new DataResponse("200", _userService.GetAllUser(), "Successfully!"));
        }

        // GET: api/Users
        [HttpGet]
        public IActionResult GetUserByToken()
        {
            var authorization = Request.Headers["Authorization"];
            var accessToken = authorization.ToString().Replace("Bearer ", "");
            var id = _jWTService.GetIdByToken(accessToken);

            var user = _userService.GetInfomation(id);

            if (user == null)
            {
                return NotFound(new Response("404", "User not found!"));
            }

            user.RefreshToken = null;
            user.Password = null;

            return Ok(new DataResponse("200", user, "Successfully!"));
        }

        // PUT: api/Users/EditInfo
        [HttpPut]
        [Route("EditInfo")]
        public  IActionResult EditInformationUserByToken([FromBody] EditUserForm editUserForm)
        {
            var authorization = Request.Headers["Authorization"];
            var accessToken = authorization.ToString().Replace("Bearer ", "");
            var id = _jWTService.GetIdByToken(accessToken);

            if (!_userService.UserExistsById(id))
            {
                return NotFound(new Response("404", "User not found!"));
            }

            _userService.EidtInfomation(id, editUserForm);

            return Ok(new Response("200", "Successfully!"));
        }

        // PUT: api/Users/EditNameUser
        [HttpPatch]
        [Route("EditNameUser")]
        public IActionResult EditUserNameByToken([FromForm] string newName)
        {
            if (newName == null)
            {
                return BadRequest(new Response("400", "Username not null!"));
            }

            var authorization = Request.Headers["Authorization"];
            var accessToken = authorization.ToString().Replace("Bearer ", "");
            var id = _jWTService.GetIdByToken(accessToken);

            if (!_userService.UserExistsById(id))
            {
                return NotFound(new Response("404", "User not found!"));
            }

            _userService.EditUserName(id, newName);

            return Ok(new Response("200", "Successfully!"));
        }

        // PUT: api/Users/ChangePassword
        [HttpPatch]
        [Route("ChangePassword")]
        public IActionResult ChangePasswordByToken([FromBody] ChangePasswordForm changePasswordForm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorization = Request.Headers["Authorization"];
            var accessToken = authorization.ToString().Replace("Bearer ", "");
            var id = _jWTService.GetIdByToken(accessToken);

            if (!_userService.UserExistsById(id))
            {
                return NotFound(new Response("404", "User not found!"));
            }

            if (!_userService.CheckOldPassword(id, changePasswordForm.OldPassword))
            {
                return BadRequest(new Response("400", "Old password not correct!"));
            }

            _userService.ChangePassword(id, changePasswordForm.NewPassword);

            return Ok(new Response("200", "Successfully!"));
        }

        // POST: api/Users
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] AddUserForm addUserForm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_userService.UserExistsByEmail(addUserForm.Email))
            {
                return BadRequest(new Response("400", "Email already exists"));
            }

            _userService.AddUser(addUserForm);
            await _context.SaveChangesAsync();

            _userService.UpdateSyncCode(_userService.GetIdByEmail(addUserForm.Email));
            await _context.SaveChangesAsync();

            return Ok(new Response("200", "Successfully added!"));
        }
    }
}