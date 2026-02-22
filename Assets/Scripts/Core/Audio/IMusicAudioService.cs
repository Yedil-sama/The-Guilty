using System.Collections;

namespace TheGuilty.Core.Audio
{
	public interface IMusicAudioService : IAudioService
	{
		void Play(AudioMaterial material, bool loop);
		void Stop();

		IEnumerator FadeOut(float duration);
		IEnumerator FadeIn(float duration);
	}
}
