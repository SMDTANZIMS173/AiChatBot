using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AiChatBot.Models;
using AiChatBot.Services;
namespace AiChatBot.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chat;

        public ChatController(IChatService chat)
        {
            _chat = chat;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ChatViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ChatViewModel model)
        {
            // Add user message
            model.Messages.Add(("user", model.UserInput));

            // Get AI response based on full conversation
            var aiResponse = await _chat.GetAnswerAsync(model.Messages);

            // Add AI response
            model.Messages.Add(("ai", aiResponse));

            // Clear input box
            model.UserInput = "";

            return View("Index", model);
        }

    }
}
