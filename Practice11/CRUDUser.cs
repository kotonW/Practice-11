using Microsoft.EntityFrameworkCore;

namespace Practice11;

public class CRUDUser
{
    public static async Task<User> Create(string name, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = new User()
        {
            Name = name,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
    
    public static async Task<List<User>> Read(string search, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var result = await db.Users
            .Where(x => EF.Functions.Like(x.Name, $"%{search}%"))
            .Include(x => x.Notes)
            .ToListAsync(ct);
        return result;
    }
    
    public static async Task<User?> Read(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        return await db.Users
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
    
    public static async Task Update(User user, string name, CancellationToken ct = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        await using var db = new DataContext();
        user.Name = name;
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }
    
    public static async Task Delete(User user, CancellationToken ct = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        await using var db = new DataContext();
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }

    public static async Task Delete(int id, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var user = await db.Users.FindAsync(id);
        if (user == null) throw new ArgumentException($"User with id {id} not found");
        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
    }
    public static async Task<List<Note>> GetUserNotes(int userId, CancellationToken ct = default)
    {
        await using var db = new DataContext();
        var notes = await db.Notes
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.Id)
            .ToListAsync(ct);
        return notes;
    }
}