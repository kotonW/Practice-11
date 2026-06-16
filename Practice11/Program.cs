using Microsoft.EntityFrameworkCore;
using Practice11;

await using var db = new DataContext();
await db.Database.EnsureCreatedAsync();