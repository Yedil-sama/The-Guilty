using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Runtime;

namespace SojaExiles

{
	public class ClosetopencloseDoor : MonoBehaviour
	{

		public Animator Closetopenandclose;
		public bool open;
		public Transform Player;
		public float InteractionDistance = 15f;
		public string OpenAnimation = "ClosetOpening";
		public string CloseAnimation = "ClosetClosing";

		void Start()
		{
			open = false;
			TryAssignPlayer();
		}

		private void TryAssignPlayer()
		{
			if (Player != null)
				return;

			try
			{
				Player = PlayerManager.Instance.transform;
			}
			catch
			{
				// no player manager available yet.
			}
		}

		void OnMouseOver()
		{
			if (Player == null)
				TryAssignPlayer();

			if (Player == null)
				return;

			float dist = Vector3.Distance(Player.position, transform.position);
			if (dist > InteractionDistance)
				return;

			if (!InputManager.ReadButtonOnce(this, Controls.USE))
			{
				if (!open)
					StartCoroutine(opening());
				else
					StartCoroutine(closing());
			}
		}

		IEnumerator opening()
		{
			if (Closetopenandclose == null)
				yield break;

			print("you are opening the door");
			Closetopenandclose.Play(OpenAnimation);
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			if (Closetopenandclose == null)
				yield break;

			print("you are closing the door");
			Closetopenandclose.Play(CloseAnimation);
			open = false;
			yield return new WaitForSeconds(.5f);
		}


	}
}