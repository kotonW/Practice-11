using Microsoft.EntityFrameworkCore;

namespace Practice11.Tests;

public class CRUDTests
{
    [Theory]
    [InlineData("test text")]
    [InlineData("")]
    public async Task Create_ValidText_Success(string text)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        var note = await CRUD.Create(text);

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
        Assert.Throws<AggregateException>(() => CRUD.Create(null!).Result);
        await db.Database.EnsureDeletedAsync();
    }

    [Theory]
    [InlineData("test text")]
    [InlineData("q")]
    public async Task Read_ValidText_Success(string search)
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        db.Notes.Add(new Note
        {
            Text = search,
            CreatedAt = DateTimeOffset.Now
        });
        db.Notes.Add(new Note
        {
            Text = $"{search} lalala",
            CreatedAt = DateTimeOffset.Now
        });
        db.Notes.Add(new Note
        {
            Text = $"lalala",
            CreatedAt = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();

        var result = await CRUD.Read(search);

        Assert.Equal(2, result.Count);
        await db.Database.EnsureDeletedAsync();
    }
    
    [Fact]
    public async Task Read_PassId_Success()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        db.Notes.Add(new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now
        });
        db.Notes.Add(new Note
        {
            Text = "tatata",
            CreatedAt = DateTimeOffset.Now
        });
        await db.SaveChangesAsync();
        var result1 = await CRUD.Read(1);
        var result2 = await CRUD.Read(2);
        var result3 = await CRUD.Read(700);
        
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
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        await CRUD.Update(note, change);
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
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now
        };
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        
        Assert.True(db.Notes.Contains(note));
        await CRUD.Delete(note);
        Assert.False(db.Notes.Contains(note));
        await db.Database.EnsureDeletedAsync();
    }
    
    [Fact]
    public async Task Delete_PassError_Fail()
    {
        await using var db = new DataContext();
        await db.Database.EnsureCreatedAsync();
        var note = new Note
        {
            Text = "lalala",
            CreatedAt = DateTimeOffset.Now
        };
        
        await Assert.ThrowsAsync<InvalidOperationException>(()=>CRUD.Delete(note));
        await db.Database.EnsureDeletedAsync();
    }
}