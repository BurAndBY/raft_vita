public class RemoveResult
{
	public bool succeded;

	public bool fullyRemoved;

	public Slot slot;

	public RemoveResult()
	{
	}

	public RemoveResult(bool succeded, bool fullyRemoved, Slot slot)
	{
		this.succeded = succeded;
		this.slot = slot;
		this.fullyRemoved = fullyRemoved;
	}
}
