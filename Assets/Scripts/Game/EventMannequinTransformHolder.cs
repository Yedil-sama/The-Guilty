using System.Collections.Generic;
using UnityEngine;

namespace TheGuilty.Game
{
    public class EventMannequinTransformHolder : MonoBehaviour
    {
        [System.Serializable]
        public class EventPositions
        {
            public string eventName;
            public List<Transform> positions = new List<Transform>();
        }

        [SerializeField] private List<EventPositions> _events = new List<EventPositions>();

        public Transform GetRandomPositionForEvent(string eventName)
        {
            EventPositions eventPos = _events.Find(e => e.eventName == eventName);
            if (eventPos != null && eventPos.positions.Count > 0)
            {
                int randomIndex = Random.Range(0, eventPos.positions.Count);
                return eventPos.positions[randomIndex];
            }
            return null;
        }

        public List<Transform> GetAllPositionsForEvent(string eventName)
        {
            EventPositions eventPos = _events.Find(e => e.eventName == eventName);
            return eventPos?.positions ?? new List<Transform>();
        }
    }
}