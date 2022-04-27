public class AddResult
{
	public bool succeded;

	public Slot slot;

	public AddResult()
	{
	}

	public AddResult(bool succeded, Slot slot)
	{
		this.succeded = succeded;
		this.slot = slot;
	}
}
