namespace TheGuilty.Core.Audio
{
	public interface IMusicAudioService : IAudioService
	{
		void Play(AudioMaterial material, bool loop);
		void Stop();
	}
}
