namespace Practice11;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public ICollection<Note>? Notes { get; set; }
}