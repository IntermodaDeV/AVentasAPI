using System;

namespace AventasApi.Models.Authentication
{
    public class UserAuthenticated
    {
        public int Id { get; set; }
        public string UserAccount { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsAdmin { get; set; }
    }
}