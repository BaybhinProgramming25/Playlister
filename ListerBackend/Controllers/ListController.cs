using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Top5ListController : ControllerBase
{
    private readonly IMongoCollection<Top5List> _top5Lists;
    private readonly IMongoCollection<CommunityList> _communityLists;
    private readonly IMongoCollection<User> _users;

    public Top5ListController(IMongoDatabase database)
    {
        _top5Lists = database.GetCollection<Top5List>("Top5Lists");
        _communityLists = database.GetCollection<CommunityList>("CommunityLists");
        _users = database.GetCollection<User>("Users");
    }

    // POST api/top5list
    [HttpPost]
    public async Task<IActionResult> CreateTop5List([FromBody] Top5List body)
    {
        try
        {
            if (body == null)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "You must provide a Top 5 List"
                });
            }

            var userId = User.FindFirst("userId")?.Value;
            
            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new 
                { 
                    success = false, 
                    errorMessage = "Please log in to create new list" 
                });
            }

            var owner = await GetOwner(userId);
            body.Owner = owner;
            body.Likes = new List<string>();
            body.Dislikes = new List<string>();
            body.Views = 0;
            body.Comments = new List<string[]>();
            body.Published = false;

            Console.WriteLine("creating top5List: " + System.Text.Json.JsonSerializer.Serialize(body));

            await _top5Lists.InsertOneAsync(body);

            return StatusCode(201, new
            {
                success = true,
                top5List = body,
                message = "Top 5 List Created!"
            });
        }
        catch (Exception error)
        {
            return BadRequest(new
            {
                error = error.Message,
                errorMessage = "Top 5 List Not Created!"
            });
        }
    }

    // PUT api/top5list/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTop5List(string id, [FromBody] UpdateTop5ListRequest body)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var owner = userId == "Guest" || string.IsNullOrEmpty(userId) 
                ? "Guest" 
                : await GetOwner(userId);

            Console.WriteLine("updateTop5List: " + System.Text.Json.JsonSerializer.Serialize(body));

            if (body == null)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "You must provide a body to update"
                });
            }

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null || top5List.Owner != owner)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                return StatusCode(403, new
                {
                    errorMessage = "Cannot modify published list!"
                });
            }

            var update = Builders<Top5List>.Update
                .Set(t => t.Name, body.Name)
                .Set(t => t.Items, body.Items);

            await _top5Lists.UpdateOneAsync(filter, update);

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                id = top5List.Id,
                message = "Top 5 List updated!"
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List not updated!"
            });
        }
    }

    // PUT api/top5list/publish/{id}
    [HttpPut("publish/{id}")]
    public async Task<IActionResult> PublishTop5List(string id, [FromBody] PublishTop5ListRequest body)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var owner = userId == "Guest" || string.IsNullOrEmpty(userId) 
                ? "Guest" 
                : await GetOwner(userId);

            Console.WriteLine("publishTop5List: " + System.Text.Json.JsonSerializer.Serialize(body));

            if (body == null)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "You must provide a body to update"
                });
            }

            if (string.IsNullOrEmpty(body.Name) || 
                body.Items == null || 
                body.Items.Count != 5 ||
                body.Items.Any(string.IsNullOrEmpty))
            {
                return StatusCode(403, new 
                { 
                    success = false, 
                    errorMessage = "Must fill in all items and title." 
                });
            }

            // Check for duplicate items
            if (body.Items.Distinct().Count() != body.Items.Count)
            {
                return StatusCode(403, new 
                { 
                    success = false, 
                    errorMessage = "All items should be unique" 
                });
            }

            // Check if list with same name already published by this owner
            var duplicateFilter = Builders<Top5List>.Filter.And(
                Builders<Top5List>.Filter.Eq(t => t.Owner, owner),
                Builders<Top5List>.Filter.Eq(t => t.Name, body.Name),
                Builders<Top5List>.Filter.Eq(t => t.Published, true)
            );
            var duplicateList = await _top5Lists.Find(duplicateFilter).FirstOrDefaultAsync();

            if (duplicateList != null)
            {
                Console.WriteLine("there is list with same name");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(duplicateList));
                return StatusCode(403, new 
                { 
                    success = false, 
                    errorMessage = "Cannot publish list with same name" 
                });
            }

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null || top5List.Owner != owner)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                return StatusCode(403, new
                {
                    errorMessage = "Cannot modify published list!"
                });
            }

            var now = DateTime.Now;

            // Update or create community list
            var communityFilter = Builders<CommunityList>.Filter.Eq(c => c.Name, body.Name);
            var communityList = await _communityLists.Find(communityFilter).FirstOrDefaultAsync();

            Console.WriteLine("community list found: " + System.Text.Json.JsonSerializer.Serialize(communityList));

            if (communityList == null)
            {
                communityList = new CommunityList
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = body.Name,
                    Items = new List<ItemVote>(),
                    Likes = new List<string>(),
                    Dislikes = new List<string>(),
                    Views = 0,
                    Comments = new List<string[]>(),
                    PublishedAt = [now.Year, now.Month, now.Day]
                };

                Console.WriteLine("creating communityList: " + System.Text.Json.JsonSerializer.Serialize(communityList));
            }

            // Update community list items with votes
            for (int i = 0; i < 5; i++)
            {
                var existingItem = communityList.Items.FirstOrDefault(a => a.Name == body.Items[i]);
                
                if (existingItem != null)
                {
                    existingItem.Votes += (5 - i);
                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(existingItem));
                }
                else
                {
                    communityList.Items.Add(new ItemVote 
                    { 
                        Name = body.Items[i], 
                        Votes = 5 - i 
                    });
                }
            }

            communityList.PublishedAt = [now.Year, now.Month, now.Day];

            if (communityList.Id == null)
            {
                await _communityLists.InsertOneAsync(communityList);
            }
            else
            {
                var communityUpdate = Builders<CommunityList>.Update
                    .Set(c => c.Items, communityList.Items)
                    .Set(c => c.PublishedAt, communityList.PublishedAt);
                await _communityLists.UpdateOneAsync(communityFilter, communityUpdate);
            }

            Console.WriteLine("updated communityList!");

            // Update top5 list
            var update = Builders<Top5List>.Update
                .Set(t => t.Name, body.Name)
                .Set(t => t.Items, body.Items)
                .Set(t => t.Likes, new List<string>())
                .Set(t => t.Dislikes, new List<string>())
                .Set(t => t.Views, 0)
                .Set(t => t.Comments, new List<string[]>())
                .Set(t => t.Published, true)
                .Set(t => t.PublishedAt, [now.Year, now.Month, now.Day]);

            await _top5Lists.UpdateOneAsync(filter, update);

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                id = top5List.Id,
                message = "Top 5 List updated!"
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List not updated!"
            });
        }
    }

    // DELETE api/top5list/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTop5List(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var owner = userId == "Guest" || string.IsNullOrEmpty(userId) 
                ? "Guest" 
                : await GetOwner(userId);

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            if (top5List == null || top5List.Owner != owner)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            var now = DateTime.Now;

            if (top5List.Published)
            {
                var communityFilter = Builders<CommunityList>.Filter.Eq(c => c.Name, top5List.Name);
                var communityList = await _communityLists.Find(communityFilter).FirstOrDefaultAsync();

                Console.WriteLine("community list found: " + System.Text.Json.JsonSerializer.Serialize(communityList));

                if (communityList == null)
                {
                    return StatusCode(403, new
                    {
                        errorMessage = "Error with CommunityList"
                    });
                }

                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(communityList.Items));
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(top5List.Items));

                for (int i = 0; i < 5; i++)
                {
                    var item = communityList.Items.FirstOrDefault(a => a.Name == top5List.Items[i]);
                    if (item != null)
                    {
                        item.Votes -= (5 - i);
                        if (item.Votes < 0)
                        {
                            Console.WriteLine("vote<0");
                        }
                        else if (item.Votes == 0)
                        {
                            communityList.Items.Remove(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error with CommunityList(cannot find item)\nitem: {item}\ncitem: {System.Text.Json.JsonSerializer.Serialize(communityList.Items)}");
                    }
                }

                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(communityList.Items));

                if (communityList.Items.Count < 5)
                {
                    if (communityList.Items.Count > 0)
                    {
                        Console.WriteLine("length is less than 5");
                    }
                    else
                    {
                        Console.WriteLine("deleting " + System.Text.Json.JsonSerializer.Serialize(communityList));
                        var deleteFilter = Builders<CommunityList>.Filter.Eq(c => c.Id, communityList.Id);
                        await _communityLists.DeleteOneAsync(deleteFilter);
                    }
                }
                else
                {
                    communityList.PublishedAt = [now.Year, now.Month, now.Day];
                    var updateFilter = Builders<CommunityList>.Filter.Eq(c => c.Id, communityList.Id);
                    var update = Builders<CommunityList>.Update
                        .Set(c => c.Items, communityList.Items)
                        .Set(c => c.PublishedAt, communityList.PublishedAt);
                    await _communityLists.UpdateOneAsync(updateFilter, update);
                }
            }

            await _top5Lists.DeleteOneAsync(filter);
            return Ok(new { success = true, data = top5List });
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return StatusCode(500);
        }
    }

    // GET api/top5list/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTop5ListById(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var owner = userId == "Guest" || string.IsNullOrEmpty(userId) 
                ? "Guest" 
                : await GetOwner(userId);

            Console.WriteLine(owner);

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var list = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            if (list == null)
            {
                return BadRequest(new { success = false, errorMessage = "List does not exist" });
            }

            if (!list.Published && list.Owner != owner)
            {
                return BadRequest(new { success = false, errorMessage = "List does not exist" });
            }

            var pair = new
            {
                _id = list.Id,
                name = list.Name,
                items = list.Items,
                owner = list.Owner,
                likes = list.Likes?.Count ?? 0,
                dislikes = list.Dislikes?.Count ?? 0,
                views = list.Views,
                comments = list.Comments,
                published = list.Published,
                publishedAt = list.PublishedAt
            };

            return Ok(new { success = true, top5List = pair });
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return BadRequest(new { success = false, errorMessage = err.Message });
        }
    }

    // GET api/top5list
    [HttpGet]
    public async Task<IActionResult> GetTop5Lists()
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            var owner = userId == "Guest" || string.IsNullOrEmpty(userId) 
                ? "Guest" 
                : await GetOwner(userId);

            var filter = Builders<Top5List>.Filter.Or(
                Builders<Top5List>.Filter.Eq(t => t.Owner, owner),
                Builders<Top5List>.Filter.Eq(t => t.Published, true)
            );

            var top5Lists = await _top5Lists.Find(filter).ToListAsync();

            if (top5Lists == null)
            {
                Console.WriteLine("!top5Lists.length");
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Top 5 Lists not found"
                });
            }

            var pairs = top5Lists.Select(list => new
            {
                _id = list.Id,
                name = list.Name,
                items = list.Items,
                owner = list.Owner,
                likes = list.Likes?.Count ?? 0,
                dislikes = list.Dislikes?.Count ?? 0,
                views = list.Views,
                comments = list.Comments,
                published = list.Published,
                publishedAt = list.PublishedAt
            }).ToList();

            return Ok(new { success = true, idNamePairs = pairs });
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return BadRequest(new { success = false, errorMessage = err.Message });
        }
    }

    // GET api/top5list/community
    [HttpGet("community")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommunityLists()
    {
        try
        {
            var communityLists = await _communityLists.Find(_ => true).ToListAsync();

            if (communityLists == null)
            {
                Console.WriteLine("!communityLists.length");
                return NotFound(new
                {
                    success = false,
                    errorMessage = "Community Lists not found"
                });
            }

            var pairs = communityLists.Select(list =>
            {
                var sortedItems = list.Items.OrderByDescending(a => a.Votes).Take(5).ToList();
                var formattedItems = sortedItems.Select(item => $"{item.Name} ({item.Votes} votes)").ToList();

                return new
                {
                    _id = list.Id,
                    name = list.Name,
                    items = formattedItems,
                    owner = "Community",
                    likes = list.Likes?.Count ?? 0,
                    dislikes = list.Dislikes?.Count ?? 0,
                    views = list.Views,
                    comments = list.Comments,
                    published = true,
                    publishedAt = list.PublishedAt
                };
            }).ToList();

            return Ok(new { success = true, idNamePairs = pairs });
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return BadRequest(new { success = false, errorMessage = err.Message });
        }
    }

    // GET api/top5list/community/{id}
    [HttpGet("community/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommunityListById(string id)
    {
        try
        {
            var filter = Builders<CommunityList>.Filter.Eq(c => c.Id, id);
            var list = await _communityLists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("communityList found: " + System.Text.Json.JsonSerializer.Serialize(list));

            if (list == null)
            {
                return BadRequest(new { success = false, errorMessage = "Community List not found" });
            }

            var sortedItems = list.Items.OrderByDescending(a => a.Votes).Take(5).ToList();
            var formattedItems = sortedItems.Select(item => $"{item.Name} ({item.Votes} votes)").ToList();

            var pair = new
            {
                _id = list.Id,
                name = list.Name,
                items = formattedItems,
                owner = "Community",
                likes = list.Likes?.Count ?? 0,
                dislikes = list.Dislikes?.Count ?? 0,
                views = list.Views,
                comments = list.Comments,
                published = true,
                publishedAt = list.PublishedAt
            };

            return Ok(new { success = true, top5List = pair });
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
            return BadRequest(new { success = false, errorMessage = err.Message });
        }
    }

    // PUT api/top5list/views/{id}
    [HttpPut("views/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateTop5ListViews(string id)
    {
        try
        {
            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                var update = Builders<Top5List>.Update.Inc(t => t.Views, 1);
                await _top5Lists.UpdateOneAsync(filter, update);
            }

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                id = top5List.Id,
                message = "Top 5 List views updated!"
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List views not updated!"
            });
        }
    }

    // PUT api/top5list/likes/{id}
    [HttpPut("likes/{id}")]
    public async Task<IActionResult> UpdateTop5ListLikes(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to like/dislike"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                var likes = top5List.Likes ?? new List<string>();
                var dislikes = top5List.Dislikes ?? new List<string>();

                if (likes.Contains(owner))
                {
                    likes.Remove(owner);
                }
                else
                {
                    likes.Add(owner);
                }

                if (dislikes.Contains(owner))
                {
                    dislikes.Remove(owner);
                }

                var update = Builders<Top5List>.Update
                    .Set(t => t.Likes, likes)
                    .Set(t => t.Dislikes, dislikes);

                await _top5Lists.UpdateOneAsync(filter, update);

                // Refresh to get updated data
                top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();
            }

            var pair = new
            {
                _id = top5List.Id,
                name = top5List.Name,
                items = top5List.Items,
                owner = top5List.Owner,
                likes = top5List.Likes?.Count ?? 0,
                dislikes = top5List.Dislikes?.Count ?? 0,
                views = top5List.Views,
                comments = top5List.Comments,
                published = top5List.Published,
                publishedAt = top5List.PublishedAt
            };

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                top5List = pair
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List views not updated!"
            });
        }
    }

    // PUT api/top5list/dislikes/{id}
    [HttpPut("dislikes/{id}")]
    public async Task<IActionResult> UpdateTop5ListDislikes(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to like/dislike"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                var likes = top5List.Likes ?? new List<string>();
                var dislikes = top5List.Dislikes ?? new List<string>();

                if (likes.Contains(owner))
                {
                    likes.Remove(owner);
                }

                if (dislikes.Contains(owner))
                {
                    dislikes.Remove(owner);
                }
                else
                {
                    dislikes.Add(owner);
                }

                var update = Builders<Top5List>.Update
                    .Set(t => t.Likes, likes)
                    .Set(t => t.Dislikes, dislikes);

                await _top5Lists.UpdateOneAsync(filter, update);

                // Refresh to get updated data
                top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();
            }

            var pair = new
            {
                _id = top5List.Id,
                name = top5List.Name,
                items = top5List.Items,
                owner = top5List.Owner,
                likes = top5List.Likes?.Count ?? 0,
                dislikes = top5List.Dislikes?.Count ?? 0,
                views = top5List.Views,
                comments = top5List.Comments,
                published = top5List.Published,
                publishedAt = top5List.PublishedAt
            };

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                top5List = pair
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List views not updated!"
            });
        }
    }

    // PUT api/top5list/community/views/{id}
    [HttpPut("community/views/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateCommunityListViews(string id)
    {
        try
        {
            var filter = Builders<CommunityList>.Filter.Eq(c => c.Id, id);
            var communityList = await _communityLists.Find(filter).FirstOrDefaultAsync();

            if (communityList == null)
            {
                return NotFound(new
                {
                    errorMessage = "Community List not found!"
                });
            }

            var update = Builders<CommunityList>.Update.Inc(c => c.Views, 1);
            await _communityLists.UpdateOneAsync(filter, update);

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                id = communityList.Id,
                message = "Community List views updated!"
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Community List views not updated!"
            });
        }
    }

    // PUT api/top5list/community/likes/{id}
    [HttpPut("community/likes/{id}")]
    public async Task<IActionResult> UpdateCommunityListLikes(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to like/dislike"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<CommunityList>.Filter.Eq(c => c.Id, id);
            var top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("communityList found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Community List not found!"
                });
            }

            var likes = top5List.Likes ?? new List<string>();
            var dislikes = top5List.Dislikes ?? new List<string>();

            if (likes.Contains(owner))
            {
                likes.Remove(owner);
            }
            else
            {
                likes.Add(owner);
            }

            if (dislikes.Contains(owner))
            {
                dislikes.Remove(owner);
            }

            var update = Builders<CommunityList>.Update
                .Set(c => c.Likes, likes)
                .Set(c => c.Dislikes, dislikes);

            await _communityLists.UpdateOneAsync(filter, update);

            // Refresh to get updated data
            top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            var sortedItems = top5List.Items.OrderByDescending(a => a.Votes).Take(5).ToList();
            var formattedItems = sortedItems.Select(item => $"{item.Name} ({item.Votes} votes)").ToList();

            var pair = new
            {
                _id = top5List.Id,
                name = top5List.Name,
                items = formattedItems,
                owner = "Community",
                likes = top5List.Likes?.Count ?? 0,
                dislikes = top5List.Dislikes?.Count ?? 0,
                views = top5List.Views,
                comments = top5List.Comments,
                published = true,
                publishedAt = top5List.PublishedAt
            };

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                top5List = pair
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Community List not updated!"
            });
        }
    }

    // PUT api/top5list/community/dislikes/{id}
    [HttpPut("community/dislikes/{id}")]
    public async Task<IActionResult> UpdateCommunityListDislikes(string id)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to like/dislike"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<CommunityList>.Filter.Eq(c => c.Id, id);
            var top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("communityList found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Community List not found!"
                });
            }

            var likes = top5List.Likes ?? new List<string>();
            var dislikes = top5List.Dislikes ?? new List<string>();

            if (likes.Contains(owner))
            {
                likes.Remove(owner);
            }

            if (dislikes.Contains(owner))
            {
                dislikes.Remove(owner);
            }
            else
            {
                dislikes.Add(owner);
            }

            var update = Builders<CommunityList>.Update
                .Set(c => c.Likes, likes)
                .Set(c => c.Dislikes, dislikes);

            await _communityLists.UpdateOneAsync(filter, update);

            // Refresh to get updated data
            top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            var sortedItems = top5List.Items.OrderByDescending(a => a.Votes).Take(5).ToList();
            var formattedItems = sortedItems.Select(item => $"{item.Name} ({item.Votes} votes)").ToList();

            var pair = new
            {
                _id = top5List.Id,
                name = top5List.Name,
                items = formattedItems,
                owner = "Community",
                likes = top5List.Likes?.Count ?? 0,
                dislikes = top5List.Dislikes?.Count ?? 0,
                views = top5List.Views,
                comments = top5List.Comments,
                published = true,
                publishedAt = top5List.PublishedAt
            };

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                top5List = pair
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Community List not updated!"
            });
        }
    }

    // PUT api/top5list/comments/{id}
    [HttpPut("comments/{id}")]
    public async Task<IActionResult> UpdateTop5ListComments(string id, [FromBody] CommentRequest body)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to leave comments"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<Top5List>.Filter.Eq(t => t.Id, id);
            var top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("top5List found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Top 5 List not found!"
                });
            }

            if (top5List.Published)
            {
                var comments = top5List.Comments ?? new List<string[]>();
                comments.Add([owner, body.Comment]);

                var update = Builders<Top5List>.Update.Set(t => t.Comments, comments);
                await _top5Lists.UpdateOneAsync(filter, update);

                // Refresh to get updated data
                top5List = await _top5Lists.Find(filter).FirstOrDefaultAsync();

                var pair = new
                {
                    _id = top5List.Id,
                    name = top5List.Name,
                    items = top5List.Items,
                    owner = top5List.Owner,
                    likes = top5List.Likes?.Count ?? 0,
                    dislikes = top5List.Dislikes?.Count ?? 0,
                    views = top5List.Views,
                    comments = top5List.Comments,
                    published = top5List.Published,
                    publishedAt = top5List.PublishedAt
                };

                Console.WriteLine("SUCCESS!!!");
                return Ok(new
                {
                    success = true,
                    top5List = pair
                });
            }

            return Ok(new { success = true });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Top 5 List not updated!"
            });
        }
    }

    [HttpPut("community/comments/{id}")]
    public async Task<IActionResult> UpdateCommunityListComments(string id, [FromBody] CommentRequest body)
    {
        try
        {
            Console.WriteLine("updating c list comments");

            var userId = User.FindFirst("userId")?.Value;

            if (userId == "Guest" || string.IsNullOrEmpty(userId))
            {
                return StatusCode(403, new
                {
                    success = false,
                    errorMessage = "Please login to leave comments"
                });
            }

            var owner = await GetOwner(userId);

            var filter = Builders<CommunityList>.Filter.Eq(c => c.Id, id);
            var top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            Console.WriteLine("communityList found: " + System.Text.Json.JsonSerializer.Serialize(top5List));

            if (top5List == null)
            {
                return NotFound(new
                {
                    errorMessage = "Community List not found!"
                });
            }

            var comments = top5List.Comments ?? new List<string[]>();
            comments.Add([owner, body.Comment]);

            var update = Builders<CommunityList>.Update.Set(c => c.Comments, comments);
            await _communityLists.UpdateOneAsync(filter, update);

            // Refresh to get updated data
            top5List = await _communityLists.Find(filter).FirstOrDefaultAsync();

            var sortedItems = top5List.Items.OrderByDescending(a => a.Votes).Take(5).ToList();
            var formattedItems = sortedItems.Select(item => $"{item.Name} ({item.Votes} votes)").ToList();

            var pair = new
            {
                _id = top5List.Id,
                name = top5List.Name,
                items = formattedItems,
                owner = "Community",
                likes = top5List.Likes?.Count ?? 0,
                dislikes = top5List.Dislikes?.Count ?? 0,
                views = top5List.Views,
                comments = top5List.Comments,
                published = true,
                publishedAt = top5List.PublishedAt
            };

            Console.WriteLine("SUCCESS!!!");
            return Ok(new
            {
                success = true,
                top5List = pair
            });
        }
        catch (Exception error)
        {
            Console.WriteLine("FAILURE: " + System.Text.Json.JsonSerializer.Serialize(error));
            return NotFound(new
            {
                error = error.Message,
                errorMessage = "Community List not updated!"
            });
        }
    }

    private async Task<string> GetOwner(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var loggedInUser = await _users.Find(filter).FirstOrDefaultAsync();
        return $"{loggedInUser.FirstName} {loggedInUser.LastName}";
    }
}

// Request models
public class UpdateTop5ListRequest
{
    public string Name { get; set; }
    public List<string> Items { get; set; }
}

public class PublishTop5ListRequest
{
    public string Name { get; set; }
    public List<string> Items { get; set; }
}

public class CommentRequest
{
    public string Comment { get; set; }
}