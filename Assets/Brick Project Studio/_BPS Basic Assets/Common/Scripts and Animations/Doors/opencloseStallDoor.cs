using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Runtime;

namespace SojaExiles

{
	public class opencloseStallDoor : MonoBehaviour
	{

		public Animator openandclose;
		public bool open;
		public Transform Player;
		public float InteractionDistance = 15f;

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
				// PlayerManager may not be initialized yet.
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
				return;

			if (!open)
			{
				StartCoroutine(opening());
			}
			else
			{
				StartCoroutine(closing());
			}
		}

		IEnumerator opening()
		{
			print("you are opening the door");
			openandclose.Play("OpeningStall");
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			openandclose.Play("ClosingStall");
			open = false;
			yield return new WaitForSeconds(.5f);
		}


	}
}