using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Checkers.Models;
using Microsoft.AspNetCore.Identity;
using Checkers.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Checkers.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public readonly ApplicationDbContext _context;
        public readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger,ApplicationDbContext context, UserManager<User> userManager)
        {
            _logger = logger;

            _context = context;
            _userManager = userManager;
        }
        /*
         * Returns view for main page
         */
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.currentUserId = userId;
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.currentUserId = userId;
            if (User.Identity.IsAuthenticated)
                ViewBag.CurrentUserName = currentUser.UserName;
            List<Message> allmessages = await _context.Messages
                .Where(m=>m.RoomId==null)
                .OrderBy(m => m.Posted)
                .ToListAsync();
            IEnumerable<Message> messages = allmessages.TakeLast(20);
            return View(messages);
        }
        [Authorize]
        /*
         * Adds message to database
         */
        public async Task<IActionResult> Create(Message message)
        {
            if(ModelState.IsValid)
            {
                message.UserName = User.Identity.Name;
                var sender = await _userManager.GetUserAsync(User);
                message.UserId = sender.Id;
                message.Posted = DateTime.Now;
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return Error();
        }
        /*
        * Returns error page
        */
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
