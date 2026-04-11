using TheGuilty.Game;
using UHFPS.Runtime;
using UnityEngine;

namespace TheGuilty.Core
{
	public sealed class SceneObjectProviderService : MonoBehaviour, IService
	{
		[SerializeField] private Mannequin _mannequin;
		[SerializeField] private Transform _phone;
		[SerializeField] private EventMannequinTransformHolder _transformHolder;
		[SerializeField] private PlayerManager _player;

		public Mannequin Mannequin => _mannequin;
		public Transform Phone => _phone;
		public EventMannequinTransformHolder TransformHolder => _transformHolder;
		public PlayerManager Player => _player;
		public Camera PlayerCamera => _player != null ? _player.MainCamera : null;

		public void Initialize()
		{
			if (_mannequin == null)
			{
				Debug.Log("SceneObjectProviderService: Mannequin not assigned in inspector.");
			}
			if (_phone == null)
			{
				Debug.Log("SceneObjectProviderService: Phone transform not assigned in inspector.");
			}
			if (_transformHolder == null)
			{
				Debug.Log("SceneObjectProviderService: EventMannequinTransformHolder not assigned in inspector.");
			}
			if (_player == null)
			{
				Debug.Log("SceneObjectProviderService: PlayerManager not assigned in inspector.");
			}
		}
	}
}