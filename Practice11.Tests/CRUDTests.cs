using Microsoft.EntityFrameworkCore;

namespace Practice11.Tests;

public class CRUDTests
{
    private async Task CreateTestUser()
    {
        await using var db = new DataContext();
        var user = new User
        {
            Name = "test"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData("test text")]
    [InlineData("")]
    public async Task Create_ValidText_Success(string text)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        var note = await CRUDNote.Create(text, 1);

        Assert.NotNull(note);
        Assert.Equal(text, note.Text);
        Assert.NotEqual(0, note.Id);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Create_NullText_Fail()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        Assert.Throws<AggregateException>(() => CRUDNote.Create(null!, 1).Result);
        await db.Database.EnsureDeletedAsync();
    }

    [Theory]
    [InlineData("test text")]
    [InlineData("q")]
    public async Task Read_ValidText_Success(string search)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        db.Notes.Add(new Note
        {
            Text = search,
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        });
        db.Notes.Add(new Note
        {
            Text = $"{search} lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        });
        db.Notes.Add(new Note
        {
            Text = $"lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        });
        await db.SaveChangesAsync();

        var result = await CRUDNote.Read(search);

        Assert.Equal(2, result.Count);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Read_PassId_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        db.Notes.Add(new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        });
        db.Notes.Add(new Note
        {
            Text = "tatata",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        });
        await db.SaveChangesAsync();
        var result1 = await CRUDNote.Read(1);
        var result2 = await CRUDNote.Read(2);
        var result3 = await CRUDNote.Read(700);

        Assert.Equal("tatata", result2.Text);
        Assert.Equal("lalala", result1.Text);
        Assert.Null(result3);
        await db.Database.EnsureDeletedAsync();
    }

    [Theory]
    [InlineData("quake")]
    [InlineData("69")]
    [InlineData("")]
    public async Task Update_ValidText_Success(string change)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        await CRUDNote.Update(note, change);
        await db.SaveChangesAsync();

        Assert.NotNull(note);
        Assert.Equal(change, note.Text);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Delete_PassValid_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        Assert.True(db.Notes.Contains(note));
        await CRUDNote.Delete(note);
        Assert.False(db.Notes.Contains(note));
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Delete_PassError_Fail()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CreateTestUser();
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now,
            UserId = 1
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => CRUDNote.Delete(note));
        await db.Database.EnsureDeletedAsync();
    }
}