using Chapter3.DataObjects;
using System.Data.Entity;
using System.Linq;

namespace Chapter3.Extensions
{
    public static class MessageExtensions
    {
        public static IQueryable<Message> OwnedByFriends(this IQueryable<Message> query, DbSet<Friend> friends, string userId)
        {
            var myPosts = from m in query
                          let fr = (from f in friends where f.FriendId == userId select f.UserId)
                          where m.UserId == userId || fr.Contains(m.UserId)
                          select m;
            return myPosts;
        }
    }
}