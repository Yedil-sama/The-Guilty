namespace TheGuilty.Core.Audio
{
	public interface IVoiceAudioService : IAudioService
	{
		void Play(AudioMaterial material);
		void Stop();
		bool IsPlaying { get; }
	}
}
