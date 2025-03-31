using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ClassRoomVR
{
    [Serializable]
    public class PlayerData
    {
        public string Time;
        public Dictionary<string, object> Parameters;

        public PlayerData(Dictionary<string, object> parameters)
        {
            Time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Parameters = parameters;
        }
    }

    [Serializable]
    public class GameData
    {
        public Dictionary<string, List<PlayerData>> Players;

        public GameData()
        {
            Players = new Dictionary<string, List<PlayerData>>();
        }

        public void AddEntry(string playerId, Dictionary<string, object> parameters)
        {
            if (!Players.ContainsKey(playerId))
                Players[playerId] = new List<PlayerData>();

            Players[playerId].Add(new PlayerData(parameters));
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(Players, true); // Convierte directamente el diccionario
        }
    }


}

