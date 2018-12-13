using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using VaultWebApp.Models;

namespace VaultWebApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Logon()
        {
            return View(new AuthModel());
        }

        [HttpPost]
        public async Task<IActionResult> Logon(AuthModel model)
        {
            try
            {
                var secretPassword = await GetValue(model.UserName);
                if (model.Password == secretPassword)
                {
                    //HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    //{
                    //    new Claim(ClaimTypes.Name, model.UserName)
                    //}, "someAuthTypeName"));

                    ViewData["AuthResult"] = $"Hello, {model.UserName}";
                    return View(new AuthModel());
                }
                ViewData["AuthResult"] = "Incorrect UserName or Password";
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new AuthModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(AuthModel model)
        {
            try
            {
                var functionUrl = "http://localhost:7071/api/Function1";// await GetValue("FunctionUrl");
                using(var httpClient = new HttpClient())
                {
                    var result = await httpClient.PostAsJsonAsync<AuthModel>(new Uri(functionUrl), model);
                    ViewData["AuthResult"] = result.ReasonPhrase;
                    return View(new AuthModel());
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return View(model);
        }

        private async Task<string> GetValue(string key)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"https://lab-keyvault.vault.azure.net/secrets/{key}").ConfigureAwait(false);
            return secret.Value;
        }
    }
}