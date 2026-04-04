namespace TheGuilty.Core.VisualEffects
{
	public interface IPostProcessEffect
	{
		void Play();
		void Stop();
		bool IsPlaying { get; }
	}
}
