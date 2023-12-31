using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace HW9.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItem> Outlines { get; set; } = new List<FeedItem>();
    public List<FeedItem> ItemsForPage { get; set; } = new List<FeedItem>();
    public int pageSize { get; set; } = 12;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<FeedItem>> GetOutlinesAsync()
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");
        XmlDocument opmlDocument = new XmlDocument();

        if (!Request.Cookies.ContainsKey("favFeeds"))
        {
            var serializedFavFeeds = JsonSerializer.Serialize(new List<FeedItem>());
            Response.Cookies.Append("favFeeds", serializedFavFeeds, new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.Now.AddMinutes(10)
            });
        }

        string OPMLcontent = await response.Content.ReadAsStringAsync();
        opmlDocument.LoadXml(OPMLcontent);
        var nodes = opmlDocument.SelectNodes("opml/body/outline");

        int id = 0;

        List<FeedItem> DeseralizedFavFeeds = null;
        var favFeedsJson = Request.Cookies["favFeeds"];

        if (!string.IsNullOrEmpty(favFeedsJson))
        {
            DeseralizedFavFeeds = JsonSerializer.Deserialize<List<FeedItem>>(favFeedsJson);
        }

        foreach (XmlNode node in nodes)
        {
            string Text = node.Attributes["text"].Value ?? "";
            string link = node.Attributes["xmlUrl"].Value ?? "";

            FeedItem newItem = new FeedItem()
            {
                ID = ++id,
                Text = Text,
                XmlLink = link
            };

            if (!string.IsNullOrEmpty(favFeedsJson))
            {
                string? favFeeds = DeseralizedFavFeeds.SingleOrDefault(x => x.ID == id)?.ToString(); //could cause a problem
                if (favFeeds != null)
                {
                    Console.WriteLine(id);
                    newItem.IsFavorite = true;
                }
            }
            Outlines.Add(newItem);
        }
        return Outlines;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Outlines = await GetOutlinesAsync();

        int totalPages = (int)Math.Ceiling((double)Outlines.Count / pageSize);
        int startIndex = (page - 1) * pageSize;
        int endIndex = startIndex + pageSize;
        ItemsForPage = Outlines.Skip(startIndex).Take(endIndex - startIndex).ToList();

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return Page();
    }

    //public async Task<IActionResult> OnPostToggleFavorite()
    //{
    //    Outlines = await GetOutlinesAsync();
    //    var btnID = int.Parse(Request.Form["btnID"]);
    //    Console.WriteLine(btnID);
    //    var favoriteFeedsCookie = JsonSerializer.Deserialize<List<FeedItem>>(Request.Cookies["favFeeds"]);
    //    var feedChosen = favoriteFeedsCookie.FirstOrDefault(x => x.ID == btnID);
    //    if (feedChosen != null)
    //    {
    //        favoriteFeedsCookie.Remove(feedChosen);
    //        feedChosen.IsFavorite = false;
    //    }
    //    else
    //    {
    //        feedChosen = Outlines.FirstOrDefault(x => x.ID == btnID);
    //        feedChosen.IsFavorite = true;
    //        favoriteFeedsCookie.Add(feedChosen);
    //    }

    //    var serializedFavFeeds = JsonSerializer.Serialize(favoriteFeedsCookie);
    //    Response.Cookies.Append("favFeeds", serializedFavFeeds);

    //    return RedirectToPage();
    //}
}

public class FeedItem
{
    public int ID { get; set; }
    public string? Text { get; set; }
    public string? XmlLink { get; set; }
    public bool IsFavorite { get; set; } = false;
}


