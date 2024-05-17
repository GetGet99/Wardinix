namespace Wardininx.UndoRedos;
public partial class UndoManager
{
	readonly Stack<UndoableActionGroup> Groups = new();
	public partial UndoableActionGroup OpenGroup(string actionName)
	{
		UndoableActionGroup group = new(actionName, new());
		AddAction(group);
		// set the group as the current active group
		Groups.Push(group);
		return group;
	}
}