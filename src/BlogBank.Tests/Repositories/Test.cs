namespace BlogBank.Tests.Repositories;

public class Test
{
    
    [Theory]
    [InlineData(1233)]
    [InlineData(12334)]
    public async Task GetById_ShouldReturnArticle_WhenArticleExists(long id)
    {
        var t = new Test();
        var start = DateTime.Now.AddHours(-2).AddMilliseconds(-16);
        var end = DateTime.Now.AddHours(-3).AddMilliseconds(-16);
        var time = DateTime.Now;

        var t1 = time - start;
        Console.WriteLine(t1.Minutes);
    } 
}