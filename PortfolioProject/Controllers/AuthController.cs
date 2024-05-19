using Auth.Data;
using Auth.Migrations;
using Auth.Models;
using Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auth.Controllers
{
	public class AuthController : Controller
	{
		//public IActionResult Register()
		//{


		//    //using (RSACryptoServiceProvider rsa = new())
		//    //{
		//    //    rsa.ImportFromPem(publicKeyPem);
		//    //    Console.WriteLine(Crypto.Encrypt("aztakurva bgec", rsa.ExportParameters(false), false));
		//    //}

		//    //using (RSACryptoServiceProvider rsa = new())
		//    //{
		//    //    rsa.ImportFromPem(privateKeyPem);
		//    //    Console.WriteLine(Crypto.Decrypt("SbVDXWcvkKfW7u+BGqhLVCsetGmKLuFPOdqmdqDN0I489I38/GG1y4B0f/ea0ltkt6PP6w2US0kjhmMkBVVQ6sJA9a51FGMd7jTcqYmVoYtZ87KOWwzQGnlpv1oPE0zsOY56HrdUpumJ6CCxVrXidM6g4ERKUsptowAG+Lu+Sec=", rsa.ExportParameters(true), false));
		//    //}

		//    //string original = "secret message";
		//    //string encrypted;
		//    //string decrypted;
		//    //string s;

		//    //using (RSACryptoServiceProvider rsa = new())
		//    //{
		//    //    // Encrypt the string
		//    //    encrypted = Crypto.Encrypt(original, rsa.ExportParameters(false), false);
		//    //    Console.WriteLine("Encrypted: {0}", encrypted);


		//    //    Console.WriteLine(rsa.ExportRSAPrivateKeyPem());
		//    //    s = rsa.ExportRSAPrivateKeyPem();
		//    //}

		//    //using (RSACryptoServiceProvider rsa = new())
		//    //{
		//    //    rsa.ImportFromPem(s);

		//    //    // Decrypt the string
		//    //    decrypted = Crypto.Decrypt(encrypted, rsa.ExportParameters(true), false);
		//    //    Console.WriteLine("Decrypted: {0}", decrypted);
		//    //}
		//    return Content($"");
		//}

		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;
		private readonly IUserStore<AppUser> _userStore;
		private readonly IUserEmailStore<AppUser> _emailStore;
		private readonly ApplicationDbContext _context;

		public AuthController(UserManager<AppUser> userManager, IUserStore<AppUser> userStore, SignInManager<AppUser> signInManager, ApplicationDbContext db)
		{
			_userManager = userManager;
			_userStore = userStore;
			_emailStore = GetEmailStore();
			_signInManager = signInManager;
			_context = db;
		}

		[HttpPost]
		public async Task<string> Register(string email, string password)
		{
			var user = CreateUser();

			using (RSACryptoServiceProvider rsa = new())
			{
				user.PrivateKey = rsa.ExportRSAPrivateKeyPem();
				user.PublicKey = rsa.ExportRSAPublicKeyPem();
			}

			await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
			await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
			var result = await _userManager.CreateAsync(user, password);

			if (result.Succeeded)
			{
				return $"{user.PublicKey}";
			}

			return password;
		}

		[HttpPost]
		public async Task<string> Login(string email, string password)
		{
			var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
			if (result.Succeeded)
			{
				AppUser applicationUser = await _userManager.GetUserAsync(User);
				
				_context.LoginTokens.Add(new LoginToken() { Token = applicationUser.PublicKey, UserName = applicationUser.UserName });
				_context.SaveChanges();

				return applicationUser.PublicKey;
			}

			return "Invalid login attempt.";
		}

		private AppUser CreateUser()
		{
			try
			{
				return Activator.CreateInstance<AppUser>();
			}
			catch
			{
				throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
					$"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
					$"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
			}
		}

		private IUserEmailStore<AppUser> GetEmailStore()
		{
			if (!_userManager.SupportsUserEmail)
			{
				throw new NotSupportedException("The default UI requires a user store with email support.");
			}
			return (IUserEmailStore<AppUser>)_userStore;
		}


		// byte[] data = Convert.FromBase64String(token);
		// DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
		// if (when<DateTime.UtcNow.AddHours(-24)) {
		//   // too old
		// }

		//byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
		//byte[] key = Guid.NewGuid().ToByteArray();

		//string token = Convert.ToBase64String(time.Concat(key).ToArray());

	}
}