using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Controllers
{
    [Authorize]
    public class ShortUrlController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private static readonly string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random _random = new Random();

        public ShortUrlController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /ShortUrl/Create
        // public IActionResult Create()
        // {
        //     return View();
        // }

        // POST: /ShortUrl/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create(string originalUrl)
        // {
        //     if (string.IsNullOrWhiteSpace(originalUrl))
        //     {
        //         ModelState.AddModelError("originalUrl", "URL is required.");
        //         return View();
        //     }
        //
        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     string shortCode;
        //     do
        //     {
        //         shortCode = GenerateShortCode(6);
        //     } while (await _context.ShortUrls.AnyAsync(s => s.ShortCode == shortCode));
        //
        //     var shortUrl = new ShortUrl
        //     {
        //         OriginalUrl = originalUrl,
        //         ShortCode = shortCode,
        //         UserId = userId,
        //         CreatedAt = DateTime.UtcNow
        //     };
        //     _context.ShortUrls.Add(shortUrl);
        //     await _context.SaveChangesAsync();
        //
        //     ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{shortCode}";
        //     return View("Success");
        // }

        // GET: /ShortUrl/MyUrls
        public async Task<IActionResult> MyUrls()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var urls = await _context.ShortUrls
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(urls);
        }

        // GET: /ShortUrl/{code}
        [AllowAnonymous]
        [HttpGet("/s/{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == code);
            if (shortUrl == null)
            {
                return NotFound();
            }
            return Redirect(shortUrl.OriginalUrl);
        }

        // POST: /ShortUrl/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shortUrl = await _context.ShortUrls
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (shortUrl == null)
            {
                return NotFound();
            }

            _context.ShortUrls.Remove(shortUrl);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyUrls));
        }

        private string GenerateShortCode(int length)
        {
            return new string(Enumerable.Repeat(_chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
} 