namespace AiChatBot.Models
{
    public class ChatViewModel
    {
        public string UserInput { get; set; } = "";

        // Instead of single AiAnswer, keep full conversation
        public List<(string Role, string Message)> Messages { get; set; } = new();

        // Optional: keep last AI answer if you want
        public string AiAnswer => Messages.LastOrDefault(m => m.Role == "ai").Message;
    }
}
