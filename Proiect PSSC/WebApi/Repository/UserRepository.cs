using Firebase.Database;
using Firebase.Database.Query;
using Proiect_PSSC.Firebase;

namespace WebApi.Repository
{
    public class UserRepository
    {
        private FirebaseClient client;

        public UserRepository()
        {
            this.client = FirebaseConfig.GetFirebaseClient();
        }

        public async Task<bool> CheckClientExists(string clientId)
        {
            var result = await client.Child("Users").Child(clientId).OnceSingleAsync<UserResponse>();

            return result != null;
        }

        public class UserResponse
        {
            public string Password { get; set; }
        }
    }
}
