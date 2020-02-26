using System.Collections.Generic;
using System.Threading.Tasks;
using Karenia.TegamiHato.Server.Models;
using Karenia.TegamiHato.Server.Services;

namespace Karenia.TegamiHato.Server.Views
{
    public class LoginCodeEmailModel
    {
        public LoginCodeEmailModel(User user, UserLoginCode code)
        {
            User = user;
            Code = code;
        }

        public User User { get; }
        public UserLoginCode Code { get; }
    }


}
