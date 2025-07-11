public interface IMentionService
{
    Task<List<int>> HandleMentionsAsync(string content);
}
