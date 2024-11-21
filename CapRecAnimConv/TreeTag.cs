
namespace CapRecAnimConv
{
	public class TreeTag
	{
		public TreeItemType Type { get; set; }
		public string Path { get; set; }
		public string Name { get; set; }

		public TreeTag(TreeItemType type, string path, string name)
		{
			Type = type;
			Path = path;
			Name = name;
		}
	}
}
