using System.Threading.Tasks;
namespace AiChatBot.Services
{
    public interface IChatService
    {
        Task<string> GetAnswerAsync(List<(string Role, string Message)> messages);
    }
}
