using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace BennyKok.NotionAPI
{
    public class NotionAsync : MonoBehaviour
    {
        //secret_b5JDoyw1hpTiF6NLIsjvzhewY7Pr4yEvbSMJ3Y5DHtE
        private IEnumerator Start()
        {
            var api = new NotionAPI(apiKey);

            yield return api.GetDatabase<CardDatabaseProperties>(database_id, (db) =>
            {
                Debug.Log(db.id);
                Debug.Log(db.created_time);
                Debug.Log(db.title.First().text.content);
            });

            yield return api.QueryDatabaseJSON(database_id, (db) =>
            {
                Debug.Log(db);
            });
        }

        // For type parsing the db Property with JsonUtility
        [Serializable]
        public class CardDatabaseProperties
        {
            public MultiSelectProperty Tags;
            public TitleProperty Name;
        }
    }

}
