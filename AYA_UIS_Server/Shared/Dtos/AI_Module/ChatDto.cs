namespace Shared.Dtos.AI_Module
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = string.Empty; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto> History { get; set; } = new();
    }
}
