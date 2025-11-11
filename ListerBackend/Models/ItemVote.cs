using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class ItemVote
{
    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("votes")]
    public int Votes { get; set; }
}