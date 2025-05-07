using System.IO;
using System.Numerics;

namespace CapRecAnimConv
{
	public class CapturedAnimation
	{
		public string OriginalBin { get; set; }

		public int Version { get; set; }
		public string ObjectHSName { get; set; }
		public int TagGroup { get; set; }
		public string TagName { get; set; }
		public int NodeChecksum { get; set; }
		public int NodeCount { get; set; }
		public int TickCount { get; set; }

		public List<MapNodeInfo> ObjectNodes { get; set; }
		public List<TickInfo> Ticks { get; set; }

		public List<ProcessedTickInfo> ProcessedTicks { get; set; }

		public CapturedAnimation(string path)
		{
			ProcessedTicks = new List<ProcessedTickInfo>();

			using (FileStream fs = new FileStream(path, FileMode.Open))
			{
				using (BinaryReader br = new BinaryReader(fs))
				{
					OriginalBin = path;

					Version = br.ReadInt32();
					if (Version > 1)
						throw new ArgumentOutOfRangeException("invalid bin version!");

					ObjectHSName = br.ReadStringToNull(32);

					TagGroup = br.ReadInt32();
					TagName = br.ReadStringToNull(256);

					NodeChecksum = br.ReadInt32();
					NodeCount = br.ReadInt32();

					ObjectNodes = new List<MapNodeInfo>();

					for (int i = 0; i < NodeCount; i++)
					{
						string nodeName = br.ReadStringToNull(32);
						int sibIndex = br.ReadInt16();
						int chilIndex = br.ReadInt16();
						int parIndex = br.ReadInt16();

						MapNodeInfo node = new MapNodeInfo(nodeName, parIndex, sibIndex, chilIndex);

						ObjectNodes.Add(node);
					}

					TickCount = br.ReadInt32();

					Ticks = new List<TickInfo>();

					for (int t = 0; t < TickCount; t++)
					{
						TickInfo tick = new TickInfo();

						tick.Flags = (ControlFlags)br.ReadUInt32();

						tick.ObjectPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

						float tl11 = br.ReadSingle();
						float tl12 = br.ReadSingle();
						float tl13 = br.ReadSingle();

						float tl21 = br.ReadSingle();
						float tl22 = br.ReadSingle();
						float tl23 = br.ReadSingle();

						Vector3 fw = new Vector3(tl11, tl12, tl13);
						Vector3 up = new Vector3(tl21, tl22, tl23);

						tick.ObjectRotation = CreateQuaternion(fw, up);

						tick.Nodes = new List<TickNodeInfo>();

						for (int n = 0; n < NodeCount; n++)
						{
							float ni = br.ReadSingle();
							float nj = br.ReadSingle();
							float nk = br.ReadSingle();
							float nw = br.ReadSingle();

							var rot = new Quaternion(ni, nj, nk, nw);

							var tr = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

							var sc = br.ReadSingle();

							tick.Nodes.Add(new TickNodeInfo(rot, tr, sc));
						}

						Ticks.Add(tick);
					}
				}
			}
		}

		public void ApplyMods(PositionModType posMod, RotationModType rotMod)
		{
			ApplyMods(posMod, rotMod, Vector3.Zero);
		}

		public void ApplyMods(PositionModType posMod, RotationModType rotMod, Vector3 customPos)
		{
			foreach (TickInfo tick in Ticks)
			{
				ProcessedTickInfo proc = new ProcessedTickInfo();

				for (int i = 0; i < tick.Nodes.Count; i++)
				{
					var node = tick.Nodes[i];

					Vector3 workingTranslation = new Vector3(node.Translation.X, node.Translation.Y, node.Translation.Z);
					Quaternion workingRotation = Quaternion.Normalize(new Quaternion(node.Rotation.X, node.Rotation.Y, node.Rotation.Z, node.Rotation.W));

					if (posMod == PositionModType.ObjectXYZ)
						workingTranslation += tick.ObjectPosition;
					else if (posMod == PositionModType.ObjectXY)
						workingTranslation += new Vector3(tick.ObjectPosition.X, tick.ObjectPosition.Y, 0);
					else if (posMod == PositionModType.Custom)
						workingTranslation += customPos;

					if (i == 0 && rotMod == RotationModType.Object)
						workingRotation *= Quaternion.Normalize(tick.ObjectRotation);

					proc.Nodes.Add(new TickNodeInfo(workingRotation, workingTranslation, node.Scale));
				}

				ProcessedTicks.Add(proc);
			}
		}

		public bool WriteJMA(string path)
		{
			if (ProcessedTicks == null || ProcessedTicks.Count == 0)
				return false;

			using (StringWriter sw = new StringWriter())
			{
				sw.WriteLine("16392");
				sw.WriteLine(TickCount);
				sw.WriteLine("30");
				sw.WriteLine("1");
				sw.WriteLine(ObjectHSName);
				sw.WriteLine(NodeCount);
				sw.WriteLine(NodeChecksum);

				foreach (MapNodeInfo info in ObjectNodes)
				{
					sw.WriteLine(info.Name);
					sw.WriteLine(info.ChildIndex);
					sw.WriteLine(info.SiblingIndex);
				}

				foreach (var tick in ProcessedTicks)
				{
					foreach (var node in tick.Nodes)
					{
						Vector3 scaledTr = node.Translation * 100f;

						sw.WriteLine(FormattableString.Invariant($"{scaledTr.X:0.0000000000}\t{scaledTr.Y:0.0000000000}\t{scaledTr.Z:0.0000000000}"));
						sw.WriteLine(FormattableString.Invariant($"{node.Rotation.X:0.0000000000}\t{node.Rotation.Y:0.0000000000}\t{node.Rotation.Z:0.0000000000}\t{node.Rotation.W:0.0000000000}"));
						sw.WriteLine(FormattableString.Invariant($"{node.Scale:0.0000000000}"));
					}
				}

				File.WriteAllText(path, sw.ToString());
			}

			return true;
		}

		public bool WriteTXT(string path, bool rotation = true)
		{
			using (StringWriter sw = new StringWriter())
			{
				sw.WriteLine(TickCount);

				foreach (TickInfo tick in Ticks)
				{
					sw.WriteLine(FormattableString.Invariant($"{tick.ObjectPosition.X:0.0000000000}\t{tick.ObjectPosition.Y:0.0000000000}\t{tick.ObjectPosition.Z:0.0000000000}"));

					if (rotation)
					{
						var normRot = Quaternion.Normalize(tick.ObjectRotation);
						sw.WriteLine(FormattableString.Invariant($"{normRot.X:0.0000000000}\t{normRot.Y:0.0000000000}\t{normRot.Z:0.0000000000}\t{normRot.W:0.0000000000}"));
					}

					File.WriteAllText(path, sw.ToString());

				}
			}

			return true;
		}

		public static Quaternion CreateQuaternion(Vector3 forward, Vector3 up)
		{
			forward = Vector3.Normalize(forward);
			up = Vector3.Normalize(up);

			Vector3 side = Vector3.Cross(up, forward);
			side = Vector3.Normalize(side);

			Matrix4x4 rotationMatrix = new Matrix4x4(
				forward.X, side.X, up.X, 0,
				forward.Y, side.Y, up.Y, 0,
				forward.Z, side.Z, up.Z, 0,
				0, 0, 0, 1);

			return Quaternion.CreateFromRotationMatrix(rotationMatrix);
		}
	}

	public class MapNodeInfo
	{
		public string Name { get; set; }
		public int ParentIndex { get; set; }
		public int SiblingIndex { get; set; }
		public int ChildIndex { get; set; }

		public MapNodeInfo(string name, int parentIndex,  int siblingIndex, int childIndex)
		{
			Name = name;
			ParentIndex = parentIndex;
			SiblingIndex = siblingIndex;
			ChildIndex = childIndex;
		}
	}

	public class TickInfo
	{
		public ControlFlags Flags { get; set; }

		public Vector3 ObjectPosition { get; set; }

		public Quaternion ObjectRotation { get; set; }

		public List<TickNodeInfo> Nodes { get; set; }

		public TickInfo()
		{
			Nodes = new List<TickNodeInfo>();
		}
	}

	public class TickNodeInfo
	{
		public Quaternion Rotation { get; set; }

		public Vector3 Translation { get; set; }

		public float Scale { get; set; }

		public TickNodeInfo(Quaternion rotation, Vector3 translation, float scale)
		{
			Rotation = rotation;
			Translation = translation;
			Scale = scale;
		}
	}

	public class ProcessedTickInfo
	{
		public List<TickNodeInfo> Nodes { get; set; }

		public ProcessedTickInfo()
		{
			Nodes = new List<TickNodeInfo>();
		}
	}
}
