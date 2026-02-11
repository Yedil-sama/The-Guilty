using TheGuilty.Core.Audio.Sfx;

namespace TheGuilty.Core.Audio
{
	public interface ISfxAudioService : IAudioService
	{
		void Play(SfxAudioMaterial material);
	}
}
