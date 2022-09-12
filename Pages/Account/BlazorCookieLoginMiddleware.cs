using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using YgoProgressionDuels.Data;
using YgoProgressionDuels.Models;

namespace YgoProgressionDuels.Pages.Account
{
    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginUserModel> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginUserModel>();


        private readonly RequestDelegate _next;

        public BlazorCookieLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SignInManager<ApplicationUser> signInManager)
        {
            if (context.Request.Path == "/Account/Login" && context.Request.Query.ContainsKey("key"))
            {
                Guid key = Guid.Parse(context.Request.Query["key"]);
                LoginUserModel loginModel = Logins[key];

                SignInResult result = await signInManager.PasswordSignInAsync(loginModel.UserName, loginModel.Password, loginModel.RememberMe, false);
                loginModel.Password = null;
                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    context.Response.Redirect("/");
                    return;
                }
                else
                {
                    //TODO: Proper error handling
                    context.Response.Redirect("/Account/LoginFailed");
                    return;
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
