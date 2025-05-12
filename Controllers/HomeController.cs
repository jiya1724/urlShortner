using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShortener.Data;
using UrlShortener.Models;

namespace Url_shortner.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private static readonly string _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly Random _random = new Random();

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShortenUrl(string originalUrl)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
        {
            ModelState.AddModelError("originalUrl", "URL is required.");
            return View("Index");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string shortCode;
        do
        {
            shortCode = GenerateShortCode(6);
        } while (await _context.ShortUrls.AnyAsync(s => s.ShortCode == shortCode));

        var shortUrl = new ShortUrl
        {
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.ShortUrls.Add(shortUrl);
        await _context.SaveChangesAsync();

        ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{shortCode}";
        return View("Index");
    }

    private string GenerateShortCode(int length)
    {
        return new string(Enumerable.Repeat(_chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
