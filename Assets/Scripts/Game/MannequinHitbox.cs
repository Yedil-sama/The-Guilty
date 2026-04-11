using UnityEngine;

namespace TheGuilty.Game
{
	public class MannequinHitbox : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{

			}
		}
	}
}