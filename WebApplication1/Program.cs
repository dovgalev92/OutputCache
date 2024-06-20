
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOutputCache(optPolicy =>
{
    optPolicy.AddPolicy("Policyone", optPolicy => { optPolicy.Expire(TimeSpan.FromSeconds(1)); });
});
var app = builder.Build();
app.UseOutputCache();

List<People> peoples = new List<People>
{
    new People() {Id = 1, Name = "Tom", Description = "Man"},
    new People() {Id = 2, Name = "Barsik", Description = "Cat"},
    new People() {Id = 3, Name = "Mucha", Description = "Dog"},
};
app.MapGet("/", async(context) =>{
    foreach (var people in peoples)
    {
        await context.Response.WriteAsync($"{people.Name}, {people.Id}, {people.Description}");
    }
}).CacheOutput(opt => opt.Tag("people"));
app.MapGet("{id}",[OutputCache(PolicyName = "Policyone")] (int id) =>
{
    var idPeople = peoples.FirstOrDefault(x => x.Id == id);
    return Results.Ok(idPeople.Name);

});
app.MapGet("/add/{id}", async (int id, string name, string description, [FromServices] IOutputCacheStore _store) 
   =>
{
    peoples.Add(new People() {Id = id, Name = name, Description = description });
    await _store.EvictByTagAsync("people", new CancellationToken());
    return Results.Ok(peoples);
});
app.Run();
