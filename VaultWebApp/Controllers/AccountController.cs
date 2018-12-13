using System;
using System.Collections.Generic;
using System.Linq;
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
            return View(new LogonModel());
        }

        [HttpPost]
        public async Task<IActionResult> Logon(LogonModel model)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync($"https://lab-keyvault.vault.azure.net/secrets/{model.UserName}").ConfigureAwait(false);
                if (model.Password == secret.Value)
                {
                    //HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    //{
                    //    new Claim(ClaimTypes.Name, model.UserName)
                    //}, "someAuthTypeName"));

                    ViewData["AuthResult"] = $"Hello, {model.UserName}";
                    return View(new LogonModel());
                }
                ViewData["AuthResult"] = "Incorrect UserName or Password";
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return View(model);
        }
    }
}