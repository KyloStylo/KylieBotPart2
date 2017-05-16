using System;

namespace KylieBot.Models
{
    [Serializable]
    public class User
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Token { get; set; }

        public bool WantsToBeAuthenticated { get; set; }
    }
}