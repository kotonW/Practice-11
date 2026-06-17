namespace Practice11;

public class Note
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required int UserId { get; set; }
    public User? User { get; set; }
}