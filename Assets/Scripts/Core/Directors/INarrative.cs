namespace TheGuilty.Core.Directors
{
	public interface INarrative
	{
		void Start();
		void Update();
		void End();
		bool IsComplete { get; }
	}
}