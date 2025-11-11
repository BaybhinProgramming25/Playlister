using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CommunityList
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("items")]
    public required List<ItemVote> Items { get; set; }

    [BsonElement("likes")]
    public required List<string> Likes { get; set; }

    [BsonElement("dislikes")]
    public required List<string> Dislikes { get; set; }

    [BsonElement("views")]
    public int Views { get; set; }

    [BsonElement("comments")]
    public required List<string[]> Comments { get; set; }

    [BsonElement("publishedAt")]
    public required int[] PublishedAt { get; set; }
}