using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Custom", menuName = "ScriptableObject/Custom", order = 6)]
   public class CustomVariables: ScriptableObject
    {
       public string name;
       public List<int> list;
    }
}
