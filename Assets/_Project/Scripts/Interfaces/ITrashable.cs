public interface ITrashable
{
    public bool CanTrash { get; }

    public void TrashObject();
}