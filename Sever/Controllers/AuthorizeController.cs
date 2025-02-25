﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sever.Entity;
using Sever.Helper;
using Sever.Models;
using Sever.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace Sever.Controllers
{
    public class AuthorizeController : Controller
    {
        private const string currentUrl = "https://localhost:7194/'";
        private readonly IUserRepository _userRepository;
        private readonly IAuthCodeRepository _authCodeRepository;
        private readonly UserManager<User> _userManager;
        public AuthorizeController(IUserRepository userRepository, IAuthCodeRepository authCodeRepository, UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _authCodeRepository = authCodeRepository;
            _userManager = userManager;
        }
        public IActionResult Index(AuthenticationRequestModel request)
        {
            return View(request);
        }

        public async Task<IActionResult> Authorize(AuthenticationRequestModel request, string username , string password, string[] scopes)
        {
            
            var user =await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return View("NotFound");
            }

            var res =await _userManager.CheckPasswordAsync(user, password);   

            if (!res)
            {
                return View("NotFound");
            }

            var code = GeneratedCode();

            var codeItem = new AuthorizationRequestData()
            {

                AuthorizationCode = code,
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                Scope = string.Join(' ',scopes),
                User = user,
                Nonce = request.Nonce,
                State = request.State

            };

            await _authCodeRepository.Add(codeItem);


            var model = new CodeFlowViewResponseModel()
            {
                Code = code,
                State = request.State,
                RedirectUri = request.RedirectUri
            };

            return View("SubmitForm", model);
        }
        [Route("oauth/token")]
        [HttpPost]
        public async Task<IActionResult> ReturnTokens(string grant_type, string code, string redirect_uri)
        {
            if(grant_type != "authorization_code")
            {
                return BadRequest();
            }

            var codeItem =await _authCodeRepository.FindByCode(code);
            if (codeItem == null) {
                return BadRequest();
            }

            _authCodeRepository.Delete(code);

            if(codeItem.RedirectUri != redirect_uri)
            {
                return BadRequest();
            }

            var jwk = JwkLoader.LoadFromDefault();

            var model = new AuthTokenResponseModel()
            {
                AccessToken = GenerateAccessToken(codeItem.User.UserName,codeItem.Scope, codeItem.ClientId, codeItem.Nonce, jwk),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = GeneratedRefreshToken(),
                IdToken = GenerateIdToken(codeItem.User.UserName, codeItem.ClientId, codeItem.Nonce, jwk)
            };
            return Json(model);
        }

        private static string GeneratedRefreshToken()
        {
            return GeneratedCode();
        }

        static Random random = new();
        private static string GeneratedCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateAccessToken(string userId, string scope, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // access_token can be the same as id_token, but here we might have different values for expirySeconds so we use 2 different functions

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new("scope", scope)
            };
            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
                currentUrl,
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }

        private string GenerateIdToken(string userId, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // we can return some claims defined here: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId)
            };

            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
               currentUrl,
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }
    }
}
