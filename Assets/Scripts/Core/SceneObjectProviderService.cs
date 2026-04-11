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
		[SerializeField] private MirrorObserver _mirror;

		public Mannequin Mannequin => _mannequin;
		public Transform Phone => _phone;
		public EventMannequinTransformHolder TransformHolder => _transformHolder;
		public PlayerManager Player => _player;
		public MirrorObserver MirrorObserver => _mirror;
		public Camera PlayerCamera => _player != null ? _player.MainCamera : null;

		public void Initialize()
		{

		}
	}
}