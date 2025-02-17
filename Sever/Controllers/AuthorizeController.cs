using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sever.Helper;
using Sever.Models;
using Sever.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sever.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthCodeRepository _authCodeRepository;
        public AuthorizeController(IUserRepository userRepository, IAuthCodeRepository authCodeRepository)
        {
            _userRepository = userRepository;
            _authCodeRepository = authCodeRepository;
        }
        public IActionResult Index(AuthenticationRequestModel request)
        {
            return View(request);
        }

        public IActionResult Authorize(AuthenticationRequestModel request, string username , string[] scopes)
        {
            if (!_userRepository.CheckUser(username))
            {
                return View("NotFound");
            }
            
            var user = _userRepository.Get(username);

            var code = GeneratedCode();

            var model = new CodeFlowViewResponseModel()
            {
                Code = code,
                State = request.State,
                RedirectUri = request.RedirectUri
            };

            var codeItem = new AuthCodeItem()
            {
                AuthenticationRequest = request,
                User = user,
                Scopes = scopes
            };

            _authCodeRepository.Add(code, codeItem);
            return View("SubmitForm", model);
        }
        [Route("oauth/token")]
        [HttpPost]
        public IActionResult ReturnTokens(string grant_type, string code, string redirect_uri)
        {
            if(grant_type != "authorization_code")
            {
                return BadRequest();
            }

            var codeItem = _authCodeRepository.FindByCode(code);
            if (codeItem == null) {
                return BadRequest();
            }

            _authCodeRepository.Delete(code);

            if(codeItem.AuthenticationRequest.RedirectUri != redirect_uri)
            {
                return BadRequest();
            }

            var jwk = JwkLoader.LoadFromDefault();

            var model = new AuthTokenResponseModel()
            {
                AccessToken = GenerateAccessToken(codeItem.User.UserName, string.Join(' ', codeItem.Scopes), codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jwk),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = GeneratedRefreshToken(),
                IdToken = GenerateIdToken(codeItem.User.UserName, codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jwk)
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
                "https://localhost:7194/",
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
                "https://localhost:7194/",
                audience,
                nonce,
                claims,
                jsonWebKey
                );


            return idToken;
        }
    }
}
