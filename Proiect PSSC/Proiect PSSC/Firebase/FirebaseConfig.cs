using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Firebase
{
    public static class FirebaseConfig
    {
        private static FirebaseClient firebaseClient;

        public static FirebaseClient GetFirebaseClient()
        {
            if (firebaseClient == null)
            {
                firebaseClient = new FirebaseClient("https://proiect-pssc-default-rtdb.europe-west1.firebasedatabase.app", new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult("AIzaSyBbgw9xqE6DbnoV2AF9IU3eQwTeuI8nmCw")
                });
            }

            return firebaseClient;
        }
    }
}
