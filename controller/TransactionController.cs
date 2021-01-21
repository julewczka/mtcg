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
            if (user?.Id == null) return ResponseTypes.Unauthorized;
            
            //TODO: possible responses: Not enough money, No package available
            return StackRepository.BuyPackage(user.Id) ? ResponseTypes.HttpOk : ResponseTypes.BadRequest;
        }
    }
}