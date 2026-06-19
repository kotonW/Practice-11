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
    public async Task CreateNote_ValidText_Success(string text)
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
    public async Task CreateNote_NullText_Fail()
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
    public async Task ReadNote_ValidText_Success(string search)
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
    public async Task ReadNote_PassId_Success()
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
    public async Task UpdateNote_ValidText_Success(string change)
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
    public async Task DeleteNote_PassValid_Success()
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
    public async Task DeleteNote_PassError_Fail()
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
    
    [Theory]
    [InlineData("test1")]
    [InlineData("test2")]
    public async Task CreateUser_ValidUsername_Success(string name)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();

        var user = await CRUDUser.Create(name);

        Assert.NotNull(user);
        Assert.Equal(name, user.Name);
        Assert.True(user.Id != 0);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CreateUser_NullUsername_Fail()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();

        Assert.Throws<AggregateException>(() => CRUDUser.Create(null!).Result);

        await db.Database.EnsureDeletedAsync();
    }

    [Theory]
    [InlineData("user1")]
    [InlineData("user2")]
    public async Task ReadUser_TextValid_Success(string text)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();

        await CRUDUser.Create("TestUser");
        await CRUDUser.Create("TestUser2");
        await CRUDUser.Create("TestUser1");

        var result = await CRUDUser.Read(text);
        Assert.Single(result);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task ReadUser_TestId_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        await CRUDUser.Create("TestUser");
        await CRUDUser.Create("TestUser1");
        var result1 = await CRUDUser.Read(1);
        var result2 = await CRUDUser.Read(2);
        var result42 = await CRUDUser.Read(42);

        Assert.Null(result42);
        Assert.Equal("TestUser", result1.Name);
        Assert.Equal("TestUser1", result2.Name);

        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task UpdateUser_UserUsernameUpdate_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        var user = await CRUDUser.Create("TestUser");

        await CRUDUser.Update(user, "TestUser2");

        Assert.NotNull(user);
        Assert.NotEqual("TestUser", user.Name);
        Assert.Equal("TestUser2", user.Name);

        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task DeleteUser_UserDelete_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();

        var user = await CRUDUser.Create("TestUser");
        await CRUDUser.Delete(user);

        Assert.Empty(db.Notes);
        await db.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task GetUserNotesById_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        var user = await CRUDUser.Create("TestUser");
        await CRUDNote.Create("wawa", user.Id);
        await CRUDNote.Create("tuz tuz", user.Id);

        var result = await CRUDUser.GetUserNotes(1);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        await db.Database.EnsureDeletedAsync();
    }
}