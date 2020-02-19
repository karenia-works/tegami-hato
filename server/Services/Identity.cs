using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Karenia.TegamiHato.Server.Services
{
    public class IdentityConstants
    {
        public static readonly ICollection<Client> clients = new[]{
            new Client(){
                AccessTokenType = AccessTokenType.Jwt,
                ClientId = "WebClient",
                ClientName = "WebClient",
                ClientSecrets = {
                    new Secret("WebClientPublic".Sha256())
                },
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                AllowedScopes = {
                    "api"
                },
            },
        };

        public static readonly ICollection<ApiResource> apiResources = new[]{
            new ApiResource("api"),
        };
    }

    public class UserIdentityService : IResourceOwnerPasswordValidator
    {
        private readonly DatabaseService db;

        public UserIdentityService(DatabaseService db)
        {
            this.db = db;
        }

        // Note:
        // This validation uses a dynamic password sent to the user via email 
        // instead of a regular password.
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {

            if (await db.CanUserLoginWithCode(context.UserName, context.Password))
            {
                // We have guarantee it will have value
                Ulid id = (await db.GetUserIdFromEmail(context.UserName)).Value;

                context.Result = new GrantValidationResult(
                    subject: id.ToString(),
                    authenticationMethod: "custom",
                    claims: new Claim[]
                    {

                    }
                );
            }
            else
            {
                context.Result = new GrantValidationResult(
                   TokenRequestErrors.InvalidGrant,
                   "Bad passcode");
            }
        }
    }
}
