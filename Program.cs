using HW9.Pages;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.MapPost("/toggleFavorite", async (HttpContext context) =>
{
    var btnID = int.Parse(context.Request.Form["btnID"]);
    var title = context.Request.Form["title"];
    var xmlLink = context.Request.Form["link"];
    Console.WriteLine(btnID);
    var favoriteFeedsCookie = JsonSerializer.Deserialize<List<FeedItem>>(context.Request.Cookies["favFeeds"]);
    var feedChosen = favoriteFeedsCookie.FirstOrDefault(x => x.ID == btnID);
    if (feedChosen != null)
    {
        favoriteFeedsCookie.Remove(feedChosen);
        feedChosen.IsFavorite = false;
    }
    else
    {
        //feedChosen = Outlines.FirstOrDefault(x => x.ID == btnID);
        //feedChosen.IsFavorite = true;
        feedChosen = new FeedItem { ID = btnID, IsFavorite = false, XmlLink = xmlLink, Text = title };
        favoriteFeedsCookie.Add(feedChosen);
    }

    var serializedFavFeeds = JsonSerializer.Serialize(favoriteFeedsCookie);
    context.Response.Cookies.Append("favFeeds", serializedFavFeeds, new CookieOptions
    {
        Path = "/",
        IsEssential = true,
        Expires = DateTime.Now.AddMinutes(10)
    });

    string redirectURL = context.Request.Headers["Referer"].ToString();
    context.Response.Redirect(redirectURL);

    //await context.Response.WriteAsync(feedChosen.IsFavorite.ToString());

});

app.MapRazorPages();

app.Run();
