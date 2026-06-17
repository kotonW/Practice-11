using Microsoft.EntityFrameworkCore;

namespace Practice11;

public class CRUDNote
{
    public static async Task<Note> Create(string text,int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) throw new ArgumentException("User not found");
        var note = new Note()
        {
            Text = text,
            CreatedAt = DateTimeOffset.Now,
            UserId = userId
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return note;
    }

    public static async Task<List<Note>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Notes
            .Where(x => EF.Functions.Like(x.Text, $"%{search}%"))
            .ToListAsync(ct);
        return result;
    }
    
    public static async Task<Note?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
    public static async Task Update(Note note, string text, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        if (note == null) throw new ArgumentNullException(nameof(note));
        note.Text = text;
        note.CreatedAt = DateTimeOffset.Now;
        db.Notes.Update(note);
        await db.SaveChangesAsync(ct);
    }
    
    public static async Task Delete(Note note, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        if (note == null) throw new ArgumentNullException(nameof(note));
        db.Notes.Remove(note);
        await db.SaveChangesAsync(ct);
    }
}