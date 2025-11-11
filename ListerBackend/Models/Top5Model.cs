using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Top5List
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("items")]
    public required List<string> Items { get; set; }

    [BsonElement("owner")]
    public required string Owner { get; set; }

    [BsonElement("likes")]
    public List<string> Likes { get; set; }

    [BsonElement("dislikes")]
    public List<string> Dislikes { get; set; }

    [BsonElement("views")]
    public int Views { get; set; }

    [BsonElement("comments")]
    public required List<string[]> Comments { get; set; }

    [BsonElement("published")]
    public bool Published { get; set; }

    [BsonElement("publishedAt")]
    public required int[] PublishedAt { get; set; }
}