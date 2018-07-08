using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreWebApi1.Middleware
{

    public enum AppHttpHeaders
    {
        Authorization,
        LicenseKey,
        Basic
    }


    public class AuthUser
    {
        public bool IsAuthenticated { get; set; }
        public string UserName { get; set; }
    }

    public class AppHttpHeadersProcessor
    {
        public static AuthUser CheckBasicAuthorization(HttpRequest request)
        {
            if (request.Headers.TryGetValue(AppHttpHeaders.Authorization.ToString(), out var authHeader))
            {
                var firstAuthHeader = authHeader.FirstOrDefault();
                if (firstAuthHeader != null && firstAuthHeader.StartsWith(AppHttpHeaders.Basic.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    var kv = Encoding.UTF8.GetString(Convert.FromBase64String(
                        firstAuthHeader.Replace(AppHttpHeaders.Basic.ToString(), "",
                            StringComparison.OrdinalIgnoreCase)));

                    var up = kv.Split(':');
                    var userName = up.ElementAt<string>(0);
                    var password = up.ElementAt<string>(1);

                    if (userName == "test" && password == "test")
                    {
                        return new AuthUser { IsAuthenticated = true, UserName = userName }; ;
                    }
                }
            }

            return new AuthUser{ IsAuthenticated = false};
        }
    }

    public abstract class AppMiddleWare
    {

        protected readonly RequestDelegate NextRequest;

        protected AppMiddleWare(RequestDelegate nextRequest)
        {
            NextRequest = nextRequest;
        }

        public abstract Task Invoke(HttpContext httpContext);

    }

    public class AppUser : GenericIdentity
    {
        protected AppUser(GenericIdentity identity) : base(identity)
        {
        }

        public AppUser(string name) : base(name)
        {
        }

        public AppUser(string name, string type) : base(name, type)
        {
        }

        public string Email { get; set; }
    }


    public class BasicAuthenticationMiddleWare : AppMiddleWare
    {
        public BasicAuthenticationMiddleWare(RequestDelegate nextRequest) : base(nextRequest)
        {

        }

        public override async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var user = AppHttpHeadersProcessor.CheckBasicAuthorization(request);
            if (user.IsAuthenticated)
            {
                var identity = new AppUser(user.UserName);
                identity.AddClaim(new System.Security.Claims.Claim("email", "email"));
                var principal = new GenericPrincipal(identity, null);
                Thread.CurrentPrincipal = principal;
                httpContext.User = principal;
                await NextRequest(httpContext);
            }
            else
            {
                httpContext.Response.Headers.Add("WWW-Authenticate", "Basic");
                httpContext.Response.StatusCode = 401;
            }

            
        }
    }

}
