using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class TransactionController
    {
        private static readonly object TransactionLock = new object();

        public static Response StartTransaction(string resource, User user)
        {
            return resource switch
            {
                "packages" => BuyPackage(user),
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response BuyPackage(User user)
        {
            lock (TransactionLock)
            {
                return user?.Id == null ? RTypes.Unauthorized : StackRepository.BuyPackage(user.Id);
            }
        }
    }
}