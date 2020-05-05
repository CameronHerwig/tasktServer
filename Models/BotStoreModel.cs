using System;
using System.ComponentModel.DataAnnotations;

namespace tasktServer.Models
{
    public class BotStoreModel
    {
        [Key]
        public Guid StoreID { get; set; }
        public string BotStoreName { get; set; }
        public string BotStoreValue { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public Guid LastUpdatedBy { get; set; }
    }
}
