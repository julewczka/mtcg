using mtcg.repositories;

namespace mtcg.controller
{
    public static class TransactionController
    {

        public static Response StartTransaction(string resource, string token)
        {
            return resource switch
            {
                "packages" => BuyPackage(token),
                _ => ResponseTypes.NotFoundRequest
            };
        }
        
        private static Response BuyPackage(string token)
        {
            var user = UserRepository.SelectUserByToken(token);
            return user?.Id == null ? ResponseTypes.Unauthorized : StackRepository.BuyPackage(user.Id);
            
        }
    }
}