using System;
using System.Runtime.Serialization;

namespace peer10
{
    [DataContract]
    public class Item
    {
        [DataMember]
        // Name of the item.
        public string Name { get; set; }
        [DataMember]
        // Full path to item.
        public string Path { get; set; }
        [DataMember]
        // Articul of item.
        public string Code { get; set; }
        [DataMember]
        // Count of item.
        public int Count { get; set; }
        [DataMember]
        // Price of item.
        public double Price { get; set; }
        [DataMember]
        // Show if item is folder.
        public bool IsFolder { get; set; }
        [DataMember]
        // The sort code for the folder.
        public string SortCode { get; set; } = "";
        // Level of depth in any folder.
        public int Level { get; set; }
    }
}